﻿using System.Collections.Immutable;
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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NotSupportSmoothInterpolationOfSyncedTypeCodeFixProvider))]
    [Shared]
    public class NotSupportSmoothInterpolationOfSyncedTypeCodeFixProvider : CodeFixProviderBase
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(NotSupportSmoothInterpolationOfSyncedType.ComponentId);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (!TryFindFirstAncestorOrSelf<FieldDeclarationSyntax>(root, context.Span, out var declaration))
                return;

            var document = context.Document;
            var diagnostic = context.Diagnostics[0];
            var action = CreateCodeAction(CodeFixResources.URA0046CodeFixTitle, ct => RemoveUdonSyncedSmoothAttributeOptionFromAttribute(document, declaration, ct), diagnostic.Id);
            context.RegisterCodeFix(action, diagnostic);
        }

        private static async Task<Document> RemoveUdonSyncedSmoothAttributeOptionFromAttribute(Document document, FieldDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            AttributeSyntax ModifyUdonSyncedAttribute(AttributeSyntax attribute)
            {
                var symbol = semanticModel.GetSymbolInfo(attribute);
                if (symbol.Symbol is not IMethodSymbol m)
                    return attribute; // UNREACHABLE

                if (UdonSharpBehaviourUtility.PrettyTypeName(m) == "UdonSharp.UdonSyncedAttribute")
                    return SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("UdonSynced"), null);

                return attribute;
            }

            var oldNode = declaration.FirstAncestorOrSelf<FieldDeclarationSyntax>();
            var attributes = oldNode.AttributeLists.Select(w => SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(w.Attributes.Select(ModifyUdonSyncedAttribute))));
            var newNode = oldNode.WithAttributeLists(SyntaxFactory.List(attributes));

            return await document.ReplaceNode(oldNode, newNode, cancellationToken).ConfigureAwait(false);
        }

        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }
    }
}