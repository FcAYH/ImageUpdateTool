using ImageUpdateTool.Models;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ImageUpdateTool.Views;

public partial class ImageArea : ContentView
{
    public event Action<string> OnImageAreaClicked;
    public event Action<string> OnImageDeleted;

    public string ImageSource
    {
        get => _imageSource;
        set
        {
            _imageSource = value.Replace('\\', '/');
            Image.Source = _imageSource;

            ToolTipProperties.SetText(Image, value.Split('\\', '/').Last());
        }
    }

    public double ImageSize
    {
        get => _imageSize;
        set
        {
            _imageSize = value;
            ImageSizeButton.Text = $"Size: {value}";
        }
    }

    public string ImageURL
    {
        get => _imageURL;
        set
        {
            _imageURL = value;
            ToolTipProperties.SetText(CopyURLButton, value);
        }
    }

    private string _imageSource;
    private double _imageSize;
    private string _imageURL;

    public ImageArea()
    {
        InitializeComponent();
    }

    private async void CopyURLButton_Clicked(object sender, EventArgs e)
    {
        await Clipboard.SetTextAsync(ImageURL);
    }

    private void Image_Clicked(object sender, EventArgs e)
    {
        OnImageAreaClicked?.Invoke(_imageSource);
    }

    private async void DeleteImageButton_Clicked(Object sender, EventArgs e)
    {
        // 删除图片操作不可逆，需要二次确认
        string msg = "Delete image CAN NOT resume, are you sure to delete it?";
        bool confirm = await Application.Current.MainPage.DisplayAlert("Alert!", msg, "是", "否");

        if (!confirm)
        {
            return;
        }

        OnImageDeleted?.Invoke(_imageSource);
    }
}