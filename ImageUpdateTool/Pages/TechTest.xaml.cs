using ImageUpdateTool.Models;
using ImageUpdateTool.Utils;
using ImageUpdateTool.Utils.Events;
using ImageUpdateTool.ViewModels;
using System.Diagnostics;
using System.Text.RegularExpressions;
using ErrorEventArgs = ImageUpdateTool.Utils.Events.ErrorEventArgs;

namespace ImageUpdateTool.Pages;

public partial class TechTest : ContentPage
{
    private MainViewModel _mainVM;

    public TechTest(MainViewModel mainVM)
    {
        InitializeComponent();
        _mainVM = mainVM;
        BindingContext = _mainVM;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        // ÿ�ε�����MainPageʱ���������¶�ȡAppSettings
        // ���ﲻʹ���¼����ƴ�������Ϊ�״�����AppSettings����Ҫclone�ֿ�
        // һ����clone��Ҫ����һ��ImageRepositoryModel����һ������Ҫ��MainPage�еĽ�����
        // չʾclone�Ľ��ȡ�
        _mainVM.UpdateSettings();
    }

    private async void OnErrorOccurred(object sender, ErrorEventArgs e)
    {
        await DisplayAlert(e.Title, e.Message, "OK");
    }
}