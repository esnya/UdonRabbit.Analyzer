## Getting Started with UdonRabbit.Analyzer in JetBrains Rider

> NOTE: I'm using Visual Studio Tools for Unity, so maybe it's the feature.

Install UdonRabbit.Analyzer to JetBrains Rider is the following steps:

1. Add the new NuGet feeds to `NuGet.config`
    1. `Tools` > `NuGet` > `Manage NuGet Packages for Solution`
    2. Open `Sources` tab
    3. Add a new feed
       * Name: `esnya@GitHub`
       * URL: `https://nuget.pkg.github.com/esnya/index.json`
       * User: Your GitHub ID
       * Password: Your GitHub Personal Access Token (PAT)
       * Enabled: ✓
2. Open `Packages` tab
3. Search `UdonRabbit.Analyzer`
4. Install UdonRabbit.Analyzer to all projects
    * Click this
    * <img src="https://user-images.githubusercontent.com/10832834/112909309-1f9e7900-912c-11eb-8709-a69aa591e595.PNG" height="150px" />
5. Happy Coding!
