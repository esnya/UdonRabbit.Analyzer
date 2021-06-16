using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

namespace UdonRabbit.Analyzer.Extensions
{
    public static class DocumentExtensions
    {
        public static async Task<Document> ReplaceNodeAsync(this Document document, SyntaxNode oldNode, SyntaxNode newNode, CancellationToken cancellationToken = default)
        {
            var oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var newRoot = oldRoot.ReplaceNode(oldNode, newNode);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}