using ErrorEventArgs = ImageUpdateTool.Utils.Events.ErrorEventArgs;
using ErrorEventHandler = ImageUpdateTool.Utils.Events.ErrorEventHandler;
using System.Windows.Input;
using ImageUpdateTool.ViewModels;

namespace ImageUpdateTool.Views;

public partial class FunctionButtons : ContentView
{
	private FunctionButtonsViewModel _functionButtonsVM;

    public static BindableProperty ErrorOccurredProperty = BindableProperty.Create(
		nameof(ErrorOccurred), typeof(ErrorEventHandler), typeof(FunctionButtons), null);

	public EventHandler ErrorOccurred
	{
        get => (EventHandler)GetValue(ErrorOccurredProperty);
        set => SetValue(ErrorOccurredProperty, value);
    }

    public FunctionButtons(FunctionButtonsViewModel viewModel)
	{
		InitializeComponent();

		_functionButtonsVM = viewModel;
	}

	private void ExecuteError(ErrorEventArgs e)
	{
		ErrorOccurred.Invoke(this, e);
	}
}