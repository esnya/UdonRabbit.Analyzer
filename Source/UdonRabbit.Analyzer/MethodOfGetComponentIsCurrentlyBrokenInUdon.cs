using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using UdonRabbit.Analyzer.Udon;

namespace UdonRabbit.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MethodOfGetComponentIsCurrentlyBrokenInUdon : DiagnosticAnalyzer
    {
        public const string ComponentId = "URA0047";
        private const string Category = UdonConstants.UdonCategory;
        private const string HelpLinkUri = "https://github.com/esnya/UdonRabbit.Analyzer/blob/master/docs/analyzers/URA0047.md";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.URA0047Title), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.URA0047MessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.URA0047Description), Resources.ResourceManager, typeof(Resources));
        private static readonly DiagnosticDescriptor RuleSet = new(ComponentId, Title, MessageFormat, Category, DiagnosticSeverity.Error, true, Description, HelpLinkUri);

        private static readonly ReadOnlyCollection<string> BrokenGenericParameters = new List<string>
        {
            "VRC.SDKBase.VRC_AvatarPedestal",
            "VRC.SDK3.Components.VRCAvatarPedestal",
            "VRC.SDKBase.VRC_Pickup",
            "VRC.SDK3.Components.VRCPickup",
            "VRC.SDKBase.VRC_PortalMarker",
            "VRC.SDK3.Components.VRCPortalMarker",
            "VRC.SDKBase.VRCStation",
            "VRC.SDK3.Components.VRCStation",
            "VRC.SDK3.Video.Components.VRCUnityVideoPlayer",
            "VRC.SDK3.Video.Components.AVPro.VRCAVProVideoPlayer",
            "VRC.SDK3.Video.Components.Base.BaseVRCVideoPlayer",
            "VRC.SDK3.Components.VRCObjectPool",
            "VRC.SDK3.Components.VRCObjectSync"
        }.AsReadOnly();

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

            if (!method.Name.StartsWith("GetComponent") && method.TypeArguments.Length == 0)
                return;

            if (BrokenGenericParameters.Contains(method.TypeArguments[0].ToDisplayString()))
                UdonSharpBehaviourUtility.ReportDiagnosticsIfValid(context, RuleSet, invocation, method.Name);
        }
    }
}