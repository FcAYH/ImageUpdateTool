using ImageUpdateTool.Models;

using System.Collections.ObjectModel;

namespace ImageUpdateTool.Pages;

public partial class TreeViweDisplay : ContentPage
{
	public TreeViweDisplay()
	{
		InitializeComponent();

        InitializeFolderList();
    }

    private void InitializeFolderList()
    {
        DirectoryInfo repoDir = new DirectoryInfo("C:\\Users\\F_CIL\\AppData\\Local\\Packages\\C9481A9D-76F1-41AF-90C4-B5EBB33523A6_9zz4h110yvjzm\\LocalState\\ImageUpdateTool_GitRepos\\Images");

        Folder rootFolder = new();

        GenerateFolders(rootFolder, repoDir);

        FolderList.ItemsSource = (System.Collections.IList)rootFolder.Children;
    }

    private void GenerateFolders(Folder parent, DirectoryInfo directory)
    {
        foreach (var dir in directory.GetDirectories())
        {
            Folder folder = new()
            {
                Name = dir.Name
            };

            parent.Children.Add(folder);
            GenerateFolders(folder, dir);
        }
    }


    private void FolderButton_Clicked(object sender, EventArgs e)
    {
        string text = (sender as Button).Text;
        DisplayAlert("Show!", text, "ok");
    }
}