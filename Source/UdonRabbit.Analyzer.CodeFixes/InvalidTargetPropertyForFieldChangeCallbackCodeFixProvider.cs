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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InvalidTargetPropertyForFieldChangeCallbackCodeFixProvider))]
    [Shared]
    public class InvalidTargetPropertyForFieldChangeCallbackCodeFixProvider : CodeFixProviderBase
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(InvalidTargetPropertyForFieldChangeCallback.ComponentId);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (!TryFindFirstAncestorOrSelf<FieldDeclarationSyntax>(root, context.Span, out var declaration))
                return;

            var document = context.Document;
            var diagnostic = context.Diagnostics[0];
            var action = CreateCodeAction(CodeFixResources.URA0053CodeFixTitle, ct => CreatePropertyDeclaration(document, declaration, ct), diagnostic.Id);
            context.RegisterCodeFix(action, diagnostic);
        }

        private static async Task<Document> CreatePropertyDeclaration(Document document, FieldDeclarationSyntax fieldDeclaration, CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var backingField = fieldDeclaration.Declaration.Variables.First().Identifier.ValueText;
            var type = fieldDeclaration.Declaration.Type;
            var attribute = fieldDeclaration.AttributeLists.SelectMany(w => w.Attributes)
                                            .First(w => UdonSharpBehaviourUtility.PrettyTypeName(semanticModel.GetSymbolInfo(w).Symbol) == UdonConstants.UdonSharpFieldChangeCallbackFullName);
            var targetProperty = attribute.ArgumentList.Arguments.First().Expression.ParseValue();

            var modifiers = SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
            var getter = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, default, default, SyntaxFactory.ArrowExpressionClause(SyntaxFactory.IdentifierName(backingField)))
                                      .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
            var setterBlock = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, SyntaxFactory.IdentifierName(backingField), SyntaxFactory.IdentifierName("value"));
            var setter = SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration, SyntaxFactory.Block(SyntaxFactory.ExpressionStatement(setterBlock)));
            var accessors = SyntaxFactory.List(new[] { getter, setter });
            var property = SyntaxFactory.PropertyDeclaration(type, targetProperty).WithModifiers(modifiers).WithAccessorList(SyntaxFactory.AccessorList(accessors));

            var oldNode = fieldDeclaration.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            var newNode = oldNode.AddMembers(property);

            return await document.ReplaceNodeAsync(oldNode, newNode, cancellationToken).ConfigureAwait(false);
        }
    }
}