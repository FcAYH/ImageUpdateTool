using ImageUpdateTool.Utils;
using System.Diagnostics;

namespace ImageUpdateTool;

public partial class AppShell : Shell
{
	private static Settings _appSettings;
	public static Settings AppSettings1 
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

		FlyoutBehavior = FlyoutBehavior.Disabled;
		CurrentItem = LoadingPageItem;
	}


    public void UnlockMainPage()
	{
		FlyoutBehavior = FlyoutBehavior.Flyout;
        MainPageItem.IsEnabled = true;
        MainPageItem.IsVisible = true;
    }
}

