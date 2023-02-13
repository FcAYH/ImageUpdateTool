using ImageUpdateTool.Utils;
using System.Diagnostics;

namespace ImageUpdateTool;

public partial class AppShell : Shell
{
	private static Settings _appSettings;
	public static Settings AppSettings 
	{ 
		get
		{
			_appSettings ??= new Settings();
			
			return _appSettings;
		} 
	}

	public AppShell()
	{
		InitializeComponent();

		ChooseStartPage();
    }

	private async void ChooseStartPage()
	{
		if (AppSettings.LocalStoragePath == string.Empty
			|| AppSettings.GitURL == string.Empty) 
		{
			CurrentItem = SettingsPageItem;
		}
		else
		{
			UnlockMainPage();
			CurrentItem = MainPageItem;
		}

#if WINDOWS
		string gitKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Git_is1";
        Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(gitKey);

		if (key == null)
		{
			await DisplayAlert("缺失Git!", "未在计算机中检测到Git程序，请确保安装Git，并将Git路径放入环境变量中。", "OK");
			CurrentItem = AboutPageItem;
			LockPages();
		}

		string path = Environment.GetEnvironmentVariable("Path");
		if (!path.Contains("Git\\cmd"))
		{
            await DisplayAlert("环境变量缺失", "请将Git路径放入环境变量中。", "OK");
            CurrentItem = AboutPageItem;
            LockPages();
        }
#endif
    }

    public void UnlockMainPage()
	{
        MainPageItem.IsEnabled = true;
        MainPageItem.IsVisible = true;
    }

	private void LockPages() 
	{
        MainPageItem.IsEnabled = false;
        MainPageItem.IsVisible = false;

		SettingsPageItem.IsEnabled = false;
		SettingsPageItem.IsVisible = false;
    }
}

