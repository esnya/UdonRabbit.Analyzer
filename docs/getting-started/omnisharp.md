# Getting Started with UdonRabbit.Analyzer in OmniSharp

Install UdonRabbit.Analyzer to Roslyn integration using OmniSharp (VS Code, Atom, Brackets, Vim and others) is the following steps:

1. Download the latest NuGet package from [GitHub Releases](https://github.com/esnya/UdonRabbit.Analyzer/releases/latest)
2. **Extract** NuGet package using 7-Zip and other unzip tools into your Udon/UdonSharp workspace
   - If your project is tracked by Git, I recommended that you remove the decompression location from Git tracking.
3. Create a `omnisharp.json` into your project root.
4. Configure `RoslynExtensionsOptions.EnableAnalyzerSupport` to `true`
5. Configure `RoslynExtensionsOptions.LocationPaths` to decompression location
   - For example, you extract NuGet package to `./tools/` directory, specify `./tools/UdonRabbit.Analyzer.x.y.z/analyzers/dotnet/cs`
6. Restart OmniSharp
7. Happy Coding!

### Example of `omnisharp.json`

```json
{
  "RoslynExtensionsOptions": {
    "EnableAnalyzersSupport": true,
    "LocationPaths": ["./tools/UdonRabbit.Analyzer.0.3.0/analyzers/dotnet/cs"]
  }
}
```

## Troubleshooting

### I cannot use both UdonRabbit Analyzer and Roslynator (and/or other analyzers) in Visual Studio Code

Roslynator modifies the **global** `omnisharp.json` in Visual Studio Code Extension.
As a result, if you created local `omnisharp.json` as described above, the Roslynator configurations will not be found and will not work correctly.
Because UdonRabbit Analyzer provides the extension for Visual Studio Code to avoid conflicts, so consider using them.
