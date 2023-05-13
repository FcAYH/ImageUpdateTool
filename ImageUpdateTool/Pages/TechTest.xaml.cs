

using ImageUpdateTool.Models;
using System.Diagnostics;

namespace ImageUpdateTool.Pages;

public partial class TechTest : ContentPage
{
    public TechTest()
    {
        InitializeComponent();

        InitializeFolderTree();
    }

    private void InitializeFolderTree()
    {
        DirectoryInfo repoDir = new(@"C:\Users\F_CIL\AppData\Local\Packages\C9481A9D-76F1-41AF-90C4-B5EBB33523A6_aw6425nzr74p4\LocalState\ImageUpdateTool_GitRepos\Images");

        Folder rootFolder = new();
        GenerateFolder(rootFolder, repoDir);

        FolderTree.ItemsSource = (System.Collections.IList)rootFolder.Children;
    }

    private void GenerateFolder(Folder parent, DirectoryInfo directory, int layer = 0)
    {
        foreach (var dir in directory.GetDirectories())
        {
            if (dir.Name == ".git") continue;
            Folder folder = new(dir.Name, dir.FullName);
            if (dir.GetDirectories().Length > 0)
                folder.IsLeaf = false;
            folder.ButtonWidthRequest -= layer * 10;
            parent.Children.Add(folder);
            GenerateFolder(folder, dir, layer + 1);
        }
    }
}