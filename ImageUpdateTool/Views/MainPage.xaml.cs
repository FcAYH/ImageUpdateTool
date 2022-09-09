using System.Security.Cryptography;
using System.Text;

namespace ImageUpdateTool.Views;

public partial class MainPage : ContentPage
{
	private Models.ImageRepo _imageRepo;

    public MainPage()
	{
		InitializeComponent();

		_imageRepo = (Models.ImageRepo)BindingContext;
	}

	private async void GitPullButton_Clicked(object sender, EventArgs e)
	{
		string result = _imageRepo.CallGitPull();
		await DisplayAlert("Info", result, "OK");
	}

	private async void SelectImageButton_Clicked(object sender, EventArgs e)
	{
        var photo = await MediaPicker.PickPhotoAsync();

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
        MD5 md5 = new MD5CryptoServiceProvider();
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
		string newFileLocalPath = dateTime + "/" + newFileName;
		_imageRepo.CallGitAdd(newFileLocalPath);
		string result = _imageRepo.CallGitCommit($"add new image {newFileName}");
		await DisplayAlert("Info", result, "OK");
		result = _imageRepo.CallGitPush();
		await DisplayAlert("Info", result, "OK");
    }

	private async void CopyUrlButton_Clicked(object sender, EventArgs e)
	{
		await Clipboard.SetTextAsync(_imageRepo.LastestImageUrl);
	}
}