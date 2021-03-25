/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

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

        private HashSet<string> _exposedMethodSymbols;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleSet);

        public override void Initialize(AnalysisContext context)
        {
            // context.EnableConcurrentExecution();
            Debugger.Launch();

            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax) context.Node;

            if (invocation.Expression is not MemberAccessExpressionSyntax member)
                return;

            if (_exposedMethodSymbols == null)
                LoadSdkAssemblies(context);

            var methodSymbol = context.SemanticModel.GetSymbolInfo(invocation);
            if (methodSymbol.Symbol == null)
                return;

            if (methodSymbol.Symbol is not IMethodSymbol method)
                return;

            if (method.Name == "EmitInteract")
                context.ReportDiagnostic(Diagnostic.Create(RuleSet, invocation.GetLocation(), method.Name));
        }

        private void LoadSdkAssemblies(SyntaxNodeAnalysisContext context)
        {
            var reference = context.Compilation.ExternalReferences.FirstOrDefault(w => w.Display.Contains("VRC.Udon.Common.dll"));
            if (reference == null)
                return; // The VRChat Udon SDK is not included in the workspace.

            string FindUnityAssetsDirectory(string path)
            {
                return path.Substring(0, path.IndexOf("Assets", StringComparison.InvariantCulture));
            }

            var assemblies = Path.GetFullPath(Path.Combine(FindUnityAssetsDirectory(reference.Display), "Library", "ScriptAssemblies"));
            var editor = Path.Combine(assemblies, "VRC.Udon.Editor.dll");
            if (!File.Exists(editor))
                return; // Invalid Path;

            Assembly ResolveDynamicLoadingAssemblies(object _, ResolveEventArgs args)
            {
                var dll = $"{args.Name.Split(',').First()}.dll";
                if (context.Compilation.ExternalReferences.Any(w => w.Display.Contains(dll)))
                    return Assembly.LoadFrom(context.Compilation.ExternalReferences.First(w => w.Display.Contains(dll)).Display);
                return Assembly.LoadFrom(Path.GetFullPath(Path.Combine(assemblies, dll)));
            }

            AppDomain.CurrentDomain.AssemblyResolve += ResolveDynamicLoadingAssemblies;
            var assembly = Assembly.LoadFrom(editor);

            var manager = new UdonEditorManager(assembly);
            if (!manager.HasInstance)
                return;

            _exposedMethodSymbols = new HashSet<string>(manager.GetUdonNodeDefinitions());
        }

        private string GetUdonNamedType(string name)
        {
            return name;
        }
    }
}