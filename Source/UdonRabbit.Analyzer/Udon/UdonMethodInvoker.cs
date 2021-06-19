using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using UdonRabbit.Analyzer.Extensions;
using UdonRabbit.Analyzer.Models;

namespace UdonRabbit.Analyzer.Udon
{
    public class UdonMethodInvoker
    {
        private static ReadOnlyCollection<(string, int)> CustomMethodInvokers => new List<(string, int)>
        {
            ("SendCustomEvent", 0),
            ("SendCustomNetworkEvent", 1),
            ("SendCustomEventDelayedSeconds", 0),
            ("SendCustomEventDelayedFrames", 0)
        }.AsReadOnly();

        private static ReadOnlyCollection<(string, int)> CustomNetworkMethodInvokers => new List<(string, int)>
        {
            ("SendCustomNetworkEvent", 1)
        }.AsReadOnly();

        public ISymbol Symbol { get; }

        public InvocationExpressionSyntax Node { get; }

        public UdonMethodInvoker(SyntaxNodeWithSymbol<InvocationExpressionSyntax> pair)
        {
            Symbol = pair.Info.Symbol;
            Node = pair.Node;
        }

        public UdonMethodInvoker(ISymbol symbol, InvocationExpressionSyntax invocation)
        {
            Symbol = symbol;
            Node = invocation;
        }

        public int GetArgumentAt()
        {
            return CustomMethodInvokers.First(w => w.Item1 == Symbol.Name).Item2;
        }

        public string GetTargetMethodName()
        {
            var arg = Node.ArgumentList.Arguments.ElementAtOrDefault(GetArgumentAt());
            if (arg == null)
                return string.Empty;

            return arg.Expression.ParseValue();
        }

        public static bool IsInvokerMethod(IMethodSymbol m)
        {
            return CustomMethodInvokers.Any(w => w.Item1 == m.Name);
        }

        public static bool IsNetworkInvokerMethod(IMethodSymbol m)
        {
            return CustomNetworkMethodInvokers.Any(w => w.Item1 == m.Name);
        }

        public static string GetTargetMethodName(ISymbol symbol, InvocationExpressionSyntax invocation)
        {
            var i = CustomMethodInvokers.First(w => w.Item1 == symbol.Name).Item2;
            var arg = invocation.ArgumentList.Arguments.ElementAtOrDefault(i);
            if (arg == null)
                return string.Empty;

            return arg.Expression.ParseValue();
        }
    }
}