using System.Collections.Generic;
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

namespace UdonRabbit.Analyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MethodCalledOverTheNetworkCannotStartWithUnderscoreCodeFixProvider))]
    public class MethodCalledOverTheNetworkCannotStartWithUnderscoreCodeFixProvider : CodeFixProviderBase
    {
        private static readonly HashSet<(string, int)> ScannedMethodList = new()
        {
            ("SendCustomNetworkEvent", 1)
        };

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
            var @class = oldDeclaration.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            var invocations = @class.DescendantNodes()
                                    .OfType<InvocationExpressionSyntax>()
                                    .Select(w => (Info: semanticModel.GetSymbolInfo(w), Syntax: w))
                                    .Where(w =>
                                    {
                                        if (w.Info.Symbol is not IMethodSymbol symbol)
                                            return false;
                                        return ScannedMethodList.Any(v => v.Item1 == symbol.Name);
                                    })
                                    .ToList();

            var callers = invocations.Where(w =>
            {
                var i = ScannedMethodList.First(v => v.Item1 == w.Info.Symbol.Name).Item2;
                var arg = w.Syntax.ArgumentList.Arguments.ElementAtOrDefault(i);
                if (arg == null)
                    return false;

                return arg.Expression.ParseValue() == oldName;
            }).ToList();

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
                var node = caller.Syntax;
                var argumentAt = ScannedMethodList.First(w => w.Item1 == caller.Info.Symbol.Name).Item2;
                var oldNode = node.ArgumentList;
                var arguments = oldNode.Arguments.Select((w, i) => ReplaceStringLiteral(w, i == argumentAt));
                var newNode = oldNode.WithArguments(SyntaxFactory.SeparatedList(arguments));

                document = await document.ReplaceNodeAsync(oldNode, newNode, cancellationToken).ConfigureAwait(false);
            }

            return document;
        }
    }
}