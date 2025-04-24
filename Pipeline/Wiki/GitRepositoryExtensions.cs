using Nuke.Common.Git;
using Nuke.Common.Tools.GitHub;

namespace Pipeline.Wiki;

public static class GitRepositoryExtensions
{

    public static string GetWikiUrl(this GitRepository repository)
    {
        return $"https://github.com/{repository.GetGitHubOwner()}/{repository.GetGitHubName()}.wiki.git";
    }

    public static GitRepository GetWikiRepository(this GitRepository repository)
    {
        return GitRepository.FromUrl(repository.GetWikiUrl());
    }

    public static object GetDetails(this GitRepository repository)
    {
        var info = new
        {
            repository.Branch,
            repository.Commit,
            repository.Endpoint,
            repository.Head,
            repository.Identifier,
            repository.RemoteBranch,
            repository.RemoteName,
            repository.Tags,
            repository.LocalDirectory,
            repository.Protocol,
        };
        return info;
    }
}