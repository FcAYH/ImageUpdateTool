using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Item = ImageUpdateTool.Models.TreeView.Item;
using Group = ImageUpdateTool.Models.TreeView.Group;

namespace ImageUpdateTool.Pages;

public partial class MainPage : ContentPage
{
	private Models.ImageRepo _imageRepo;

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

    public MainPage()
	{
        InitializeComponent();
		InitializeGitStatus();
        _imageRepo = (Models.ImageRepo)BindingContext;

		InitializeFolderList();
    }

	protected override void OnAppearing()
	{
		base.OnAppearing();

        Window.Destroying += Window_Destroying;
    }

	private void Window_Destroying(object sender, EventArgs e)
	{
		SaveGitStatus();
	}

    private void InitializeFolderList()
    {
        DirectoryInfo repoDir = new DirectoryInfo(_imageRepo.LocalRepoPath);

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
			// Move photo to _localRepoPath
			string dateTime = DateTime.Now.ToString("yyyy/MM/dd");
			string folderPath = Path.Combine(_imageRepo.LocalRepoPath, dateTime);

			if (!Directory.Exists(folderPath))
			{
				Directory.CreateDirectory(folderPath);
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
			CopyURLButton.Text = $"Click to copy: {_imageRepo.LastestImageUrl}";
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
}