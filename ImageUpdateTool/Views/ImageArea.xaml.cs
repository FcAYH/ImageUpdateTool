using ImageUpdateTool.Models;

namespace ImageUpdateTool.Views;

public partial class ImageArea : ContentView
{
	public string ImageSource
	{
		get => _imageSource;
		set
		{
			_imageSource = value;
			Image.Source = value;
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

	public string ImageURL { get; set; }

	private string _imageSource;
	private double _imageSize;

	public ImageArea()
	{
		InitializeComponent();
	}

    private async void CopyURLButton_Clicked(object sender, EventArgs e)
	{
        await Clipboard.SetTextAsync(ImageURL);
    }
}