using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

using UdonRabbit.Analyzer.Abstractions;
using UdonRabbit.Analyzer.Extensions;
using UdonRabbit.Analyzer.Udon;

namespace UdonRabbit.Analyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MethodSpecifyForSendCustomEventIsNotFoundInBehaviourCodeFixProvider))]
    [Shared]
    public class MethodSpecifyForSendCustomEventIsNotFoundInBehaviourCodeFixProvider : CodeFixProviderBase
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(MethodSpecifyForSendCustomEventIsNotFoundInBehaviour.ComponentId);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf<InvocationExpressionSyntax>(root, context.Span, out var invocation))
                return;

            if (invocation.Expression is MemberAccessExpressionSyntax expression)
            {
                var model = await context.Document.GetSemanticModelAsync().ConfigureAwait(false);

                var receiver = expression.Expression;
                var t = model.GetTypeInfo(receiver);
                var thisClass = invocation.FirstAncestorOrSelf<ClassDeclarationSyntax>();
                var symbol = model.GetDeclaredSymbol(thisClass);

                if (t.Type.Equals(symbol, SymbolEqualityComparer.Default))
                {
                    var document = context.Document;
                    var diagnostic = context.Diagnostics[0];
                    var name = GetMissingMethodName(invocation, model);

                    var action = CreateCodeAction(CodeFixResources.URA0045CodeFixTitle, ct => CreateNewEmptyMethodInSameDocumentAsync(document, invocation, name, ct), diagnostic.Id, name);
                    context.RegisterCodeFix(action, diagnostic);
                }
                else
                {
                    // create to another class
                    var declared = t.Type.Locations.FirstOrDefault();
                    if (declared == null)
                        return; // MAYBE UNREACHABLE

                    var document = context.Document.Project.Solution.GetDocument(declared.SourceTree);
                    if (document == null)
                        return; // MAYBE UNREACHABLE

                    var diagnostic = context.Diagnostics[0];
                    var name = GetMissingMethodName(invocation, model);

                    var action = CreateCodeAction(CodeFixResources.URA0045CodeFixTitle, ct => CreateNewEmptyMethodInAnotherDocumentAsync(document, declared.SourceSpan, name, ct), diagnostic.Id, name);
                    context.RegisterCodeFix(action, diagnostic);
                }
            }
            else
            {
                var document = context.Document;
                var diagnostic = context.Diagnostics[0];
                var name = GetMissingMethodName(invocation, await document.GetSemanticModelAsync(CancellationToken.None).ConfigureAwait(false));

                var action = CreateCodeAction(CodeFixResources.URA0045CodeFixTitle, ct => CreateNewEmptyMethodInSameDocumentAsync(document, invocation, name, ct), diagnostic.Id, name);
                context.RegisterCodeFix(action, diagnostic);
            }
        }

        private static async Task<Document> CreateNewEmptyMethodInSameDocumentAsync(Document document, InvocationExpressionSyntax invocation, string missingMethodName, CancellationToken cancellationToken)
        {
            var oldNode = invocation.FirstAncestorOrSelf<ClassDeclarationSyntax>();

            var sb = new StringBuilder();
            sb.Append("public void ").Append(missingMethodName).AppendLine("()");
            sb.AppendLine("{");
            sb.AppendLine("}");

            if (oldNode.Members.Count > 0)
                sb.Insert(0, Environment.NewLine);

            var newNode = oldNode.AddMembers(SyntaxFactory.ParseMemberDeclaration(sb.ToString())).WithAdditionalAnnotations(Formatter.Annotation);

            return await document.ReplaceNodeAsync(oldNode, newNode, cancellationToken).ConfigureAwait(false);
        }

        private static async Task<Solution> CreateNewEmptyMethodInAnotherDocumentAsync(Document document, TextSpan span, string missingMethodName, CancellationToken cancellationToken)
        {
            var oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (oldRoot.FindNode(span) is not ClassDeclarationSyntax oldNode)
                return document.Project.Solution;

            var sb = new StringBuilder();
            sb.Append("public void ").Append(missingMethodName).AppendLine("()");
            sb.AppendLine("{");
            sb.AppendLine("}");

            if (oldNode.Members.Count > 0)
                sb.Insert(0, Environment.NewLine);

            var newNode = oldNode.AddMembers(SyntaxFactory.ParseMemberDeclaration(sb.ToString())).WithAdditionalAnnotations(Formatter.Annotation);
            var newRoot = oldRoot.ReplaceNode(oldNode, newNode);

            return document.Project.Solution.WithDocumentSyntaxRoot(document.Id, newRoot);
        }

        private static string GetMissingMethodName(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
        {
            var s = semanticModel.GetSymbolInfo(invocation);
            if (s.Symbol is not IMethodSymbol m)
                return string.Empty; // UNREACHABLE

            var i = UdonConstants.UdonCustomMethodInvokers.First(w => w.Item1 == m.Name).Item2;
            var arg = invocation.ArgumentList.Arguments.ElementAtOrDefault(i);
            if (arg == null)
                return string.Empty; // UNREACHABLE

            return arg.Expression.ParseValue();
        }
    }
}