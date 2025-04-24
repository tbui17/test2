using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Tools.GitHub;

namespace Pipeline;
using System.IO;
using System.Linq;
using Lokql.Engine.Commands;
using NotNullStrings;
using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Utilities;
using FileSystem;
using Wiki;
using Serilog;


public partial class Build
{

    AbsolutePath WikiFolder => TemporaryDirectory / "WikiBin";
    AbsolutePath CommandHelpFile => WikiFolder / "LokqlDx-‐-commands.md";
    AbsolutePath WikiRepositoryFolder => AbsolutePath.Create(Path.GetTempPath()) / "WikiRepo"; // we need to use the system temp path because TemporaryDirectory (.nuke/temp) is inside a repository


    [Parameter] string WikiRepositoryUrl;
    [Parameter] string MainRepositoryUrl;

    [Parameter] public bool DryRun;

    [Solution] Solution Solution;

    [Parameter] string GitUsername;
    [Parameter] string GitEmail;

    GitHubActions GitHubActions => GitHubActions.Instance;



    [GitRepository] GitRepository GitRepository;

    GitRepository MainRepository { get; set; }

    GitRepository WikiRepository { get; set; }

    Target ProvideMainRepository => _ => _
        .Unlisted()
        .Executes(() =>
            {
                var isEmpty = MainRepositoryUrl.IsNullOrWhiteSpace();
                MainRepository = isEmpty
                    ? GitRepository
                    : GitRepository.FromUrl(MainRepositoryUrl);

                Log.Information("Initialized main repository object. {IsFromParameter} {@RepositoryDetails}",!isEmpty,MainRepository.GetDetails());
            }
        );

     Target ProvideWikiRepository => _ => _
        .DependsOn(ProvideMainRepository)
        .Unlisted()
        .Executes(() =>
            {
                var isEmpty = WikiRepositoryUrl.IsNullOrWhiteSpace();
                WikiRepository = isEmpty
                    ? MainRepository.GetWikiRepository()
                    : GitRepository.FromUrl(WikiRepositoryUrl);

                Log.Information("Initialized wiki repository object. {IsFromParameter} {@RepositoryDetails}",!isEmpty,MainRepository.GetDetails());
            }
        );

    GitContextFactory GitContextFactory { get; set; }
    Target ProvideGitContextFactory => _ => _
        .Requires(() => Git)
        .Unlisted()
        .Executes(() => GitContextFactory = new GitContextFactory(Git));

    GitContext WikiGitContext { get; set; }
    Target InitializeWikiGitContext => _ => _
        .DependsOn(ProvideGitContextFactory,CloneWikiRepository)
        .Requires(() => GitUsername, () => GitEmail)
        .Unlisted()
        .Executes(() =>
        {
            var ctx = GitContextFactory.Create(WikiRepositoryFolder);
            ctx.Git($"config --local user.name \"{GitUsername:nq}\"");
            ctx.Git($"config --local user.email \"{GitEmail:nq}\"");
            ctx.Git("config --local core.autocrlf false");
            var url = WikiRepository.GetAuthenticatedUrl(GitUsername,GitHubActions.Token);
            ctx.Git($"remote add {WikiRepositoryFolder.Name} {url}");
            WikiGitContext = ctx;
        });


    Target CreateOrCleanWikiBin => _ => _
        .Unlisted()
        .Executes(() =>
            {
                Log.Debug("Clearing {Folder}", WikiFolder);
                WikiFolder.CreateOrCleanDirectory();
            }
        );

    Target CreateOrCleanRepositoryFolder => _ => _
        .Unlisted()
        .Executes(() =>
            {
                Log.Debug("Clearing {Folder}", WikiRepositoryFolder);
                WikiRepositoryFolder.CreateOrCleanDirectory();
            }
        );

    Target GenerateCommandHelp => _ => _
        .DependsOn(CreateOrCleanWikiBin)
        .Executes(() =>
            {
                var helpMessage = new VerbRenderer().RenderMarkdownHelp(
                    CommandProcessor.Default().GetVerbs()
                    );
                var file = CommandHelpFile;
                Log.Debug("Writing command help to {FilePath}", file);
                file.WriteAllText(helpMessage);
            }
        );

    Target CloneWikiRepository => _ => _
        .DependsOn(ProvideGitContextFactory, ProvideWikiRepository, CreateOrCleanRepositoryFolder)
        .Unlisted()
        .Executes(() =>
            {
                Log.Debug("Created clean folder at {Folder}", WikiRepositoryFolder);
                var ctx = GitContextFactory.Create(WikiRepositoryFolder.Parent.NotNull());
                ctx.Git($"clone {WikiRepository.HttpsUrl} {WikiRepositoryFolder.Name}");
            }
        );

    bool VerifyDirty()
    {
        var ctx = WikiGitContext;
        var changes = ctx.Git("status --porcelain");
        var hasChanges = changes.Count > 0;
        Log.Debug("{ChangeCount} changes detected",changes.Count);
        return hasChanges;
    }

    Target PopulateWikiRepositoryFolder => _ => _
        .DependsOn(CloneWikiRepository, GenerateCommandHelp)
        .Unlisted()
        .Executes(() =>
            {
                WikiFolder.CopyContents(WikiRepositoryFolder);
                Log.Debug("Copied contents of {SourceFolder} to {TargetFolder}", WikiFolder, WikiRepositoryFolder);
            }
        );

    Target WriteWiki => _ => _
        .DependsOn(PopulateWikiRepositoryFolder,InitializeWikiGitContext)
        .OnlyWhenDynamic(VerifyDirty)
        .Executes(() =>
            {
                var ctx = WikiGitContext;
                ctx.Git("add --all");
                var message = ctx.Git("diff --staged --shortstat").Select(x => x.Text).JoinAsLines();
                ctx.Commit(message);
                Log.Information("Successfully committed wiki changes in repository at {Folder}.",WikiRepositoryFolder);
            }
        );

    Target PushWikiChanges => _ => _
        .DependsOn(ProvideGitContextFactory)
        .TriggeredBy(WriteWiki)
        .OnlyWhenStatic(() => !DryRun)
        .Unlisted()
        .Executes(() =>
            {
                var ctx = WikiGitContext;
                ctx.Git($"push {WikiRepositoryFolder.Name} HEAD:master");
                Log.Information("Successfully pushed wiki changes.");
            }
        );
}

