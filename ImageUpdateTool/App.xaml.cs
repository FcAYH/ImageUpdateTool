using ImageUpdateTool.Utils;
using System.Diagnostics;

namespace ImageUpdateTool;

public partial class App : Application
{ 
	public App()
	{
		InitializeComponent();

		MainPage = new AppShell();
	}

    protected override Window CreateWindow(IActivationState activationState)
    {
        var window = base.CreateWindow(activationState);
		window.Destroying += OnWindowDestroy;
		return window;
    }



    private void OnWindowDestroy(object sender, EventArgs e)
    {
        // 在窗口关闭之后，将之前开启的，没运行结束的git进程杀掉
        // TODO: 在后台存在没运行结束的git进程时，应当询问用户是否确认退出程序
        //       然而这个功能还不会写
        CommandRunner.CloseAll();
    }
}
