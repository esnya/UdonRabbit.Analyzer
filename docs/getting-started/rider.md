## Getting Started with UdonRabbit.Analyzer in JetBrains Rider

Install UdonRabbit.Analyzer to JetBrains Rider is the following steps:

1. Download the latest NuGet package from [GitHub Releases](https://github.com/mika-f/UdonRabbit.Analyzer/releases/latest)
1. Move downloaded NuGet package into the suitable directory
   1. This directory worked as the Local NuGet Package Source
   1. If it already exists, move one there
1. Add the new NuGet feeds to NuGet.config
1. Install UdonRabbit.Analyzer to all project
1. Happy Coding!

> NOTE: Normally, this steps [should be automated](https://docs.microsoft.com/en-us/visualstudio/gamedev/unity/extensibility/customize-project-files-created-by-vstu?view=vs-2019), but I don't know how to do it.

### Example of NuGet.config

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="dotnet-tools" value="https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-tools/nuget/v3/index.json" />
    <add key="Local Package Source" value="D:\NuGetPackages" />
  </packageSources>
  <disabledPackageSources>
    <clear />
  </disabledPackageSources>
</configuration>

```
