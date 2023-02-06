using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using UraniumUI.Material.Controls;
using Folder = ImageUpdateTool.Models.Folder;
using ImageUpdateTool.Views;

namespace ImageUpdateTool.Pages;

public partial class MainPage : ContentPage
{
    private Models.ImageRepo _imageRepo;

    private List<ImageArea> _imageList = new List<ImageArea>();

    public enum GitStatus
    {
        FailToPull, Success, FailToAdd, FailToCommit, FailToPush
    }
    private GitStatus _gitStatus = GitStatus.Success;
    public GitStatus Status
    {
        get
        {
            return _gitStatus;
        }
        private set
        {
            _gitStatus = value;
            RetryButton.IsEnabled = _gitStatus != GitStatus.Success;
        }
    }

    private string _newFileLocalPath;

    private const int IMAGE_AREA_WIDTH = 200;
    private const int IMAGE_AREA_HEIGHT = 150;

    private Folder _currentFolder;

    private bool ___selected = false; // do not use it directly
    private bool _hasSelectedAFolder 
    { 
        get => ___selected;
        set
        {
            ___selected= value;
            if (!value)
            {
                ImageDisplayGrid.Clear();
            }
        }
    }

    public MainPage()
    {
        InitializeComponent();
        InitializeGitStatus();
        _imageRepo = (Models.ImageRepo)BindingContext;

        InitializeFolderTree();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        Window.Destroying += Window_Destroying;
        ImageDisplayGrid.SizeChanged += DisplayGrid_SizeChanged;
    }

    private void DisplayGrid_SizeChanged(object sender, EventArgs e)
    {
        if (_hasSelectedAFolder)
        {
            ResizeImageDisplayGrid();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

    }

    private void Window_Destroying(object sender, EventArgs e)
    {
        SaveGitStatus();
    }

    private void InitializeFolderTree()
    {
        DirectoryInfo repoDir = new DirectoryInfo(_imageRepo.LocalRepoPath);

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

    private void InitializeGitStatus()
    {
        string localPath = FileSystem.AppDataDirectory;
        string statusSavePath = Path.Combine(localPath, "gitStatus.txt");

        if (!File.Exists(statusSavePath))
        {
            File.Create(statusSavePath);
            Status = GitStatus.Success;
        }
        else
        {
            string status = File.ReadAllText(statusSavePath);
            try
            {
                Status = (GitStatus)Enum.Parse(typeof(GitStatus), status);
            }
            catch(Exception)
            {
                Status = GitStatus.Success;
            }
        }
    }

    public void SaveGitStatus()
    {
        string localPath = FileSystem.AppDataDirectory;
        string statusSavePath = Path.Combine(localPath, "gitStatus.txt");

        if (!File.Exists(statusSavePath))
            File.Create(statusSavePath);
        File.WriteAllText(statusSavePath, Status.ToString());
    }

    private async void GitPullButton_Clicked(object sender, EventArgs e)
    {
        string error = _imageRepo.CallGitPull();
        if (!string.IsNullOrEmpty(error))
        {
            Status = GitStatus.FailToPull;
            await DisplayAlert("Error", $"拉取仓库内容失败，错误信息:\n{error}\n请检查后点击Retry", "Yes");
        }
        else
        {
            Status = GitStatus.Success;
            await DisplayAlert("Success", "拉取完成", "Yes");
        }
    }

    private async void SelectImageButton_Clicked(object sender, EventArgs e)
    {
        var photo = await MediaPicker.PickPhotoAsync();

        if (photo != null)
        {
            CopyURLButton.IsEnabled = false;

            // Move photo to _localRepoPath
            string dateTime = DateTime.Now.ToString("yyyy/MM/dd");
            string folderPath = Path.Combine(_imageRepo.LocalRepoPath, dateTime);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                InitializeFolderTree();
            }

            // Use md5 as new filename
            string extensionName = photo.FullPath.Split('.').Last();
            FileStream file = new(photo.FullPath, System.IO.FileMode.Open);
            MD5 md5 = MD5.Create();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();
            StringBuilder sb = new();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            sb.Append('.' + extensionName);
            string newFileName = sb.ToString();
            string newFilePath = Path.Combine(folderPath, newFileName);

            File.Move(photo.FullPath, newFilePath);

            // add, commit, push
            _newFileLocalPath = dateTime + "/" + newFileName;
            
            string error = _imageRepo.CallGitAdd(_newFileLocalPath);
            if (!string.IsNullOrEmpty(error))
            {
                Status = GitStatus.FailToAdd;
                await DisplayAlert("Error", $"新增内容失败，错误原因:\n{error}\n请检查后点击Retry", "Yes");
                return;
            }

            error = _imageRepo.CallGitCommit($"add new image {newFileName}");
            if (!string.IsNullOrEmpty(error))
            {
                Status = GitStatus.FailToCommit;
                await DisplayAlert("Error", $"提交内容失败，错误原因:\n{error}\n请检查后点击Retry", "Yes");
                return;
            }

            error = _imageRepo.CallGitPush();
            if (!string.IsNullOrEmpty(error))
            {
                Status = GitStatus.FailToPush;
                await DisplayAlert("Error", $"推送更新失败，错误原因:\n{error}\n请检查后点击Retry", "Yes");
                return;
            }

            await DisplayAlert("Success", "上传成功!", "Yes");
            CopyURLButton.IsEnabled = true;
        }
    }

    private async void CopyUrlButton_Clicked(object sender, EventArgs e)
    {
        await Clipboard.SetTextAsync(_imageRepo.LastestImageUrl);
    }

    private async void OnRetry_Clicked(object sender, EventArgs e)
    {
        if (Status == GitStatus.FailToPull)
        {
            string error = _imageRepo.CallGitPull();
            if (!string.IsNullOrEmpty(error))
            {
                Status = GitStatus.FailToPull;
                await DisplayAlert("Error", "拉取仓库内容失败，请检查网络后点击Retry", "Yes");
            }
            else
            {
                Status = GitStatus.Success;
                await DisplayAlert("Success", "拉取完成", "Yes");
            }
        }

        if (Status == GitStatus.FailToAdd)
        {
            string error = _imageRepo.CallGitAdd(_newFileLocalPath);
            if (!string.IsNullOrEmpty(error))
            {
                Status = GitStatus.FailToAdd;
                await DisplayAlert("Error", $"新增内容失败，错误原因:\n{error}\n请检查后点击Retry", "Yes");
                return;
            }
            Status = GitStatus.FailToCommit;
        }

        if (Status == GitStatus.FailToCommit)
        {
            string error = _imageRepo.CallGitCommit($"add new image {_newFileLocalPath.Split(new char[] {'/', '\\'}).Last()}");
            if (!string.IsNullOrEmpty(error))
            {
                Status = GitStatus.FailToCommit;
                await DisplayAlert("Error", $"提交内容失败，错误原因:\n{error}\n请检查后点击Retry", "Yes");
                return;
            }
            Status = GitStatus.FailToPush;
        }

        if (Status == GitStatus.FailToPush)
        {
            string error = _imageRepo.CallGitPush();
            if (!string.IsNullOrEmpty(error))
            {
                Status = GitStatus.FailToPush;
                await DisplayAlert("Error", $"推送修改失败，错误原因:\n{error}\n请检查后点击Retry", "Yes");
                return;
            }
            else
            {
                Status = GitStatus.Success;
                await DisplayAlert("Success", "上传成功", "Yes");
            }
        }
    }

    private void OnOpenRepoButton_Clicked(object sender, EventArgs e)
    {
        Process.Start("explorer.exe", _imageRepo.LocalRepoPath);
    }

    private void FolderButton_Cilcked(object sender, EventArgs e)
    {
        /*
         * sender => Button
         * Parent => Grid
         * Parent.Parent => ContentView
         * Parent.Parent.Parent => HorizontalStackLayout
         * Parent.Parent.Parent.Parent => TreeViewNodeHolderView
         */
        var folder = (sender as Button).BindingContext as Folder;
        if (!folder.IsLeaf)
        {
            var holder = ((sender as Button).Parent.Parent.Parent.Parent as TreeViewNodeHolderView);
            holder.IsExpanded = !holder.IsExpanded;
            var icon = ((sender as Button).Parent as Grid).FindByName<Label>("FolderIcon");
            icon.Text = holder.IsExpanded ? "📂" : "📁";
        }
        else
        {
            if (_currentFolder != folder)
            {
                _hasSelectedAFolder = true;
                _currentFolder = folder;
                _imageList.Clear();

                if (Directory.Exists(folder.Path))
                {
                    var dir = new DirectoryInfo(folder.Path);
                    foreach (var img in dir.GetFiles())
                    {
                        ImageArea area = new ImageArea();
                        area.ImageSource = img.FullName;
                        area.ImageSize = img.Length;
                        area.ImageURL = _imageRepo.LocalPathToURL(img.FullName);
                        _imageList.Add(area);
                    }
                }

                RearrangeImages();
            }
        }
    }

    private void ResizeImageDisplayGrid()
    {
        int previousGridColumnCount = ImageDisplayGrid.ColumnDefinitions.Count;
        int previousGridRowCount = ImageDisplayGrid.RowDefinitions.Count;
        int gridWidth = (int)ImageDisplayGrid.Width;
        int gridCount = previousGridColumnCount * previousGridRowCount;

        int newColumnCount = gridWidth / IMAGE_AREA_WIDTH;
        int newRowCount = (int)Math.Ceiling(gridCount / (double)newColumnCount);
        

        if (previousGridRowCount == newRowCount
               && previousGridColumnCount == newColumnCount)
            return;

        ImageDisplayGrid.RowDefinitions = new RowDefinitionCollection(
                                                GenerateGridRow(newRowCount));
        ImageDisplayGrid.ColumnDefinitions = new ColumnDefinitionCollection(
                                                GenerateGridColumn(newColumnCount));
        RearrangeImages();
    }

    private RowDefinition[] GenerateGridRow(int count)
    {
        var row = new RowDefinition[count];
        for (int i = 0; i < count; i++)
        {
            row[i] = new RowDefinition(IMAGE_AREA_HEIGHT);
        }
        return row;
    }

    private ColumnDefinition[] GenerateGridColumn(int count)
    {
        var column = new ColumnDefinition[count];
        for (int i = 0; i < count; i++)
        {
            column[i] = new ColumnDefinition();
        }
        return column;
    }

    private void RearrangeImages()
    {
        ImageDisplayGrid.Clear();
        int index = 0;
        for (int i = 0; i < ImageDisplayGrid.RowDefinitions.Count; i++)
        {
            for (int j = 0; j < ImageDisplayGrid.ColumnDefinitions.Count; j++)
            {
                if (index >= _imageList.Count) 
                    return;
                var content = _imageList[index];
                ImageDisplayGrid.Add(content, j, i);
                index++;
            }
        }
    }

    private void ImageDisplayGrid_Loaded(object sender, EventArgs e)
    {
        int gridWidth = (int)ImageDisplayGrid.Width;
        int gridHeight = (int)ImageDisplayGrid.Height;

        ImageDisplayGrid.RowDefinitions = new RowDefinitionCollection(
                                        GenerateGridRow(gridHeight / IMAGE_AREA_HEIGHT));
        ImageDisplayGrid.ColumnDefinitions = new ColumnDefinitionCollection(
                                                GenerateGridColumn(gridWidth / IMAGE_AREA_WIDTH));
    }
}