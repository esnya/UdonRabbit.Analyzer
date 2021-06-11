using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

namespace UdonRabbit.Analyzer.Abstractions
{
    public abstract class CodeFixProviderBase : CodeFixProvider
    {
        protected static bool TryFindFirstAncestorOrSelf<T>(SyntaxNode root, TextSpan span, out T r) where T : SyntaxNode
        {
            r = root.FindNode(span).FirstAncestorOrSelf<T>();

            return r != null;
        }

        protected static CodeAction CreateCodeAction(string title, Func<CancellationToken, Task<Document>> callback, string equivalenceKey = null, params object[] arguments)
        {
            return CodeAction.Create(string.Format(title, arguments), callback, equivalenceKey);
        }

        protected static CodeAction CreateCodeAction(string title, Func<CancellationToken, Task<Solution>> callback, string equivalenceKey = null, params object[] arguments)
        {
            return CodeAction.Create(string.Format(title, arguments), callback, equivalenceKey);
        }
    }
}