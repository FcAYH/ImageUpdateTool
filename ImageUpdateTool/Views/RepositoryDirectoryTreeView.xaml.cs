using ImageUpdateTool.ViewModels;
using ServiceProvider = ImageUpdateTool.Utils.Tools.ServiceProvider;

namespace ImageUpdateTool.Views;

public partial class RepositoryDirectoryTreeView : ContentView
{
	public RepositoryDirectoryTreeView()
	{
		InitializeComponent();

		var vm = ServiceProvider.GetService<RepositoryDirectoryViewModel>();
		BindingContext = vm;
	}
}