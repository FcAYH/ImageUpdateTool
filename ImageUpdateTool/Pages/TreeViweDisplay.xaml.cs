using ImageUpdateTool.Models;

using Item = ImageUpdateTool.Models.TreeView.Item;
using Group = ImageUpdateTool.Models.TreeView.Group;

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

        Group rootGroup = new();
        rootGroup.Name = "Github Repo";

        GenerateGroup(rootGroup, repoDir);

        FolderList.RootNodes = FolderList.ProcessGroups(rootGroup);
    }

    private void GenerateGroup(Group parent, DirectoryInfo directory)
    {
        foreach (var dir in directory.GetDirectories())
        {
            Group group = new();
            group.Name = dir.Name;
            parent.Children.Add(group);
            GenerateGroup(group, dir);
        }
    }
}