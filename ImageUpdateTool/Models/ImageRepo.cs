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

    private const string URL_PREFIX = "https://raw.githubusercontent.com/FcAYH/Images/master/";
    private const string GIT_REPO_URL = "https://github.com/FcAYH/Images.git";
    private const string USER_NAME = "FcAYH";
    private const string USER_EMAIL = "1473988037@qq.com";
    private const string ROOT_FOLDER_NAME = "ImageUpdateTool_GitRepos";
    private const string REPO_FOLDER_NAME = "Images";

    private string _applicationFolderPath = "";
    private string _localRepoPath = "C:\\Users\\F_CIL\\AppData\\Local\\Packages\\C9481A9D-76F1-41AF-90C4-B5EBB33523A6_9zz4h110yvjzm\\LocalCache\\Local\\ImageUpdateTool_GitRepos\\Images";

    private Logic.CommandRunner _gitProcess;

    public ImageRepo()
    {
        string localPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _applicationFolderPath = Path.Combine(localPath, ROOT_FOLDER_NAME);
        //_localRepoPath = Path.Combine(_applicationFolderPath, REPO_FOLDER_NAME);

        if (!Directory.Exists(_applicationFolderPath))
            Directory.CreateDirectory(_applicationFolderPath);

        _gitProcess = new("git", @"C:\Users\F_CIL\AppData\Local\Packages\C9481A9D-76F1-41AF-90C4-B5EBB33523A6_9zz4h110yvjzm\LocalCache\Local\ImageUpdateTool_GitRepos\Images");
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
}
