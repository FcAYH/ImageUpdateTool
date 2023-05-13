using System.Diagnostics;

namespace ImageUpdateTool;

public partial class App : Application
{ 
	public App()
	{
		InitializeComponent();

		MainPage = new AppShell();
	}

    public override void CloseWindow(Window window)
    {
		Debug.WriteLine("close");

        base.CloseWindow(window);
    }
}
