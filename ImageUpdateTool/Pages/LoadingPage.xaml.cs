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
            // ò����һ��bug����Ҫdelayһ�£���֤ҳ��������˲����л�����Ȼ�ᱨһ��unhandled�쳣
            await Task.Delay(100);
            ChooseStartPage();
        }
    }

    private async Task<bool> SelfTest()
    {
        /*
		 * ���г���Ҫ�����������
		 * 1. ��Ȿ���Ƿ�װ��git
		 * 2. ���git�Ƿ�д�뻷������
		 */

#if WINDOWS
        string gitKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Git_is1";
        Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(gitKey);

        if (key == null)
        {
            LoadingMessageLabel.Text = "ȱ��Git...���Ȱ�װGit";
            await DisplayAlert("ȱʧGit!", "δ�ڼ�����м�⵽Git������ȷ����װGit������Git·�����뻷�������С�", "OK");
            return false;
        }

        string path = Environment.GetEnvironmentVariable("Path");
        if (!path.Contains("Git\\cmd"))
        {
            LoadingMessageLabel.Text = "��Ҫ��Git��ӵ�����������...";
            await DisplayAlert("��������ȱʧ", "�뽫Git·�����뻷�������С�", "OK");
            return false;
        }
#elif ANDROID
// TODO: Android�ļ�� 
#elif IOS || MACCATALYST
// TODO: iOS�ļ��
#endif

        return true;
    }

    private async void ChooseStartPage()
    {
        // ���ͼ���ֿ� URL Ϊ�գ���չʾ Settings ҳ�棬
        // ����ֱ�ӽ��� Main Page
        if (_settings.ImageRepositoryURL == string.Empty)
        {
            await Shell.Current.GoToAsync("//SettingsPage");
        }
        else
        {
            (Shell.Current as AppShell).UnlockMainPage();
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}