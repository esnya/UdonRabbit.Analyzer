using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using UdonRabbit.Analyzer.Udon;

namespace UdonRabbit.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MethodSpecifyForSendCustomEventIsNotFoundInBehaviour : DiagnosticAnalyzer
    {
        public const string ComponentId = "URA0045";
        private const string Category = UdonConstants.UdonCategory;
        private const string HelpLinkUri = "https://github.com/esnya/UdonRabbit.Analyzer/blob/master/docs/analyzers/URA0045.md";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.URA0045Title), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.URA0045MessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.URA0045Description), Resources.ResourceManager, typeof(Resources));
        private static readonly DiagnosticDescriptor RuleSet = new(ComponentId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUri);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleSet);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeMethodInvocation, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeMethodInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax) context.Node;
            if (!UdonSharpBehaviourUtility.ShouldAnalyzeSyntax(context.SemanticModel, invocation))
                return;

            var symbol = context.SemanticModel.GetSymbolInfo(invocation);
            if (symbol.Symbol is not IMethodSymbol method)
                return;

            if (!UdonMethodInvoker.IsInvokerMethod(method))
                return;

            // has receiver
            if (invocation.Expression is MemberAccessExpressionSyntax expression)
            {
                var receiver = expression.Expression;
                var t = context.SemanticModel.GetTypeInfo(receiver);
                if (!UdonSharpBehaviourUtility.IsUserDefinedTypes(context.SemanticModel, t.Type, t.Type.TypeKind))
                    return;

                var name = UdonMethodInvoker.GetTargetMethodName(method, invocation);
                var m = t.Type.GetMembers().Where(w => w is IMethodSymbol).FirstOrDefault(w => w.Name == name);
                if (m == null)
                    UdonSharpBehaviourUtility.ReportDiagnosticsIfValid(context, RuleSet, invocation, name, method.Name, t.Type.Name);
            }
            else
            {
                var name = UdonMethodInvoker.GetTargetMethodName(method, invocation);
                var m = context.SemanticModel.LookupSymbols(invocation.SpanStart).Where(w => w is IMethodSymbol).FirstOrDefault(w => w.Name == name);
                if (m == null)
                {
                    var cls = invocation.FirstAncestorOrSelf<ClassDeclarationSyntax>();
                    var sym = context.SemanticModel.GetDeclaredSymbol(cls);
                    UdonSharpBehaviourUtility.ReportDiagnosticsIfValid(context, RuleSet, invocation, name, method.Name, sym.Name);
                }
            }
        }
    }
}