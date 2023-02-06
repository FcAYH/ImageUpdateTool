using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageUpdateTool.Models;

internal class ImageRepo
{
    public string LastestImageUrl { get; private set; }
    public string LocalRepoPath { get { return _localRepoPath; } }

    public const string URL_PREFIX = "https://cdn.jsdelivr.net/gh/FcAYH/Images/";
    private const string GIT_REPO_URL = "https://github.com/FcAYH/Images.git";
    private const string USER_NAME = "FcAYH";
    private const string USER_EMAIL = "1473988037@qq.com";
    private const string ROOT_FOLDER_NAME = "ImageUpdateTool_GitRepos";
    private const string REPO_FOLDER_NAME = "Images";

    private string _applicationFolderPath = "";
    private string _localRepoPath = "";

    private Logic.CommandRunner _gitProcess;

    public ImageRepo()
    {
        string localPath = FileSystem.AppDataDirectory;
        _applicationFolderPath = Path.Combine(localPath, ROOT_FOLDER_NAME);
        _localRepoPath = Path.Combine(_applicationFolderPath, REPO_FOLDER_NAME);

        if (!Directory.Exists(_applicationFolderPath))
            Directory.CreateDirectory(_applicationFolderPath);

        _gitProcess = new("git", _localRepoPath);
    }

    public string CallGitPull()
    {
        string result;
        if (!Directory.Exists(_localRepoPath))
        {
            // git clone
            Logic.CommandRunner gitClone = new("git", _applicationFolderPath);
            result = gitClone.Run($"clone {GIT_REPO_URL}");
        }
        else
        {
            // git pull
            result = _gitProcess.Run("pull");
        }

        return result;
    }

    public string CallGitPush()
    {
        return _gitProcess.Run("push");
    }

    public string CallGitCommit(string message)
    {
        return _gitProcess.Run($"commit -m \"{message}\"");
    }

    public string CallGitAdd(string filePath)
    {
        LastestImageUrl = URL_PREFIX + filePath;
        return _gitProcess.Run($"add {filePath}");
    }

    public string LocalPathToURL(string localPath)
    {
        return localPath.Replace(_localRepoPath + "\\", URL_PREFIX).Replace("\\", "/");
    }
}
