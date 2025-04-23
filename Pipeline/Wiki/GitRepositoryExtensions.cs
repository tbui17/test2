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
}