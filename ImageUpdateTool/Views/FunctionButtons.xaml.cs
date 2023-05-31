using ErrorEventArgs = ImageUpdateTool.Utils.Events.ErrorEventArgs;
using ErrorEventHandler = ImageUpdateTool.Utils.Events.ErrorEventHandler;
using System.Windows.Input;

namespace ImageUpdateTool.Views;

public partial class FunctionButtons : ContentView
{
    public static BindableProperty ErrorOccurredProperty = BindableProperty.Create(
		nameof(ErrorOccurred), typeof(ErrorEventHandler), typeof(FunctionButtons), null);

	public EventHandler ErrorOccurred
	{
        get => (EventHandler)GetValue(ErrorOccurredProperty);
        set => SetValue(ErrorOccurredProperty, value);
    }

    public FunctionButtons()
	{
		InitializeComponent();
	}

	private void ExecuteError(ErrorEventArgs e)
	{
		ErrorOccurred.Invoke(this, e);
	}
}