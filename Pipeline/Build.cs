using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;

namespace Pipeline;

[GitHubActions(name: "a", GitHubActionsImage.UbuntuLatest, OnPullRequestBranches = ["main"])]
public partial class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    ///

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    Configuration Configuration => IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [PathVariable] Tool Git;

    public static int Main() => Execute<Build>(x => x.Default);

    Target Clean => _ => _
        .Executes(() =>
            {
                DotNetTasks.DotNetClean();
            }
        );

    Target Restore => _ => _
        .Executes(() =>
            {
                DotNetTasks.DotNetRestore();
            }
        );

    Target Compile => _ => _
        .Executes(() =>
            {
                DotNetTasks.DotNetBuild();
            }
        );


    Target Default => _ => _
        .DependsOn(Test, WriteWiki, ProvideMainRepository);
}
