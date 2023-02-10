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

	private void ChooseStartPage()
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
	}

	public void UnlockMainPage()
	{
        MainPageItem.IsEnabled = true;
        MainPageItem.IsVisible = true;
    }
}
