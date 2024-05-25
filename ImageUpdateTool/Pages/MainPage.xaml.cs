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
            每次导航到 MainPage 时，都会重新读取 AppSettings
            这里不使用事件机制触发是因为首次设置 AppSettings 后需要 clone 仓库
            close 操作一方面需要构造一个 ImageRepositoryModel，另一方面需要用 MainPage 中的进度条展示进度。
        */
        _mainVM.UpdateSettings();
    }

    private async void OnErrorOccurred(object sender, ErrorEventArgs e)
    {
        await DisplayAlert(e.Title, e.Message, "OK");
    }
}