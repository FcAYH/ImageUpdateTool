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

        // 每次导航到MainPage时，都会重新读取AppSettings
        // 这里不使用事件机制触发是因为首次设置AppSettings后需要clone仓库
        // 一方面clone需要构造一个ImageRepositoryModel，另一方面想要用MainPage中的进度条
        // 展示clone的进度。
        _mainVM.UpdateSettings();
    }

    private async void OnErrorOccurred(object sender, ErrorEventArgs e)
    {
        await DisplayAlert(e.Title, e.Message, "OK");
    }
}