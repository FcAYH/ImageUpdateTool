using ImageUpdateTool.Models;
using Microsoft.UI.Xaml;
using System.Collections.ObjectModel;

namespace ImageUpdateTool.Pages;

public partial class TreeViweDisplay : ContentPage
{
	public TreeViweDisplay()
	{
		InitializeComponent();

		ResizeGrid();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        DisplayAlert("Appearing", "a", "ok");
        //Microsoft.UI.Xaml.Window.Current.SizeChanged += Page_SizeChanged;
    }

    private void Page_SizeChanged(object sender, WindowSizeChangedEventArgs args)
    {
        throw new NotImplementedException();
    }

    private void ResizeGrid()
	{
		Button button = new Button();
		button.Text = "Clicked";
		button.Clicked += GridButton_Clicked;
		button.BackgroundColor = Colors.Red;
		ImageGrid.Add(button, 0, 0);
	}

    private void GridButton_Clicked(object sender, EventArgs e)
    {
		var rowDefine = ImageGrid.RowDefinitions;
		rowDefine.Add(new RowDefinition());
		var colDefine = ImageGrid.ColumnDefinitions;
		colDefine.Add(new ColumnDefinition());
    }
}