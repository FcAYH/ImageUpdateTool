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
        // ���Git URL�ĺϷ���
        string gitURL = GitURLEntry.Text;
        if (!(gitURL.StartsWith("https") && gitURL.EndsWith(".git")))
        {
            await DisplayAlert("Error", "Invaild \"Git URL\"", "OK");
            return;
        }

        string localDirectory = LocalDiretoryPreviewEntry.Text;
        string oldLocalDirectory = AppShell.AppSettings.LocalStoragePath;
        DirectoryInfo[] dirs = new DirectoryInfo(oldLocalDirectory).GetDirectories();
        
        // �������˴洢·��
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
                        await DisplayAlert("Error", $"��ΪȨ�����⣬�޷�ɾ���ļ���{name}������ܻ�Ӱ������Ĳ���\n�������OK�󣬻��Զ�Ϊ�����ļ���Դ���������������н�{name}�ļ���ɾ��", "OK");
                        CommandRunner explorer = new("explorer");
                        explorer.Run(oldLocalDirectory);
                        await DisplayAlert("Confirm", $"����ɾ��{name}�ļ��к���\"OK\"", "OK");
                    }
                }
            }

            CloneProgressBar.IsVisible = true;
            var progress = new Progress<double>(value =>
            {
                Dispatcher.Dispatch(
                    () => CloneProgressBar.ProgressTo(value, 250, Easing.Linear)
                );
            });

            CommandRunner gitClone = new("git", localDirectory);
            var error = await gitClone.RunAsync($"clone {gitURL}");
            CloneProgressBar.IsVisible = false;

            if (!string.IsNullOrEmpty(error))
            {
                await DisplayAlert("Error", $"��¡�ֿ�ʧ�ܣ�����ԭ��\n{error}\n���޸����ٴε��Apply", "OK");
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