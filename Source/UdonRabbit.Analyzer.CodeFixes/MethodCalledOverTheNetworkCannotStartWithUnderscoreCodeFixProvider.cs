using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;

using UdonRabbit.Analyzer.Abstractions;

namespace UdonRabbit.Analyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MethodCalledOverTheNetworkCannotStartWithUnderscoreCodeFixProvider))]
    public class MethodCalledOverTheNetworkCannotStartWithUnderscoreCodeFixProvider : CodeFixProviderBase
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(MethodCalledOverTheNetworkCannotStartWithUnderscore.ComponentId);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf<MethodDeclarationSyntax>(root, context.Span, out var declaration))
                return;

            var document = context.Document;
            var diagnostic = context.Diagnostics[0];
            var action = CreateCodeAction(CodeFixResources.URA0043CodeFixTitle, ct => MakeTheMethodCallableOverTheNetwork(document, declaration, ct), diagnostic.Id);
            context.RegisterCodeFix(action, diagnostic);
        }

        private static async Task<Solution> MakeTheMethodCallableOverTheNetwork(Document document, MethodDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            var solution = document.Project.Solution;
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var symbol = semanticModel.GetDeclaredSymbol(declaration, cancellationToken);

            var newName = symbol.Name;
            while (newName.StartsWith("_"))
                newName = newName.Substring(1);

            if (string.IsNullOrWhiteSpace(newName))
                return solution;

            return await Renamer.RenameSymbolAsync(solution, symbol, newName, solution.Workspace.Options, cancellationToken).ConfigureAwait(false);
        }
    }
}