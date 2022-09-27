using System.Security.Cryptography;
using System.Text;

namespace ImageUpdateTool.Views;

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

		_imageRepo = (Models.ImageRepo)BindingContext;
	}

	private async void GitPullButton_Clicked(object sender, EventArgs e)
	{
		string error = _imageRepo.CallGitPull();
		if (!string.IsNullOrEmpty(error))
		{
			Status = GitStatus.FailToPull;
			await DisplayAlert("Error", $"��ȡ�ֿ�����ʧ�ܣ�������Ϣ:\n{error}\n�������Retry", "Yes");
		}
		else
		{
			Status = GitStatus.Success;
			await DisplayAlert("Success", "��ȡ���", "Yes");
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
                await DisplayAlert("Error", $"��������ʧ�ܣ�����ԭ��:\n{error}\n�������Retry", "Yes");
                return;
			}

			error = _imageRepo.CallGitCommit($"add new image {newFileName}");
            if (!string.IsNullOrEmpty(error))
            {
                Status = GitStatus.FailToCommit;
                await DisplayAlert("Error", $"�ύ����ʧ�ܣ�����ԭ��:\n{error}\n�������Retry", "Yes");
                return;
            }

            error = _imageRepo.CallGitPush();
            if (!string.IsNullOrEmpty(error))
            {
                Status = GitStatus.FailToPush;
                await DisplayAlert("Error", $"���͸���ʧ�ܣ�����ԭ��:\n{error}\n�������Retry", "Yes");
                return;
            }

			await DisplayAlert("Success", "�ϴ��ɹ�!", "Yes");
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
                await DisplayAlert("Error", "��ȡ�ֿ�����ʧ�ܣ������������Retry", "Yes");
            }
        }

		if (Status == GitStatus.FailToAdd)
		{
            string error = _imageRepo.CallGitAdd(_newFileLocalPath);
            if (!string.IsNullOrEmpty(error))
            {
                Status = GitStatus.FailToAdd;
                await DisplayAlert("Error", $"��������ʧ�ܣ�����ԭ��:\n{error}\n�������Retry", "Yes");
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
                await DisplayAlert("Error", $"�ύ����ʧ�ܣ�����ԭ��:\n{error}\n�������Retry", "Yes");
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
                await DisplayAlert("Error", $"�����޸�ʧ�ܣ�����ԭ��:\n{error}\n�������Retry", "Yes");
                return;
            }
        }
	}
}