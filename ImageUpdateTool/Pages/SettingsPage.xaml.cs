using ImageUpdateTool.Utils;

namespace ImageUpdateTool.Pages;

public partial class SettingsPage : ContentPage
{
	public SettingsPage()
	{
		InitializeComponent();
        InitializeContent();
	}

    private void InitializeContent()
    {
        AppThemeButtonGroup.SelectedIndex = ((int)AppShell.AppSettings.CurrentTheme);
        GitURLEntry.Text = AppShell.AppSettings.GitURL;
        LocalDiretoryPreviewEntry.Text = AppShell.AppSettings.LocalStoragePath;
    }

    private async void SelectButton_Clicked(object sender, EventArgs e)
    {
        string path = await FolderPicker.PickSingleFolderAsync();
        LocalDiretoryPreviewEntry.Text = path;
    }

    private async void ApplyButton_Clicked(object sender, EventArgs e)
    {
        // 检测Git URL的合法性
        string gitURL = GitURLEntry.Text;
        if (!(gitURL.StartsWith("https") && gitURL.EndsWith(".git")))
        {
            await DisplayAlert("Error", "Invaild \"Git URL\"", "OK");
            return;
        }

        string localDirectory = LocalDiretoryPreviewEntry.Text;
        string oldLocalDirectory = AppShell.AppSettings.LocalStoragePath;
        DirectoryInfo[] dirs = new DirectoryInfo(oldLocalDirectory).GetDirectories();
        
        if (localDirectory != oldLocalDirectory
                && gitURL == AppShell.AppSettings.GitURL)
        {
            string[] dirName = dirs.Select(d => d.Name).ToArray();
            if (dirName.Length > 0)
            {
                foreach (string name in dirName)
                {
                    Directory.Move(Path.Combine(oldLocalDirectory, name),
                                    Path.Combine(localDirectory, name));
                }
            }
        }

        if (gitURL != AppShell.AppSettings.GitURL)
        {
            string[] dirName = dirs.Select(d => d.FullName).ToArray();
            if (dirName.Length > 0)
            {
                foreach (string name in dirName)
                {
                    try
                    {
                        Directory.Delete(name, true);
                    }
                    catch 
                    {
                        await DisplayAlert("Error", $"因为权限问题，无法删除文件夹{name}，这可能会影响后续的操作\n在您点击OK后，会自动为您打开文件资源管理器，请在其中将{name}文件夹删除", "OK");
                        CommandRunner explorer = new("explorer");
                        explorer.Run(oldLocalDirectory);
                        await DisplayAlert("Confirm", $"请在删除{name}文件夹后点击\"OK\"", "OK");
                    }
                }
            }

            CommandRunner gitClone = new("git", localDirectory);
            var error = gitClone.Run($"clone {gitURL}");
            
            if (!string.IsNullOrEmpty(error))
            {
                await DisplayAlert("Error", $"克隆仓库失败，错误原因\n{error}\n请修复后再次点击Apply", "OK");
                return;
            }
        }

        AppShell.AppSettings.GitURL = gitURL;
        AppShell.AppSettings.LocalStoragePath = localDirectory;
        
        (Shell.Current as AppShell).UnlockMainPage();
        await Shell.Current.GoToAsync("//MainPage?ImageRepoChanged=true");
    }

    private void AppThemeButtonGroup_SelectedItemChanged(object sender, EventArgs e)
    {
        AppShell.AppSettings.CurrentTheme = (AppTheme)(AppThemeButtonGroup.SelectedIndex);
    }

    private void RestoreTheDefaultButton_Clicked(object sender, EventArgs e)
    {
        AppShell.AppSettings.SetLocalStoragePathToDefault();
        InitializeContent();
    }
}