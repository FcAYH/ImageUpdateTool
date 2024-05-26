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

    #region Fields
    // AppSettings实际会操作的参数就只有AppTheme和Language
    // 剩下的参数AppSettings只负责保存和读取，具体的实现逻辑放在Image Repository Model中
    private AppTheme _colorTheme;
    private Language _language;
    private string _gitUserName;
    private string _gitUserEmail;
    private string _imageRepositoryURL;
    private string _localStorageLocation; // TODO: 类型待定
    #endregion

    #region Properties
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
        GitUserName = userName;
        GitUserEmail = userEmail;
        ImageRepositoryURL = url;
        LocalStorageLocation = location;

        Save();
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
