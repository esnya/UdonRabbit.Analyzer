using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UdonRabbit.Analyzer.Extensions
{
    public static class ExpressionSyntaxExtensions
    {
        public static string ParseValue(this ExpressionSyntax obj)
        {
            return obj switch
            {
                LiteralExpressionSyntax literal => ParseLiteral(literal),
                InvocationExpressionSyntax invoke => ParseInvocation(invoke),
                _ => null
            };
        }

        private static string ParseLiteral(LiteralExpressionSyntax literal)
        {
            return literal.Kind() switch
            {
                SyntaxKind.StringLiteralExpression => literal.Token.ValueText,
                _ => ""
            };
        }

        private static string ParseInvocation(InvocationExpressionSyntax invocation)
        {
            return invocation.Expression switch
            {
                IdentifierNameSyntax identifier when identifier.Identifier.Kind() == SyntaxKind.IdentifierToken && identifier.Identifier.Text == "nameof" => invocation.ArgumentList.Arguments.FirstOrDefault()?.GetLastToken().ValueText,
                _ => ""
            };
        }
    }
}