using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

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

        public static async Task<T> FindEquivalentNodeAsync<T>(this Document document, T node, CancellationToken cancellationToken = default) where T : SyntaxNode
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            return root.DescendantNodesAndSelf(_ => true).OfType<T>().FirstOrDefault(w => w.IsEquivalentTo(node, true));
        }

        public static async Task<SyntaxNode> FindNodeAsync(this Document document, TextSpan span, CancellationToken cancellationToken = default)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            return root.FindNode(span);
        }
    }
}