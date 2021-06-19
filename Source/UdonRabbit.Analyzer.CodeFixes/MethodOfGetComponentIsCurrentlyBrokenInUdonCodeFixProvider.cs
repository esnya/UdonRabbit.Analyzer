using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using UdonRabbit.Analyzer.Abstractions;
using UdonRabbit.Analyzer.Extensions;

namespace UdonRabbit.Analyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MethodOfGetComponentIsCurrentlyBrokenInUdonCodeFixProvider))]
    [Shared]
    public class MethodOfGetComponentIsCurrentlyBrokenInUdonCodeFixProvider : CodeFixProviderBase
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(MethodOfGetComponentIsCurrentlyBrokenInUdon.ComponentId);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            if (!TryFindFirstAncestorOrSelf<InvocationExpressionSyntax>(root, context.Span, out var invocation))
                return;

            var document = context.Document;
            var diagnostic = context.Diagnostics[0];
            var action = CreateCodeAction(CodeFixResources.URA0047CodeFixTitle, ct => UseNonGenericMethodInsteadOfGenericMethod(document, invocation, ct), diagnostic.Id);
            context.RegisterCodeFix(action, diagnostic);
        }

        private async Task<Document> UseNonGenericMethodInsteadOfGenericMethod(Document document, InvocationExpressionSyntax invocation, CancellationToken cancellationToken)
        {
            var oldNode = invocation.FirstAncestorOrSelf<InvocationExpressionSyntax>();

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var symbol = ModelExtensions.GetSymbolInfo(semanticModel, oldNode, cancellationToken);
            if (symbol.Symbol is not IMethodSymbol method)
                return document; // MAYBE UNREACHABLE

            var genericArguments = method.TypeArguments;
            var arguments = oldNode.ArgumentList.Arguments.ToList();

            var typeofExpression = CreateTypeofExpression(semanticModel, oldNode.Span, genericArguments[0]);
            arguments.Insert(0, SyntaxFactory.Argument(typeofExpression));

            var newArguments = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(arguments));
            var newExpression = SyntaxFactory.IdentifierName(((GenericNameSyntax) oldNode.Expression).Identifier);
            var newNode = oldNode.WithArgumentList(newArguments).WithExpression(newExpression);

            return await document.ReplaceNodeAsync(oldNode, newNode, cancellationToken).ConfigureAwait(false);
        }

        private static ExpressionSyntax CreateTypeofExpression(SemanticModel semanticModel, TextSpan span, ITypeSymbol t)
        {
            if (semanticModel.CanReferenceNamedSymbol(span, t.Name))
                return SyntaxFactory.TypeOfExpression(SyntaxFactory.ParseTypeName(t.Name));
            return SyntaxFactory.TypeOfExpression(SyntaxFactory.ParseTypeName(t.ToDisplayString()));
        }
    }
}