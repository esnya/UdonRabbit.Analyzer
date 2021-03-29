## Getting Started with UdonRabbit.Analyzer in JetBrains Rider

> NOTE: I'm using Visual Studio Tools for Unity, so maybe it's the feature.

Install UdonRabbit.Analyzer to JetBrains Rider is the following steps:

1. Download the latest NuGet package from [GitHub Releases](https://github.com/esnya/UdonRabbit.Analyzer/releases/latest)
1. Move downloaded NuGet package into the suitable directory
   - This directory worked as the Local NuGet Package Source
   - If it already exists, move one there
1. Add the new NuGet feeds to `NuGet.config`
   - If it does not already exists, create a new one in project root
1. Install UdonRabbit.Analyzer to all project
   - or create a `packages.config` in your project root
1. Happy Coding!

### Example of `NuGet.config`

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

### Example of `packages.config`

```xml
<?xml version="1.0" encoding="utf-8"?>
<packages>
  <package id="UdonRabbit.Analyzer" version="0.3.0" targetFramework="net471" />
</packages>
```
