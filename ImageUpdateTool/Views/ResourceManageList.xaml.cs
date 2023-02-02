using ImageUpdateTool.Controls;

namespace ImageUpdateTool.Views;

public partial class ResourceManageList : ContentView
{
	public static readonly BindableProperty RepoPathProperty = BindableProperty.Create(nameof(RepoPath), typeof(string), typeof(ResourceManageList), string.Empty);

    public string RepoPath
	{
		get => (string)GetValue(RepoPathProperty);
		set => SetValue(RepoPathProperty, value);
	}

	public ResourceManageList()
	{
		InitializeComponent();
	}

	public void InitializeResourceList()
	{
		DirectoryInfo repoDir = new DirectoryInfo(RepoPath);
		foreach (var dir in repoDir.GetDirectories())
		{ 
			FolderButton button = new ();
			button.Text = dir.Name;
			button.Style = (Style)Resources["FolderButton"];
			button.Clicked += Test;
			Directories.Add(button);
		}
	}

	private void Test(object sender, EventArgs e)
	{
		throw new NotImplementedException();
	}
}