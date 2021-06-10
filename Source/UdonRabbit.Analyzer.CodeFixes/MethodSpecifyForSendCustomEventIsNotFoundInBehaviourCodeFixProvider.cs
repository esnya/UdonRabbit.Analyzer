using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

using UdonRabbit.Analyzer.Extensions;

namespace UdonRabbit.Analyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MethodSpecifyForSendCustomEventIsNotFoundInBehaviourCodeFixProvider))]
    [Shared]
    public class MethodSpecifyForSendCustomEventIsNotFoundInBehaviourCodeFixProvider : CodeFixProvider
    {
        private static readonly HashSet<(string, int)> ScannedMethodLists = new()
        {
            ("SendCustomEvent", 0),
            ("SendCustomNetworkEvent", 1),
            ("SendCustomEventDelayedSeconds", 0),
            ("SendCustomEventDelayedFrames", 0)
        };

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(MethodSpecifyForSendCustomEventIsNotFoundInBehaviour.ComponentId);

        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf<InvocationExpressionSyntax>(root, context.Span, out var invocation))
                return;

            if (invocation.Expression is MemberAccessExpressionSyntax expression)
            {
                var semanticModel = await context.Document.GetSemanticModelAsync().ConfigureAwait(false);

                var receiver = expression.Expression;
                var t = ModelExtensions.GetTypeInfo(semanticModel, receiver);
                var thisClass = invocation.FirstAncestorOrSelf<ClassDeclarationSyntax>();
                var symbol = ModelExtensions.GetDeclaredSymbol(semanticModel, thisClass);

                Debugger.Break();
            }

            var document = context.Document;
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(CodeAction.Create("Create a new empty method", ct => CreateNewEmptyMethodAsync(document, invocation, ct), diagnostic.Id), diagnostic);
        }

        private static bool TryFindFirstAncestorOrSelf<T>(SyntaxNode root, TextSpan span, out T r) where T : SyntaxNode
        {
            r = root.FindNode(span).FirstAncestorOrSelf<T>();

            return r != null;
        }

        private static async Task<Document> CreateNewEmptyMethodAsync(Document document, InvocationExpressionSyntax invocation, CancellationToken cancellationToken)
        {
            var name = GetMissingMethodName(invocation, await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false));

            var oldNode = invocation.FirstAncestorOrSelf<ClassDeclarationSyntax>();

            var sb = new StringBuilder();
            sb.AppendLine($"public void {name}()");
            sb.AppendLine("{");
            sb.AppendLine("}");

            if (oldNode.Members.Count > 0)
                sb.Insert(0, Environment.NewLine);

            var newNode = oldNode.AddMembers(SyntaxFactory.ParseMemberDeclaration(sb.ToString())).WithAdditionalAnnotations(Formatter.Annotation);

            var oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var newRoot = oldRoot.ReplaceNode(oldNode, newNode);

            return document.WithSyntaxRoot(newRoot);
        }

        private static string GetMissingMethodName(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
        {
            var s = semanticModel.GetSymbolInfo(invocation);
            if (s.Symbol is not IMethodSymbol m)
                return string.Empty; // UNREACHABLE

            var i = ScannedMethodLists.First(w => w.Item1 == m.Name).Item2;
            var arg = invocation.ArgumentList.Arguments.ElementAtOrDefault(i);
            if (arg == null)
                return string.Empty; // UNREACHABLE

            return arg.Expression.ParseValue();
        }
    }
}