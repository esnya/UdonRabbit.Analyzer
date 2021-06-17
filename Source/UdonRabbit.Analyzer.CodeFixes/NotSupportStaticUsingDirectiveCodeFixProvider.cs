using System;
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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NotSupportStaticUsingDirectiveCodeFixProvider))]
    [Shared]
    public class NotSupportStaticUsingDirectiveCodeFixProvider : CodeFixProviderBase
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(NotSupportStaticUsingDirective.ComponentId);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (!TryFindFirstAncestorOrSelf<UsingDirectiveSyntax>(root, context.Span, out var directive))
                return;

            var document = context.Document;
            var diagnostic = context.Diagnostics[0];
            var action = CreateCodeAction(CodeFixResources.URA0030CodeFixTitle, ct => ResolveReferenceAndRemoveStaticUsingDirective(document, directive, ct), diagnostic.Id);
            context.RegisterCodeFix(action, diagnostic);
        }

        private static async Task<Document> ResolveReferenceAndRemoveStaticUsingDirective(Document document, UsingDirectiveSyntax directive, CancellationToken cancellationToken)
        {
            var references = await FindInvalidReferences(document, directive, cancellationToken).ConfigureAwait(false);
            var newDocument = await ReplaceOrInsetNewUsingDirective(document, directive, cancellationToken).ConfigureAwait(false);
            return await ResolveMissingReferences(newDocument, directive, references, cancellationToken).ConfigureAwait(false);
        }

        private static async Task<IEnumerable<IdentifierNameSyntax>> FindInvalidReferences(Document document, UsingDirectiveSyntax directive, CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var references = new List<IdentifierNameSyntax>();

            var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            foreach (var syntax in syntaxRoot.DescendantNodesAndSelf().Where(w => w is IdentifierNameSyntax).Cast<IdentifierNameSyntax>())
            {
                var s = semanticModel.GetSymbolInfo(syntax);
                switch (s.Symbol)
                {
                    case IMethodSymbol m when m.ReceiverType.ToClassString() == directive.Name.ToFullString():
                    case IPropertySymbol p when p.ContainingType.ToClassString() == directive.Name.ToFullString():
                    case IFieldSymbol f when f.ContainingType.ToClassString() == directive.Name.ToFullString():
                        references.Add(syntax);
                        break;
                }
            }

            return references;
        }

        private static async Task<Document> ReplaceOrInsetNewUsingDirective(Document document, UsingDirectiveSyntax oldDirective, CancellationToken cancellationToken)
        {
            var @namespace = SyntaxFactory.ParseName(oldDirective.Name.ToFullString().Substring(0, oldDirective.Name.ToFullString().LastIndexOf(".", StringComparison.Ordinal)));
            var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (syntaxRoot is not CompilationUnitSyntax compilation)
                return document;

            var isRequireNewUsingDirective = compilation.Usings.None(w => w.StaticKeyword.IsKind(SyntaxKind.None) && w.Name.IsEquivalentTo(@namespace, true));
            if (isRequireNewUsingDirective)
            {
                UsingDirectiveSyntax ReplaceStaticDirective(UsingDirectiveSyntax directive)
                {
                    if (directive.IsEquivalentTo(oldDirective))
                        return directive.WithStaticKeyword(SyntaxFactory.Token(SyntaxKind.None)).WithName(@namespace);
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

        private static async Task<Document> ResolveMissingReferences(Document document, UsingDirectiveSyntax directive, IEnumerable<IdentifierNameSyntax> references, CancellationToken cancellationToken)
        {
            var @namespace = directive.Name.ToFullString().Substring(directive.Name.ToFullString().LastIndexOf(".", StringComparison.Ordinal) + 1);

            SyntaxNode ReplaceToQualifiedNameReference(IdentifierNameSyntax node)
            {
                if (string.IsNullOrWhiteSpace(@namespace))
                    return node;
                return SyntaxFactory.IdentifierName($"{@namespace}.{node.Identifier.ValueText}");

                // I want to use the following code, but ReplaceNode throws InvalidCastException
                // return SyntaxFactory.QualifiedName(SyntaxFactory.ParseName(@namespace), SyntaxFactory.Token(SyntaxKind.DotToken), SyntaxFactory.IdentifierName(node.Identifier.ValueText));
            }

            foreach (var reference in references)
            {
                var oldNode = await document.FindEquivalentNodeAsync(reference, cancellationToken).ConfigureAwait(false);
                var newNode = ReplaceToQualifiedNameReference(oldNode);

                document = await document.ReplaceNodeAsync(oldNode, newNode, cancellationToken).ConfigureAwait(false);
            }

            return document;
        }
    }
}