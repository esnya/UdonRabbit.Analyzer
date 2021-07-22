using System.Collections.Generic;
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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NotAllowDirectlySetValueWithFieldChangeCallbackCodeFixProvider))]
    [Shared]
    public class NotAllowDirectlySetValueWithFieldChangeCallbackCodeFixProvider : CodeFixProviderBase
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(NotAllowDirectlySetValueWithFieldChangeCallback.ComponentId);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (!TryFindFirstAncestorOrSelf<AssignmentExpressionSyntax>(root, context.Span, out var assignment))
                return;

            var document = context.Document;
            var diagnostic = context.Diagnostics[0];
            var action = CreateCodeAction(CodeFixResources.URA0052CodeFixTitle, ct => UsePropertyAssignment(document, assignment, ct), diagnostic.Id);
            context.RegisterCodeFix(action, diagnostic);
        }

        private static async Task<Document> UsePropertyAssignment(Document document, AssignmentExpressionSyntax assignment, CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var memberAccess = assignment.Left as MemberAccessExpressionSyntax;
            if (memberAccess == null)
                return document;

            var symbol = ModelExtensions.GetSymbolInfo(semanticModel, memberAccess);
            if (symbol.Symbol is not IFieldSymbol field)
                return document;

            var attributes = field.GetAttributes();
            var attribute = attributes.First(w => w.AttributeClass.ToClassString() == UdonConstants.UdonSharpFieldChangeCallbackFullName);
            var reference = await attribute.ApplicationSyntaxReference.GetSyntaxAsync(cancellationToken).ConfigureAwait(false) as AttributeSyntax;
            if (reference == null)
                return document;

            var targetProperty = reference.ArgumentList.Arguments[0].Expression.ParseValue();
            var t = semanticModel.GetTypeInfo(memberAccess.Expression);
            var members = t.Type.GetMembers(targetProperty);
            var property = members.FirstOrDefault(w => w is IPropertySymbol p && p.Name == targetProperty) as IPropertySymbol;
            if (property == null)
                return document;

            if (property.SetMethod is { DeclaredAccessibility: Accessibility.Public })
            {
                var oldNode = assignment.FirstAncestorOrSelf<AssignmentExpressionSyntax>();
                var newNode = assignment.WithLeft(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, memberAccess.Expression, SyntaxFactory.IdentifierName(targetProperty)));

                return await document.ReplaceNodeAsync(oldNode, newNode, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var oldNode = assignment.FirstAncestorOrSelf<AssignmentExpressionSyntax>();

                var ma = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, memberAccess.Expression, SyntaxFactory.IdentifierName("SetProgramVariable"));
                var arguments = new List<ArgumentSyntax>
                {
                    SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(memberAccess.Name.Identifier.ValueText))),
                    SyntaxFactory.Argument(assignment.Right)
                };
                var newNode = SyntaxFactory.InvocationExpression(ma, SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(arguments)));

                return await document.ReplaceNodeAsync(oldNode, newNode, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}