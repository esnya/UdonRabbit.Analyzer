using System.Collections.Immutable;
using System.Diagnostics;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using UdonRabbit.Analyzer.Udon;

namespace UdonRabbit.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FieldAccessorIsNotExposedToUdon : DiagnosticAnalyzer
    {
        private const string ComponentId = "URA0002";
        private const string Category = UdonConstants.UdonCategory;
        private const string HelpLinkUri = "https://docs.mochizuki.moe/udon-rabbit/packages/analyzer/analyzers/URA0002/";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.URA0002Title), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.URA0002MessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.URA0002Description), Resources.ResourceManager, typeof(Resources));
        private static readonly DiagnosticDescriptor RuleSet = new(ComponentId, Title, MessageFormat, Category, DiagnosticSeverity.Error, true, Description, HelpLinkUri);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleSet);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.SimpleMemberAccessExpression);
        }

        private void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
        {
            var memberAccess = (MemberAccessExpressionSyntax) context.Node;

            var classDeclaration = memberAccess.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (classDeclaration == null)
                return;

            var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
            if (!classSymbol.BaseType.Equals(context.SemanticModel.Compilation.GetTypeByMetadataName(UdonConstants.UdonSharpBehaviourFullName), SymbolEqualityComparer.Default))
                return;

            if (UdonSymbols.Instance == null)
                UdonSymbols.Initialize(context.Compilation);

            var isAssignment = memberAccess.Parent is AssignmentExpressionSyntax;

            var t = context.SemanticModel.GetTypeInfo(memberAccess.Expression);
            var fieldSymbol = context.SemanticModel.GetSymbolInfo(memberAccess);
            if (fieldSymbol.Symbol is IFieldSymbol field)
            {
                if (UdonSymbols.Instance != null && !UdonSymbols.Instance.FindUdonVariableName(context.SemanticModel, t.Type, field, isAssignment))
                    context.ReportDiagnostic(Diagnostic.Create(RuleSet, memberAccess.GetLocation(), field.Name));
                return;
            }

            if (fieldSymbol.Symbol is IPropertySymbol props)
            {
                if (UdonSymbols.Instance != null && !UdonSymbols.Instance.FindUdonVariableName(context.SemanticModel, t.Type, props, isAssignment))
                    context.ReportDiagnostic(Diagnostic.Create(RuleSet, memberAccess.GetLocation(), props.Name));
                return;
            }

            Debug.WriteLine("Unknown Field Accessor");
        }
    }
}