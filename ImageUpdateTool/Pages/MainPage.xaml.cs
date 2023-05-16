using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using UraniumUI.Material.Controls;
using Folder = ImageUpdateTool.Models.Folder;
using ImageUpdateTool.Views;
using ImageUpdateTool.Models;
using ImageUpdateTool.Utils;

namespace ImageUpdateTool.Pages;

[QueryProperty(nameof(ImageRepoChanged), "ImageRepoChanged")]
public partial class MainPage : ContentPage
{
    public ImageRepo CurrentImageRepo
    {
        get => _currentImageRepo;
        set
        {
            _currentImageRepo = value;
            InitializeFolderTree();
        }
    }

    private ImageRepo _currentImageRepo;

    private bool _imageRepoChanged;
    public bool ImageRepoChanged
    {
        get => _imageRepoChanged;
        set
        {
            _imageRepoChanged = value;
            if (_imageRepoChanged)
            {
                CurrentImageRepo = new ImageRepo(AppShell.AppSettings.GitURL,
                                            AppShell.AppSettings.LocalStoragePath);
            }
        }
    }

    private List<ImageArea> _imageList = new List<ImageArea>();
   
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

    private string _newFileRelativePath;
    private string _removeFileRelativePath;

    private const int IMAGE_AREA_WIDTH = 200;
    private const int IMAGE_AREA_HEIGHT = 150;

    private Folder _currentFolder;

    private Progress<double> _gitProgress;

    public MainPage()
    {
        InitializeComponent();
        InitializeGitStatus();

        CurrentImageRepo = new ImageRepo(AppShell.AppSettings.GitURL,
                                            AppShell.AppSettings.LocalStoragePath);

        _gitProgress = new(value =>
        {
            GitProgress.ProgressTo(value, 250, Easing.Linear);
        });
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        Window.Destroying += Window_Destroying;
        ImageDisplayGrid.SizeChanged += DisplayGrid_SizeChanged;
    }

    private void DisplayGrid_SizeChanged(object sender, EventArgs e)
    {
        UpdateImageDisplayGrid();
    }

    private void Window_Destroying(object sender, EventArgs e)
    {
        SaveGitStatus();
    }

    private void InitializeFolderTree()
    {
        DirectoryInfo repoDir = new(_currentImageRepo.LocalRepoPath);

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
        var status = Preferences.Get(nameof(GitStatus), "Success");
        try
        {
            Status = (GitStatus)Enum.Parse(typeof(GitStatus), status);
        }
        catch (Exception)
        {
            Status = GitStatus.Success;
        }
    }

    public void SaveGitStatus()
    {
        Preferences.Set(nameof(GitStatus), Status.ToString());
    }

    private async void GitPullButton_Clicked(object sender, EventArgs e)
    {
        GitProgress.IsVisible = true;
        string error = await _currentImageRepo.CallGitPullAsync(_gitProgress);
        GitProgress.IsVisible = false;

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
            string folderPath = Path.Combine(_currentImageRepo.LocalRepoPath, dateTime);

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
            _newFileRelativePath = dateTime + "/" + newFileName;
            
            string error = _currentImageRepo.CallGitAdd(_newFileRelativePath);
            if (!string.IsNullOrEmpty(error))
            {
                Status = GitStatus.FailToAdd;
                await DisplayAlert("Error", $"新增内容失败，错误原因:\n{error}\n请检查后点击Retry", "Yes");
                return;
            }

            error = _currentImageRepo.CallGitCommit($"add new image {newFileName}");
            if (!string.IsNullOrEmpty(error))
            {
                Status = GitStatus.FailToCommit;
                await DisplayAlert("Error", $"提交内容失败，错误原因:\n{error}\n请检查后点击Retry", "Yes");
                return;
            }

            GitProgress.IsVisible = true;
            error = await _currentImageRepo.CallGitPushAsync(_gitProgress);
            GitProgress.IsVisible = false;

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
        await Clipboard.SetTextAsync(_currentImageRepo.LastestImageURL);
    }

    private async void OnRetry_Clicked(object sender, EventArgs e)
    {
        if (Status == GitStatus.FailToPull)
        {
            string error = _currentImageRepo.CallGitPull();
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
            string error = _currentImageRepo.CallGitAdd(_newFileRelativePath);
            if (!string.IsNullOrEmpty(error))
            {
                Status = GitStatus.FailToAdd;
                await DisplayAlert("Error", $"新增内容失败，错误原因:\n{error}\n请检查后点击Retry", "Yes");
                return;
            }
            Status = GitStatus.FailToCommit;
        }

        if (Status == GitStatus.FailToRemove)
        {
            string error = _currentImageRepo.CallGitRemove(_removeFileRelativePath);
            if (!string.IsNullOrEmpty(error))
            {
                Status = GitStatus.FailToRemove;
                await DisplayAlert("Error", $"删除图片失败，错误原因:\n{error}\n请检查后点击Retry", "Yes");
            }
            Status = GitStatus.FailToCommit;
        }

        if (Status == GitStatus.FailToCommit)
        {
            string error = _currentImageRepo.CallGitCommit($"add new image {_newFileRelativePath.Split(new char[] {'/', '\\'}).Last()}");
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
            string error = _currentImageRepo.CallGitPush();
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
        Process.Start("explorer.exe", _currentImageRepo.LocalRepoPath);
    }

    private void FolderButton_Clicked(object sender, EventArgs e)
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
                _currentFolder = folder;

                GenerateImageList(folder.Path);
                UpdateImageDisplayGrid();
            }
        }
    }

    private void GenerateImageList(string path)
    {
        _imageList.Clear();

        if (Directory.Exists(path))
        {
            var dir = new DirectoryInfo(path);
            foreach (var img in dir.GetFiles())
            {
                if (img.Extension == ".db")
                    continue;

                ImageArea area = new()
                {
                    ImageSize = img.Length,
                    ImageURL = _currentImageRepo.LocalPathToURL(img.FullName),
                };

                area.ImageSource = Enum.IsDefined(typeof(ImageExtension), img.Extension.ToLower().Trim('.'))
                                    ? img.FullName
                                    : "unable_to_preview.png";
                area.OnImageAreaClicked += OnImageClicked;
                area.OnImageDeleted += OnImageDeleted;
                _imageList.Add(area);
            }
        }
    }

    private void OnImageClicked(string imgPath)
    {
        var bgColor = new Color(BackgroundColor.Red, BackgroundColor.Blue, BackgroundColor.Green, 0.8f);
        ImagePreviewArea.BackgroundColor = bgColor;
        PreviewImage.Source = imgPath;
        ImagePreviewArea.IsVisible = true;
    }

    private async void OnImageDeleted(string imgPath)
    {
        if (!await DisplayAlert("Confrim", "确认要删除该图片嘛？", "Yes", "Cancel"))
        { 
            return; 
        }

        if (!Path.Exists(imgPath)) // 图片已不存在，理论上不会发生
        {
            return;
        }
      
        bool result = false;
        try
        {
            File.Delete(imgPath);
            result = true;

            // TODO：如果图片是该文件夹下的最后一张，则将文件夹一并删除
        }
        catch
        {
            var folder = Path.GetDirectoryName(imgPath);
            var name = Path.GetFileName(imgPath); 

            await DisplayAlert("Error", $"因为权限问题，无法删除图片{name}，\n在您点击OK后，会自动为您打开文件资源管理器，请在其中将{name}图片删除", "OK");
            CommandRunner explorer = new("explorer");
            explorer.Run(folder);
            result = await DisplayAlert("Confirm", $"如果您已经删除了图片{name}，请点击OK，如果您不想删除它，请点击Cancel", "OK", "Cancel");
        }

        if (result)
        {
            // LocalStoragePath是用反斜杠 '\' 的，imgPath是用正斜杠的 '/'，emm
            var localPath = AppShell.AppSettings.LocalStoragePath.Replace('\\', '/') + "/";
            var relativePath = imgPath.Replace(localPath, "");
            // 这时相对路径是：仓库名/year/month/day/image，需要去掉仓库名
            relativePath = relativePath[(relativePath.IndexOf('/') + 1)..];
            Debug.WriteLine(relativePath);

            var error = _currentImageRepo.CallGitRemove(relativePath);
            if (!string.IsNullOrEmpty(error))
            {
                Status = GitStatus.FailToRemove;
                await DisplayAlert("Error", $"删除图片失败，错误原因:\n{error}\n请检查后点击Retry", "Yes");
                return;
            }

            error = _currentImageRepo.CallGitCommit($"remove {relativePath}");
            if (!string.IsNullOrEmpty(error))
            {
                Status = GitStatus.FailToCommit;
                await DisplayAlert("Error", $"提交内容失败，错误原因:\n{error}\n请检查后点击Retry", "Yes");
                return;
            }

            GitProgress.IsVisible = true;
            error = await _currentImageRepo.CallGitPushAsync(_gitProgress);
            GitProgress.IsVisible = false;

            if (!string.IsNullOrEmpty(error))
            {
                Status = GitStatus.FailToPush;
                await DisplayAlert("Error", $"推送更新失败，错误原因:\n{error}\n请检查后点击Retry", "Yes");
                return;
            }
            
            
                //TODO：成功清理后要刷新图片的显示
                //      SelectImage 上传时同样忘记了，需要手动重新打开文件夹才会刷新
        }
    }

    private void ImagePreviewAreaCloseButton_Clicked(object sender, EventArgs e)
    {
        PreviewImage.Source = "";
        ImagePreviewArea.IsVisible = false;
    }

    private void UpdateImageDisplayGrid()
    {
        ResizeImageDisplayGrid();
        RearrangeImages();
    }

    private void ResizeImageDisplayGrid()
    {
        int currentGridColumnCount = ImageDisplayGrid.ColumnDefinitions.Count;
        int currentGridRowCount = ImageDisplayGrid.RowDefinitions.Count;
        int gridWidth = (int)ImageDisplayGrid.Width;
        int gridCountRequest = _imageList.Count;

        int newColumnCount = gridWidth / IMAGE_AREA_WIDTH;
        int newRowCount = (int)Math.Ceiling(gridCountRequest / (double)newColumnCount);
        

        if (currentGridRowCount == newRowCount
               && currentGridColumnCount == newColumnCount)
            return;

        ImageDisplayGrid.RowDefinitions = new RowDefinitionCollection(
                                                GenerateGridRow(newRowCount));
        ImageDisplayGrid.ColumnDefinitions = new ColumnDefinitionCollection(
                                                GenerateGridColumn(newColumnCount));
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