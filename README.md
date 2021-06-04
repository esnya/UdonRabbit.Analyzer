# UdonRabbit.Analyzer

Experimental .NET Roslyn Analyzer for VRChat Udon and UdonSharp.  
This analyzer accelerates Udon development with UdonSharp by detecting grammars that are not supported by Udon and/or UdonSharp in the editor stage.  
Check out the [list of analyzers](./docs/analyzers/README.md) for supported grammars and other information.

## Features

- It reports the part of the behaviour that doesn't work well on VRChat as an error in editor
- It reports an error in the editor that about the syntax that cannot be compiled by the UdonSharp
- etc

## Getting Started

- [Microsoft Visual Studio 2019](./docs/getting-started/visual-studio.md)
- [Visual Studio Code](./docs/getting-started/vscode.md)
- [OmniSharp](./docs/getting-started/omnisharp.md)
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

### Create a new analyzer

1. Open `Source/UdonRabbit.Analyzer.sln` in your Visual Studio or other IDE
2. Build `UdonRabbit.Analyzer.CodeGen`
3. Run `dotnet ./UdonRabbit.Analyzer.CodeGen.dll -i IDENTIFIER -l "CLASS_NAME" -t "TITLE" -d "DESCRIPTION" -m "MESSAGE_FORMAT" -c CATEGORY -s SEVERITY -w ../` in `ROOT/bin`
4. Rebuild ResX in Visual Studio or run `ResGen.exe`
5. Start Coding!

## Testing

### Requirements

- .NET 5
- Visual Studio Version 16.8+
- Unity Project that containing VRCSDK3 and UdonSharp

### How to test

1. Configure the following environment variables in `Source/UdonRabbit.Analyzer.Test/bin/Debug/net5.0/UdonRabbit.runsettings`
   - `UDONRABBIT_ANALYZER_TEST_PROJECT` : Unity 2018.4.20f1 Test Project Location (`.csproj`)
     - I recommended to reference to `Assembly-CSharp.csproj` because it has all references to DLLs.
     - If you are not want to reference to `Assembly-CSharp.csproj`, add the following external references:
       - `TextMeshPro`
       - `UdonSharp.Runtime`
   - `UDONRABBIT_ANALYZER_TEST_UDON_SHARP` : `UdonSharp.Runtime.dll` Location
2. Run `dotnet test`

## Analyzer Documents

See https://docs.mochizuki.moe/en-us/udon-rabbit/packages/analyzer

## ScreenShots

### Report Unsupported Language Features

<img src="https://user-images.githubusercontent.com/10832834/120594815-70a85180-c47c-11eb-8edf-2b9b7fbc0517.PNG" width="650px" />

### Report Unsupported Udon APIs

<img src="https://user-images.githubusercontent.com/10832834/120595125-e1e80480-c47c-11eb-96d0-99e41217b5a9.PNG" width="650px" />

### Report Unsupported Unity Messages

<img src="https://user-images.githubusercontent.com/10832834/120595297-16f45700-c47d-11eb-987e-9b09e322b889.PNG" width="450px" />

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
