using ErrorEventArgs = ImageUpdateTool.Utils.Events.ErrorEventArgs;
using ErrorEventHandler = ImageUpdateTool.Utils.Events.ErrorEventHandler;
using ServiceProvider = ImageUpdateTool.Utils.Tools.ServiceProvider;
using System.Windows.Input;
using ImageUpdateTool.ViewModels;
using System.Diagnostics;


namespace ImageUpdateTool.Views;

public partial class FunctionButtons : ContentView
{
    private FunctionButtonsViewModel _functionButtonsVM;

    public event ErrorEventHandler ErrorOccurred;

    public FunctionButtons()
    {
        InitializeComponent();

        // 因为 MAUI 当前还不支持 Custom View 的构造函数依赖注入，
        // 所以这里需要自己手动控制一下
        var vm = ServiceProvider.GetService<FunctionButtonsViewModel>();
        _functionButtonsVM = vm;
        BindingContext = _functionButtonsVM;
    }

    public FunctionButtons(FunctionButtonsViewModel viewModel)
    {
        // 直接在 MainPage.xaml 中调用 <views:FunctionButtons/>
        // 是不会调用这个构造函数的，需要等MAUI支持
        InitializeComponent();
        _functionButtonsVM = viewModel;
        BindingContext = _functionButtonsVM;
    }

    private void ExecuteError(ErrorEventArgs e)
    {
        ErrorOccurred.Invoke(this, e);
    }
}