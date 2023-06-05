using ImageUpdateTool.ViewModels;
using ImageUpdateTool.Utils;
using System.ComponentModel.DataAnnotations;

namespace ImageUpdateTool.Pages;

public partial class SettingsPage : ContentPage
{
    private SettingsViewModel _settingsVM;
    private readonly EmailAddressAttribute _emailValidator = new();
    public SettingsPage(SettingsViewModel settingsVM)
	{
        _settingsVM = settingsVM;
        BindingContext = _settingsVM;

		InitializeComponent();
        InitializeContent();
	}

    private void InitializeContent()
    {
        ColorThemeButtonGroup.SelectedIndex = (int)_settingsVM.ColorTheme;
        LanguageButtonGroup.SelectedIndex = (int)_settingsVM.Language;
    }

    private async void SelectButton_Clicked(object sender, EventArgs e)
    {
        string path = await FolderPicker.PickSingleFolderAsync();
        _settingsVM.LocalStorageLocation = path;
    }

    private async void ApplyButton_Clicked(object sender, EventArgs e)
    {
        _settingsVM.IsApplyButtonEnabled = false;
        // ���ApplyButton���ж����������Ƿ�Ϸ���
        // �����Ϸ����򵯳�����򣬲����κ��޸ģ�
        // ���Ϸ��������޸ġ�

        // ������û����������գ�������д�Ļ�������Ҫ������ȷ�ĸ�ʽ
        string userEmail = _settingsVM.GitUserEmail;
        if (!string.IsNullOrEmpty(userEmail) && !_emailValidator.IsValid(userEmail))
        {
            await DisplayAlert("Error", "Invaild Email Address!", "OK");
            _settingsVM.IsApplyButtonEnabled = true;
            return;
        }

        string url = _settingsVM.ImageRepositoryURL;
        if (!(url.StartsWith("https") && url.EndsWith(".git")))
        {
            await DisplayAlert("Error", "Invaild \"Image Repository URL\"!", "OK");
            _settingsVM.IsApplyButtonEnabled = true;
            return;
        }

        string path = _settingsVM.LocalStorageLocation;
        if (!Directory.Exists(path))
        {
            await DisplayAlert("Error", "Invaild \"Local Storage Location\"!\nThis Directory is not exist!", "OK");
            _settingsVM.IsApplyButtonEnabled = true;
            return;
        }

        // �����޸�
        _settingsVM.ApplyChanges();

        _settingsVM.IsApplyButtonEnabled = true;

        // �л���MainPage
        await Shell.Current.GoToAsync("//TechTestPage");
    }

    private void ColorThemeButtonGroup_SelectedItemChanged(object sender, EventArgs e)
    {
        _settingsVM.ColorTheme = (AppTheme)(ColorThemeButtonGroup.SelectedIndex);
    }

    private void LanguageButtonGroup_SelectedItemChanged(object sender, EventArgs e)
    {
        _settingsVM.Language = (Language)(LanguageButtonGroup.SelectedIndex);
    }

    private void RestoreTheDefaultButton_Clicked(object sender, EventArgs e)
    {
        _settingsVM.SetLocalStorageLocationToDefault();
    }
}