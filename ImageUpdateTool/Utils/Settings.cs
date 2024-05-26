using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ImageUpdateTool.Utils;

public class Settings
{
    private const string DEFAULT_ROOT_FOLDER_NAME = "ImageUpdateTool_GitRepos";

    /// <summary>
    /// 当前软件使用的颜色主题
    /// </summary>
    public AppTheme CurrentTheme
    {
        get
        {
            if (string.IsNullOrEmpty(_currentTheme))
            {
                _currentTheme = Preferences.Get(nameof(CurrentTheme), string.Empty);
            }

            var currentTheme = AppTheme.Unspecified;
            try
            {
                currentTheme = Enum.Parse<AppTheme>(_currentTheme);
            }
            catch { }

            return currentTheme;
        }
        set
        {
            _currentTheme = value.ToString();
            Application.Current.UserAppTheme = value;
            Preferences.Set(nameof(CurrentTheme), _currentTheme);
        }
    }

    /// <summary>
    /// 当前软件所管理的图床的 git Web URL
    /// </summary>
    public string GitURL
    {
        get
        {
            if (string.IsNullOrEmpty(_gitURL))
            {
                _gitURL = Preferences.Get(nameof(GitURL), string.Empty);
            }

            return _gitURL;
        }
        set
        {
            _gitURL = value;
            Preferences.Set(nameof(GitURL), _gitURL);
        }
    }

    /// <summary>
    /// 当前软件用于在本地存储 git 仓库的目录
    /// </summary>
    public string LocalStoragePath
    {
        get
        {
            if (string.IsNullOrEmpty(_localStoragePath))
            {
                _localStoragePath = Preferences.Get(nameof(LocalStoragePath), string.Empty);
            }

            if (string.IsNullOrEmpty(_localStoragePath))
            {
                _localStoragePath = Path.Combine(FileSystem.AppDataDirectory, DEFAULT_ROOT_FOLDER_NAME);
                if (!Directory.Exists(_localStoragePath))
                    Directory.CreateDirectory(_localStoragePath);
            }

            return _localStoragePath;
        }
        set
        {
            _localStoragePath = value;
            Preferences.Set(nameof(LocalStoragePath), _localStoragePath);
        }

    }

    private string _currentTheme = string.Empty;
    private string _gitURL = string.Empty;
    private string _localStoragePath = string.Empty;

    public Settings()
    {
        Application.Current.UserAppTheme = CurrentTheme;
    }

    public void SetLocalStoragePathToDefault()
    {
        LocalStoragePath = Path.Combine(FileSystem.AppDataDirectory, DEFAULT_ROOT_FOLDER_NAME);
    }
}
