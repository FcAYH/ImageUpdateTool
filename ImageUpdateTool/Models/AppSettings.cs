using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Language = ImageUpdateTool.Utils.Language;

namespace ImageUpdateTool.Models;

public class AppSettings
{
    private const string DEFAULT_ROOT_FOLDER_NAME = "ImageUpdateTool_GitRepos";

    #region Properties
    private AppTheme _colorTheme;
    private Language _language;
    private string _gitUserName;
    private string _gitUserEmail;
    private string _imageRepositoryURL;
    private string _localStorageLocation; // TODO: 类型待定

    // 仅改变url、仅改变location、同时改变url和location 需要用不同的逻辑处理
    private bool _urlChanged;
    private bool _locationChanged;
    #endregion

    #region Attributes
    public AppTheme ColorTheme
    {
        get => _colorTheme;
        set
        {
            if (_colorTheme != value)
            {
                _colorTheme = value;
                Application.Current.UserAppTheme = value;
            }
        }
    }

    public Language Language
    {
        get => _language;
        set
        {
            if (_language != value)
            {
                _language = value;
                // TODO：切换语言
            }
        }
    }

    public string GitUserName
    {
        get => _gitUserName;
        set
        {
            if (_gitUserName != value)
            {
                _gitUserName = value;
                // TODO: 更新git用户名
                // 注：这里进更改本地的，而不会影响全局，所以应该在建好仓库后去修改
            }
        }
    }

    public string GitUserEmail
    {
        get => _gitUserEmail;
        set
        {
            if (_gitUserEmail != value)
            {
                _gitUserEmail = value;
                // TODO: 更新git用户邮箱
            }
        }
    }

    public string ImageRepositoryURL
    {
        get => _imageRepositoryURL;
        set
        {
            if (_imageRepositoryURL != value)
            {
                _imageRepositoryURL = value;
                _urlChanged = true;
            }
        }
    }

    public string LocalStorageLocation
    {
        get => _localStorageLocation;
        set
        {
            if (_localStorageLocation != value)
            {
                _localStorageLocation = value;
                _locationChanged = true;
            }
        }
    }
    #endregion

    #region Events
    public event Action<string> OnImageRepositoryURLChanged;
    public event Action<string> OnLocalStorageLocationChanged;
    public event Action<string, string> OnUrlAndLocationChanged;
    #endregion

    public AppSettings()
    {
        // AppSettings内容简单，采用Preference存储
        try
        {
            ColorTheme = Enum.Parse<AppTheme>(Preferences.Get(nameof(ColorTheme), AppTheme.Unspecified.ToString()));
        }
        catch
        {
            ColorTheme = AppTheme.Unspecified;
        }

        try
        {
            Language = Enum.Parse<Language>(Preferences.Get(nameof(Language), Language.English.ToString()));
        }
        catch
        {
            Language = Language.English;
        }

        GitUserName = Preferences.Get(nameof(GitUserName), string.Empty);
        GitUserEmail = Preferences.Get(nameof(GitUserEmail), string.Empty);
        ImageRepositoryURL = Preferences.Get(nameof(ImageRepositoryURL), string.Empty);
        LocalStorageLocation = Preferences.Get(nameof(LocalStorageLocation), GenerateDefaultStorageLocation());
    }

    public static string GenerateDefaultStorageLocation()
    {
        var defaultStorageLocation = Path.Combine(FileSystem.AppDataDirectory, DEFAULT_ROOT_FOLDER_NAME);
        if (!Directory.Exists(defaultStorageLocation))
        {
            Directory.CreateDirectory(defaultStorageLocation);
        }
        return defaultStorageLocation;
    }

    public void Apply(string userName, string userEmail, string url, string location)
    {
        _urlChanged = false;
        _locationChanged = false;

        GitUserName = userName;
        GitUserEmail = userEmail;
        ImageRepositoryURL = url;
        LocalStorageLocation = location;

        Save();

        if (_urlChanged && _locationChanged)
            OnUrlAndLocationChanged(url, location);
        else if (_urlChanged)
            OnImageRepositoryURLChanged(url);
        else if (_locationChanged)
            OnLocalStorageLocationChanged(location);
    }

    public void Save()
    {
        Preferences.Set(nameof(ColorTheme), ColorTheme.ToString());
        Preferences.Set(nameof(Language), Language.ToString());
        Preferences.Set(nameof(GitUserName), GitUserName);
        Preferences.Set(nameof(GitUserEmail), GitUserEmail);
        Preferences.Set(nameof(ImageRepositoryURL), ImageRepositoryURL);
        Preferences.Set(nameof(LocalStorageLocation), LocalStorageLocation);
    }
}
