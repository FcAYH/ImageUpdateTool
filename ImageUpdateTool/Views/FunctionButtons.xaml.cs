using ErrorEventArgs = ImageUpdateTool.Utils.Events.ErrorEventArgs;
using ErrorEventHandler = ImageUpdateTool.Utils.Events.ErrorEventHandler;
using ServiceProvider =  ImageUpdateTool.Utils.Tools.ServiceProvider;
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

		// ��ΪMAUI��ǰ����֧��Custom View�Ĺ��캯������ע�룬
		// ����������Ҫ�Լ��ֶ�����һ��
		var vm = ServiceProvider.GetService<FunctionButtonsViewModel>();
		_functionButtonsVM = vm;
		BindingContext = _functionButtonsVM;
	}

    public FunctionButtons(FunctionButtonsViewModel viewModel)
	{
		// ֱ����MainPage.xaml�е���<views:FunctionButtons/>
		// �ǲ������������캯���ģ���Ҫ��MAUI֧��
		InitializeComponent();
		_functionButtonsVM = viewModel;
		BindingContext = _functionButtonsVM;
	}

	private void ExecuteError(ErrorEventArgs e)
	{
		ErrorOccurred.Invoke(this, e);
	}
}