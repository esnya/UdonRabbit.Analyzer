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

namespace UdonRabbit.Analyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NotSupportNamespaceAliasDirectiveCodeFixProvider))]
    [Shared]
    public class NotSupportNamespaceAliasDirectiveCodeFixProvider : CodeFixProviderBase
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(NotSupportNamespaceAliasDirective.ComponentId);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (!TryFindFirstAncestorOrSelf<UsingDirectiveSyntax>(root, context.Span, out var directive))
                return;

            var document = context.Document;
            var diagnostic = context.Diagnostics[0];
            var action = CreateCodeAction(CodeFixResources.URA0031CodeFixTitle, ct => ResolveReferencesAndRemoveNamespaceAliasDirective(document, directive, ct), diagnostic.Id);
            context.RegisterCodeFix(action, diagnostic);
        }

        private async Task<Document> ResolveReferencesAndRemoveNamespaceAliasDirective(Document document, UsingDirectiveSyntax directive, CancellationToken cancellationToken)
        {
            var references = (await FindInvalidReferences(document, directive, cancellationToken).ConfigureAwait(false)).ToList();
            var shouldUseQualifiedName = await ShouldUseQualifiedNameInChanges(document, directive, references, cancellationToken).ConfigureAwait(false);
            var newDocument = await ReplaceOrInsertNewUsingDirective(document, directive, shouldUseQualifiedName, cancellationToken).ConfigureAwait(false);
            return await ResolveMissingReferences(newDocument, directive, references, shouldUseQualifiedName, cancellationToken).ConfigureAwait(false);
        }

        private static async Task<IEnumerable<SyntaxNode>> FindInvalidReferences(Document document, UsingDirectiveSyntax directive, CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var references = new List<SyntaxNode>();

            var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            foreach (var syntax in syntaxRoot.DescendantNodesAndSelf().Where(w => w is MemberAccessExpressionSyntax or AliasQualifiedNameSyntax or QualifiedNameSyntax))
            {
                var s = semanticModel.GetSymbolInfo(syntax);
                switch (s.Symbol)
                {
                    case ITypeSymbol t when t.ContainingNamespace.ToDisplayString() == directive.Name.ToFullString():
                    case IMethodSymbol m when syntax is AliasQualifiedNameSyntax && m.ContainingNamespace.ToDisplayString() == directive.Name.ToFullString():
                        references.Add(syntax);
                        break;
                }
            }

            return references;
        }

        private static async Task<bool> ShouldUseQualifiedNameInChanges(Document document, UsingDirectiveSyntax oldDirective, IEnumerable<SyntaxNode> references, CancellationToken cancellationToken)
        {
            // apply the changes once and get if there are any compilation errors.
            var indeterminateDocument = await ReplaceOrInsertNewUsingDirectivePreview(document, oldDirective, cancellationToken).ConfigureAwait(false);
            var determinateDocument = await ResolveMissingReferencesPreview(indeterminateDocument, oldDirective, references, cancellationToken).ConfigureAwait(false);
            var semanticModel = await determinateDocument.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            var diagnostics = semanticModel.GetDiagnostics(null, cancellationToken);
            return diagnostics.Any(w => w.Severity == DiagnosticSeverity.Error && w.Id == "CS0104" && w.Descriptor.Category == "Compiler");
        }

        private static async Task<Document> ReplaceOrInsertNewUsingDirectivePreview(Document document, UsingDirectiveSyntax oldDirective, CancellationToken cancellationToken)
        {
            return await ReplaceOrInsertNewUsingDirective(document, oldDirective, false, cancellationToken).ConfigureAwait(false);
        }

        private static async Task<Document> ReplaceOrInsertNewUsingDirective(Document document, UsingDirectiveSyntax oldDirective, bool shouldUseQualifiedName, CancellationToken cancellationToken)
        {
            var @namespace = oldDirective.Name;
            var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (syntaxRoot is not CompilationUnitSyntax compilation)
                return document;

            var isRequireNewUsingDirective = compilation.Usings.None(w => w.Alias.IsKind(SyntaxKind.None) && w.Name.IsEquivalentTo(@namespace, true));
            if (isRequireNewUsingDirective && !shouldUseQualifiedName)
            {
                UsingDirectiveSyntax ReplaceStaticDirective(UsingDirectiveSyntax directive)
                {
                    if (directive.IsEquivalentTo(oldDirective))
                        return directive.WithAlias(null);
                    return directive;
                }

                var usings = compilation.Usings.Select(ReplaceStaticDirective).ToList();
                return await document.ReplaceNodeAsync(compilation, compilation.WithUsings(SyntaxFactory.List(usings)), cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var usings = compilation.Usings.Where(w => !w.IsEquivalentTo(oldDirective)).ToList();
                return await document.ReplaceNodeAsync(compilation, compilation.WithUsings(SyntaxFactory.List(usings)), cancellationToken).ConfigureAwait(false);
            }
        }

        private static async Task<Document> ResolveMissingReferencesPreview(Document document, UsingDirectiveSyntax directive, IEnumerable<SyntaxNode> references, CancellationToken cancellationToken)
        {
            return await ResolveMissingReferences(document, directive, references, false, cancellationToken).ConfigureAwait(false);
        }

        private static async Task<Document> ResolveMissingReferences(Document document, UsingDirectiveSyntax directive, IEnumerable<SyntaxNode> references, bool shouldUseQualifiedName, CancellationToken cancellationToken)
        {
            var @namespace = directive.Name.ToFullString();

            IdentifierNameSyntax ProcessAliasQualifiedNameSyntax(AliasQualifiedNameSyntax a, SemanticModel m, SyntaxNode e)
            {
                if (m.ShouldUseQualifiedName(e.Span, a.Name.ToFullString()) || shouldUseQualifiedName)
                    return SyntaxFactory.IdentifierName($"{directive.Name.ToFullString()}.{a.Name.Identifier.ValueText}");
                return SyntaxFactory.IdentifierName(a.Name.Identifier.ValueText);
            }

            IdentifierNameSyntax ProcessMemberAccessExpressionSyntax(MemberAccessExpressionSyntax a, SemanticModel m, SyntaxNode e)
            {
                if (m.ShouldUseQualifiedName(e.Span, a.Name.Identifier.ValueText) || shouldUseQualifiedName)
                    return SyntaxFactory.IdentifierName($"{directive.Name.ToFullString()}.{a.Name.Identifier.ValueText}");
                return SyntaxFactory.IdentifierName(a.Name.Identifier.ValueText);
            }

            IdentifierNameSyntax ProcessQualifiedNameSyntax(QualifiedNameSyntax q, SemanticModel m, SyntaxNode e)
            {
                if (m.ShouldUseQualifiedName(e.Span, q.Right.Identifier.ValueText) || shouldUseQualifiedName)
                    return SyntaxFactory.IdentifierName($"{directive.Name.ToFullString()}.{q.Right.Identifier.ValueText}");
                return SyntaxFactory.IdentifierName(q.Right.Identifier.ValueText);
            }

            async Task<SyntaxNode> ReplaceToQualifiedNameReference(Document doc, SyntaxNode node)
            {
                var semanticModel = await doc.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
                var equivalentNode = await doc.FindEquivalentNodeAsync(node, cancellationToken).ConfigureAwait(false);

                SyntaxNode n = node switch
                {
                    AliasQualifiedNameSyntax a => ProcessAliasQualifiedNameSyntax(a, semanticModel, equivalentNode),
                    MemberAccessExpressionSyntax m => ProcessMemberAccessExpressionSyntax(m, semanticModel, equivalentNode),
                    QualifiedNameSyntax q => ProcessQualifiedNameSyntax(q, semanticModel, equivalentNode),
                    _ => null
                };

                return n?.WithLeadingTrivia(node.GetLeadingTrivia()).WithTrailingTrivia(node.GetTrailingTrivia());
            }

            foreach (var reference in references)
            {
                var oldNode = await document.FindEquivalentNodeAsync(reference, cancellationToken).ConfigureAwait(false);
                if (oldNode == null)
                    continue; // Already Processed, MAYBE UNREACHABLE

                var newNode = await ReplaceToQualifiedNameReference(document, oldNode).ConfigureAwait(false);
                if (newNode == null)
                    continue; // ???

                document = await document.ReplaceNodeAsync(oldNode, newNode, cancellationToken).ConfigureAwait(false);
            }

            return document;
        }
    }
}