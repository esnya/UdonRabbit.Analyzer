using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;

using UdonRabbit.Analyzer.Abstractions;
using UdonRabbit.Analyzer.Extensions;
using UdonRabbit.Analyzer.Udon;

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

            var oldName = symbol.Name;
            var newName = symbol.Name;
            while (newName.StartsWith("_"))
                newName = newName.Substring(1);

            if (string.IsNullOrWhiteSpace(newName))
                return solution;

            var newSolution = await Renamer.RenameSymbolAsync(solution, symbol, newName, solution.Workspace.Options, cancellationToken).ConfigureAwait(false);
            var newDocument = newSolution.GetDocument(document.Id);
            if (newDocument == null)
                return newSolution; // MAYBE UNREACHABLE

            var newSemanticModel = await newDocument.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var newDeclaration = await newDocument.FindEquivalentNodeAsync(declaration.WithIdentifier(SyntaxFactory.Identifier(newName)), cancellationToken).ConfigureAwait(false);
            var finalDocument = await RenameSpecifiedByStringLiteral(newDocument, newSemanticModel, newDeclaration, oldName, newName, cancellationToken).ConfigureAwait(false);
            var newSyntaxRoot = await finalDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            return newSolution.WithDocumentSyntaxRoot(document.Id, newSyntaxRoot);
        }

        private static async Task<Document> RenameSpecifiedByStringLiteral(Document document, SemanticModel semanticModel, MethodDeclarationSyntax oldDeclaration, string oldName, string newName, CancellationToken cancellationToken)
        {
            var invocations = oldDeclaration.ScanMethodCallers(semanticModel, UdonMethodInvoker.IsNetworkInvokerMethod).Select(w => new UdonMethodInvoker(w));
            var callers = invocations.Where(w => w.GetTargetMethodName() == oldName);

            ArgumentSyntax ReplaceStringLiteral(ArgumentSyntax arg, bool replace)
            {
                if (!replace)
                    return arg;
                if (arg.Expression.IsKind(SyntaxKind.StringLiteralExpression))
                    return arg.WithExpression(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(newName)));
                return arg;
            }

            foreach (var caller in callers)
            {
                var argumentAt = caller.GetArgumentAt();
                var oldNode = caller.Node.ArgumentList;
                var arguments = oldNode.Arguments.Select((w, i) => ReplaceStringLiteral(w, i == argumentAt));
                var newNode = oldNode.WithArguments(SyntaxFactory.SeparatedList(arguments));

                document = await document.ReplaceNodeAsync(oldNode, newNode, cancellationToken).ConfigureAwait(false);
            }

            return document;
        }
    }
}