# UdonRabbit.Analyzer

.NET Roslyn Analyzer for VRChat Udon and UdonSharp.  
This analyzer accelerates your Udon development with UdonSharp.  
Check out the [list of analyzers](docs/analyzers/README.md) defined in this project.  
Not enough types of analyzers in UdonRabbit.Analyzer?  
You can check out the [list of missing analyzers](https://github.com/mika-f/UdonRabbit.Analyzer/issues?q=is%3Aissue+is%3Aopen+sort%3Aupdated-desc+label%3Aenhancement) or create a new issues.

## Getting Started

### Microsoft Visual Studio

1. Download VSIX from [GitHub Releases](https://github.com/mika-f/UdonRabbit.Analyzer/releases/latest) and Install to your Visual Studio and restart it
2. Open your VRChat Udon/UdonSharp project in Visual Studio
3. Happy Coding!

### Microsoft Visual Studio Code

1. Download NuGet Packafe from [GitHub Releases](https://github.com/mika-f/UdonRabbit.Analyzer/releases/latest) and extract it to some location
2. Open your VRChat Udon/UdonSharp workspace in Visal Studio Code
3. Create a `omnisharp.json` in root directory
4. Configure `RoslynExtensionsOptions.EnableAnalyzerSupport` to true and add extract path to `LocationPaths`
5. Restart OmniSharp
6. Happy Coding!

## Requirements

- Microsoft Visual Studio 2019 or IDE that supports Roslyn Analyzer
- VRCSDK3 that supports Udon
- UdonSharp

## Development Requirements

- .NET 5
- Visual Studio Version 16.8+ or Visual Studio for Mac 8.8+
- Unity Project that containing VRCSDK3 and UdonSharp

## Analyzer Documents

See https://docs.mochizuki.moe/udon-rabbit/analayzer/analyzers/

## ScreenShots

<img src="https://user-images.githubusercontent.com/10832834/112584755-c8528d00-8e3b-11eb-9204-1c05c0669ffc.PNG" width="500px" />

## Troubleshooting

### Analyzer is not worked on UdonSharpBehaviour

You cannot run multiple UdonRabbit.Analyzer instances in the same Unity workspace.  
Therefore, if you are running Analyzer in multiple editors, disable one.  
If that doesn't work, we recommend restarting your PC.

## License

MIT by [@6jz](https://twitter.com/6jz)

## Third Party Notices

This project contains some code from the following project.  
See method comments for details.

- [MerlinVR/UdonSharp](https://github.com/MerlinVR/UdonSharp)
