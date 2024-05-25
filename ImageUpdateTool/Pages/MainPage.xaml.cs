using ImageUpdateTool.Models;
using ImageUpdateTool.Utils;
using ImageUpdateTool.Utils.Events;
using ImageUpdateTool.ViewModels;
using System.Diagnostics;
using System.Text.RegularExpressions;
using ErrorEventArgs = ImageUpdateTool.Utils.Events.ErrorEventArgs;

namespace ImageUpdateTool.Pages;

public partial class MainPage : ContentPage
{
    private MainViewModel _mainVM;

    public MainPage(MainViewModel mainVM)
    {
        InitializeComponent();
        _mainVM = mainVM;
        BindingContext = _mainVM;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        /*
            ÿ�ε����� MainPage ʱ���������¶�ȡ AppSettings
            ���ﲻʹ���¼����ƴ�������Ϊ�״����� AppSettings ����Ҫ clone �ֿ�
            close ����һ������Ҫ����һ�� ImageRepositoryModel����һ������Ҫ�� MainPage �еĽ�����չʾ���ȡ�
        */
        _mainVM.UpdateSettings();
    }

    private async void OnErrorOccurred(object sender, ErrorEventArgs e)
    {
        await DisplayAlert(e.Title, e.Message, "OK");
    }
}