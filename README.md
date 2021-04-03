# UdonRabbit.Analyzer

Experimental .NET Roslyn Analyzer for VRChat Udon and UdonSharp.  
This analyzer accelerates your Udon development with UdonSharp.  
Check out the [list of analyzers](docs/analyzers/README.md) defined in this project.  
Not enough types of analyzers in UdonRabbit.Analyzer?  
You can check out the [list of missing analyzers](https://github.com/mika-f/UdonRabbit.Analyzer/issues?q=is%3Aissue+is%3Aopen+sort%3Aupdated-desc+label%3Aenhancement) or create a new issues.

## Features

- It reports the part of the behaviour that doesn't work well on VRChat as an error in editor
- It reports an error in the editor that about the syntax that cannot be compiled by the UdonSharp
- etc

## Getting Started

- [Microsoft Visual Studio 2019](./docs/getting-started/visual-studio.md)
- [Visual Studio Code (OmniSharp)](./docs/getting-started/omnisharp.md)
- [JetBrains Rider](./docs/getting-started/rider.md)

## Requirements

- Microsoft Visual Studio 2019 or IDE that supports Roslyn Analyzer
- VRCSDK3 that supports Udon
- UdonSharp

## Development

### Requirements

- .NET 5
- Visual Studio Version 16.8+ or Visual Studio for Mac 8.8+
- Unity Project that containing VRCSDK3 and UdonSharp

### How to develop

1. Open `Source/UdonRabbit.Analyzer.sln` in your Visual Studio
2. Start `UdonRabbit.Analyzer.VSIX` as debug profile
3. After the Visual Studio Experimental Instance starts, open the Unity project that has VRCSDK3 and UdonSharp installed
4. Open any source file that inherits from `UdonSharp.UdonSharpBehaviour`

## Testing

### Requirements

- .NET 5
- Visual Studio Version 16.8+
- Unity Project that containing VRCSDK3 and UdonSharp

### How to test

1. Configure the following environment variables in `Source/UdonRabbit.Analyzer.Test/bin/Debug/net5.0/UdonRabbit.runsettings`
   - `UDONRABBIT_ANALYZER_TEST_PROJECT` : Unity 2018.4.20f1 Test Project Location (`.csproj`)
     - Default: `null` (Current Directory)
   - `UDONRABBIT_ANALYZER_TEST_UDON_SHARP` : `UdonSharp.Runtime.dll` Location
     - Default: `null` (Current Directory)
2. Run `dotnet test`

## Analyzer Documents

See https://docs.mochizuki.moe/udon-rabbit/analayzer/analyzers/

## ScreenShots

<img src="https://user-images.githubusercontent.com/10832834/112584755-c8528d00-8e3b-11eb-9204-1c05c0669ffc.PNG" width="500px" />

## Troubleshooting

### I found a bug on VS/VS Code/Rider/Analyzer

If you find a bug, feel free to create a new issue!

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
- [Microsoft/Microsoft.Unity.Analyzers](https://github.com/microsoft/Microsoft.Unity.Analyzers)

## Links

Discord Server : https://discord.gg/h42BzsFtD2  
Patreon : XXX
