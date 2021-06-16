using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using UdonRabbit.Analyzer.Abstractions;
using UdonRabbit.Analyzer.Extensions;

namespace UdonRabbit.Analyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MethodCalledByCustomNetworkEventMustBePublicCodeFixProvider))]
    [Shared]
    public class MethodCalledByCustomNetworkEventMustBePublicCodeFixProvider : CodeFixProviderBase
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(MethodCalledByCustomNetworkEventMustBePublic.ComponentId);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf<MethodDeclarationSyntax>(root, context.Span, out var declaration))
                return;

            var document = context.Document;
            var diagnostic = context.Diagnostics[0];
            var action = CreateCodeAction(CodeFixResources.URA0041CodeFixTitie, ct => MakeMethodAsPublic(document, declaration, ct), diagnostic.Id);
            context.RegisterCodeFix(action, diagnostic);
        }

        private static async Task<Document> MakeMethodAsPublic(Document document, MethodDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            var oldNode = declaration.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            var newNode = oldNode.WithModifiers(SyntaxTokenList.Create(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));

            return await document.ReplaceNode(oldNode, newNode, cancellationToken).ConfigureAwait(false);
        }
    }
}