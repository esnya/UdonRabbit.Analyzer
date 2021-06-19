using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using UdonRabbit.Analyzer.Abstractions;
using UdonRabbit.Analyzer.Extensions;
using UdonRabbit.Analyzer.Udon;

namespace UdonRabbit.Analyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MethodSpecifyForSendCustomNetworkEventMustBePublicCodeFixProvider))]
    [Shared]
    public class MethodSpecifyForSendCustomNetworkEventMustBePublicCodeFixProvider : CodeFixProviderBase
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(MethodSpecifyForSendCustomNetworkEventMustBePublic.ComponentId);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf<InvocationExpressionSyntax>(root, context.Span, out var invocation))
                return;

            if (invocation.Expression is MemberAccessExpressionSyntax expression)
            {
                var sm = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
                var receiver = expression.Expression;
                var t = ModelExtensions.GetTypeInfo(sm, receiver);
                var document = context.Document.Project.Solution.GetDocument(t.Type.Locations.FirstOrDefault()?.SourceTree);
                if (document == null)
                    return;

                var symbol = ModelExtensions.GetSymbolInfo(sm, invocation);
                var targetMethod = UdonMethodInvoker.GetTargetMethodName(symbol.Symbol, invocation);

                var diagnostic = context.Diagnostics[0];
                var action = CreateCodeAction(CodeFixResources.URA0042CodeFixTitle, ct => MakeMethodAsPublic(document, targetMethod, ct), diagnostic.Id);
                context.RegisterCodeFix(action, diagnostic);
            }
        }

        private static async Task<Solution> MakeMethodAsPublic(Document document, string targetMethod, CancellationToken cancellationToken)
        {
            var declarations = await document.EnumerableNodesAsync<MethodDeclarationSyntax>(w => w.Identifier.ValueText == targetMethod, cancellationToken).ConfigureAwait(false);
            var declaration = declarations.FirstOrDefault(w => w.ParameterList.Parameters.Count == 0);
            if (declaration == null)
                return document.Project.Solution; // MAYBE UNREACHABLE

            var oldNode = declaration;
            var newNode = oldNode.WithModifiers(SyntaxTokenList.Create(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));
            var newDocument = await document.ReplaceNodeAsync(oldNode, newNode, cancellationToken).ConfigureAwait(false);
            var newSyntaxRoot = await newDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            return document.Project.Solution.WithDocumentSyntaxRoot(document.Id, newSyntaxRoot);
        }
    }
}