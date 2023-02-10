

using System.Diagnostics;

namespace ImageUpdateTool.Pages;

public partial class TechTest : ContentPage
{
    public TechTest()
    {
        InitializeComponent();

        
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await DisplayAlert("e", "s", "ok");
        
        Utils.CommandRunner explorer = new("explorer");
        explorer.Run("C:\\Users\\F_CIL\\Desktop");

        Debug.WriteLine("Closed explorer");
    }

}