using ImageUpdateTool.Models;
using System.Diagnostics;

namespace ImageUpdateTool.Pages;

public partial class LoadingPage : ContentPage
{
    private readonly AppSettings _settings;

	public LoadingPage(AppSettings appSettings)
	{
		InitializeComponent();
        _settings = appSettings;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        bool result = await SelfTest();
        if (result)
        {
            // https://stackoverflow.com/questions/75821516/maui-shell-navigation-fails-rare-reproduction
            // 貌似是一个bug？需要delay一下，保证页面加载完了才能切换，不然会报一个unhandled异常
            await Task.Delay(100);
            ChooseStartPage();
        }
    }

    private async Task<bool> SelfTest()
	{
        /*
		 * 运行程序要经过三道检测
		 * 1. 检测本机是否安装了git
		 * 2. 检测git是否被写入环境变量
		 */

#if WINDOWS
        string gitKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Git_is1";
        Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(gitKey);

        if (key == null)
        {
            LoadingMessageLabel.Text = "缺少Git...请先安装Git";
            await DisplayAlert("缺失Git!", "未在计算机中检测到Git程序，请确保安装Git，并将Git路径放入环境变量中。", "OK");
            return false;
        }

        string path = Environment.GetEnvironmentVariable("Path");
        if (!path.Contains("Git\\cmd"))
        {
            LoadingMessageLabel.Text = "需要将Git添加到环境变量中...";
            await DisplayAlert("环境变量缺失", "请将Git路径放入环境变量中。", "OK");
            return false;
        }
#elif ANDROID
// TODO: Android的检测 
#elif IOS || MACCATALYST
// TODO: iOS的检测
#endif

        return true;
    }

    private async void ChooseStartPage()
	{
        //await Shell.Current.GoToAsync("//TechTestPage");
        // 如果图床仓库URL为空，则展示Settings页面，
        // 否则直接进入Main Page
        if (_settings.ImageRepositoryURL == string.Empty)
        {
            await Shell.Current.GoToAsync("//SettingsPage");
        }
        else
        {
            (Shell.Current as AppShell).UnlockMainPage();
            await Shell.Current.GoToAsync("//TechTestPage");
        }
    }
}