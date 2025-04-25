using Nuke.Common.CI.GitHubActions;
namespace Pipeline;
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

    AbsolutePath WikiDirectory => TemporaryDirectory / "WikiBin";
    AbsolutePath CommandHelpFile => WikiDirectory / "LokqlDx-‐-commands.md";
    AbsolutePath WikiRepositoryDirectory => TemporaryDirectory / "WikiRepo";
    const string BotName = "github-actions[bot]";
    const string BotEmail = "41898282+github-actions[bot]@users.noreply.github.com";



    [Parameter("You should not include credentials in the URL.")]
    string WikiRepositoryUrl;
    [Parameter("You should not include credentials in the URL.")]
    string MainRepositoryUrl;

    [Parameter] public bool DryRun;

    [Solution] Solution Solution;

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

                Log.Information("Initialized main repository object. {IsFromParameter} {@Repository}",!isEmpty,MainRepository);
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

                Log.Information("Initialized wiki repository object. {IsFromParameter} {@Repository}",!isEmpty,WikiRepository);
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
        .Unlisted()
        .Executes(() =>
        {
            var ctx = GitContextFactory.Create(WikiRepositoryDirectory);
            ctx.Git($"config --local user.name \"{BotName:nq}\"");
            ctx.Git($"config --local user.email \"{BotEmail:nq}\"");
            var url = WikiRepository.GetAuthenticatedHttpsUrl(new AuthenticationDetails(BotName,GitHubActions.NotNull().Token.NotNullOrWhiteSpace()));
            ctx.Git($"git remote set-url origin {url}");
            WikiGitContext = ctx;
            Log.Information("Initialized git repository configuration for {@Repository}",WikiRepository);
        });


    Target CreateOrCleanWikiBin => _ => _
        .Unlisted()
        .Executes(() =>
            {
                Log.Debug("Clearing {Folder}", WikiDirectory);
                WikiDirectory.CreateOrCleanDirectory();
            }
        );

    Target CreateOrCleanRepositoryFolder => _ => _
        .Unlisted()
        .Executes(() =>
            {
                Log.Debug("Clearing {Folder}", WikiRepositoryDirectory);
                WikiRepositoryDirectory.CreateOrCleanDirectory();
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
                Log.Debug("Created clean folder at {Folder}", WikiRepositoryDirectory);
                var ctx = GitContextFactory.Create(WikiRepositoryDirectory.Parent.NotNull());
                ctx.Git($"clone {WikiRepository.HttpsUrl} {WikiRepositoryDirectory.Name}");
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
                WikiDirectory.CopyContents(WikiRepositoryDirectory);
                Log.Debug("Copied contents of {SourceFolder} to {TargetFolder}", WikiDirectory, WikiRepositoryDirectory);
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
                ctx.Git($"commit -m \"{message:nq}\"");
                Log.Information("Successfully committed wiki changes in repository at {Folder}.",WikiRepositoryDirectory);
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
                ctx.Git($"push");
                Log.Information("Successfully pushed wiki changes.");
            }
        );
}

