

using ImageUpdateTool.Models;
using ImageUpdateTool.Utils;
using System.Diagnostics;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ImageUpdateTool.Pages;

public partial class TechTest : ContentPage
{
    public TechTest()
    {
        InitializeComponent();
        var baseColor = this.BackgroundColor;
        Color gridColor = new Color(baseColor.Red, baseColor.Green, baseColor.Blue, 0.8f);
        ImageGrid.BackgroundColor = gridColor;
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        ImageGrid.IsVisible = true;
    }

    private void CloseButton_Clicked(object sender, EventArgs e)
    {
        ImageGrid.IsVisible = false;
    }
}