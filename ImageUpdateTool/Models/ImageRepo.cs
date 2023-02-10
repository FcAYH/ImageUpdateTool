using System.Text.RegularExpressions;

namespace ImageUpdateTool.Models;

public partial class ImageRepo
{
    public string LastestImageURL { get; private set; }
    public string LocalRepoPath { get { return _localRepoPath; } }

    // public const string URL_PREFIX = "https://cdn.jsdelivr.net/gh/FcAYH/Images/";
    //private const string GIT_REPO_URL = "https://github.com/FcAYH/Images.git";
    
    private string _localRepoPath;
    private string _rootDirectory;
    private string _gitRepoURL;
    private string _userName;
    private string _repositoryName;

    private const string URL_FORMAT = "https://cdn.jsdelivr.net/gh/{0}/{1}/{2}";
    
    [GeneratedRegex("https://[a-zA-Z.]*/([a-zA-Z0-9_-])*/([a-zA-Z0-9_-])*\\.git", RegexOptions.Compiled)]
    private static partial Regex URL_REGEX();

    private Utils.CommandRunner _gitProcess;

    public ImageRepo(string gitRepoURL, string rootDirectory)
    {
        _gitRepoURL = gitRepoURL;
        _rootDirectory = rootDirectory;

        // 从 https://github.com/UserName/RepositoryName.git 中截取仓库名称
        _repositoryName = _gitRepoURL.Split('/').Last().Split('.').First();

        Regex regex = URL_REGEX();
        var match = regex.Match(_gitRepoURL);

        _userName = new string(match.Groups[1].Captures.SelectMany(g => g.Value).ToArray());
        _repositoryName = new string(match.Groups[2].Captures.SelectMany(g => g.Value).ToArray());

        _localRepoPath = Path.Combine(_rootDirectory, _repositoryName);
        
        if (!Directory.Exists(_rootDirectory))
            Directory.CreateDirectory(_rootDirectory);

        if (!Directory.Exists(_localRepoPath))
            CallGitClone();
        
        _gitProcess = new("git", _localRepoPath);
    }

    public string CallGitClone()
    {
        Utils.CommandRunner gitClone = new("git", _rootDirectory);
        return gitClone.Run($"clone {_gitRepoURL}");
    }

    public string CallGitPull()
    {
        return _gitProcess.Run("pull");
    }

    public string CallGitPush()
    {
        return _gitProcess.Run("push");
    }

    public string CallGitCommit(string message)
    {
        return _gitProcess.Run($"commit -m \"{message}\"");
    }

    public string CallGitAdd(string relativePath)
    {
        LastestImageURL = string.Format(URL_FORMAT, 
                                    _userName, 
                                    _repositoryName, 
                                    relativePath.Replace("\\", "/"));

        return _gitProcess.Run($"add {relativePath}");
    }

    public string LocalPathToURL(string localPath)
    {
        string relativaPath = localPath.Replace(_localRepoPath, "");
        return string.Format(URL_FORMAT,
                                _userName,
                                _repositoryName,
                                relativaPath.Replace("\\", "/"));
    }
}
