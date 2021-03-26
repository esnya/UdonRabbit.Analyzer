/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using System.Collections.Immutable;
using System.Diagnostics;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using UdonRabbit.Analyzer.Udon;

#pragma warning disable RS1026

namespace UdonRabbit.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MethodIsNotExposedToUdon : DiagnosticAnalyzer
    {
        private const string ComponentId = "URA0001";
        private const string Category = "Udon";
        private const string HelpLinkUri = "https://docs.mochizuki.moe/udon-rabbit/packages/analyzer/analyzers/URA0001/";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.URA0001Title), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.URA0001MessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.URA0001Description), Resources.ResourceManager, typeof(Resources));
        private static readonly DiagnosticDescriptor RuleSet = new(ComponentId, Title, MessageFormat, Category, DiagnosticSeverity.Error, true, Description, HelpLinkUri);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleSet);

        public override void Initialize(AnalysisContext context)
        {
            Debugger.Launch();

            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax) context.Node;

            var classDeclaration = invocation.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (classDeclaration == null)
                return;

            var declaration = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
            if (!declaration.BaseType.Equals(context.SemanticModel.Compilation.GetTypeByMetadataName("UdonSharp.UdonSharpBehaviour"), SymbolEqualityComparer.Default))
                return;

            if (UdonSymbols.Instance == null)
                UdonSymbols.Initialize(context.Compilation);

            var methodSymbol = context.SemanticModel.GetSymbolInfo(invocation);
            if (methodSymbol.Symbol == null)
                return;

            if (methodSymbol.Symbol is not IMethodSymbol method)
                return;

            if (UdonSymbols.Instance != null && UdonSymbols.Instance.FindUdonMethodName(context.SemanticModel, method))
                context.ReportDiagnostic(Diagnostic.Create(RuleSet, invocation.GetLocation(), method.Name));
        }
    }
}