using ImageUpdateTool.Models;
using System.Diagnostics;

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

	private void DeleteImageButton_Clicked(Object sender, EventArgs e)
	{
		OnImageDeleted?.Invoke(_imageSource);
	}
}