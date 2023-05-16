

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
       
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        Debug.WriteLine(AppShell.AppSettings.LocalStoragePath);
        //Debug.WriteLine(FileSystem.Current.AppDataDirectory);

        //var localState = FileSystem.Current.AppDataDirectory;

        //var imgFile = Path.Combine(localState, "htt.png");

        //try
        //{
        //    File.Delete(imgFile);
        //}
        //catch (Exception exp)
        //{
        //    Debug.WriteLine(exp);
        //}
    }
}