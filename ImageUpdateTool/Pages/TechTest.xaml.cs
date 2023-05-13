

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

    private async void Button_Clicked(object sender, EventArgs e)
    {
        // Disable the button while cloning
        cloneButton.IsEnabled = false;

        // Reset the progress bar
        progressBar.IsVisible = true;
        progressBar.Progress = 0;

        var progress = new Progress<double>(value =>
        {
            this.Dispatcher.Dispatch(() =>
            {
                progressBar.ProgressTo(value, 250, Easing.Linear);
            });
        });
        var gitRunner = new CommandRunner("git", @"C:\Users\F_CIL\Desktop\");
        var error = await gitRunner.RunAsync("clone --progress https://github.com/FcAYH/Images.git", progress);
        cloneButton.IsEnabled = true;
        progressBar.IsVisible = false;

        if (!string.IsNullOrEmpty(error))
        {
            await DisplayAlert("Error", $"¿ËÂ¡²Ö¿âÊ§°Ü£¬´íÎóÔ­Òò\n{error}\nÇëÐÞ¸´ºóÔÙ´Îµã»÷Apply", "OK");
            return;
        }
    }

    private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        // Do nothing with the standard output data
    }

    private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        // Parse the standard error data for progress information
        if (e.Data != null)
        {
            // Use a regular expression to match the progress percentage
            var regex = new Regex(@"(\d+)%");
            var match = regex.Match(e.Data);
            if (match.Success)
            {
                // Convert the percentage to a double value
                var percentage = double.Parse(match.Groups[1].Value) / 100;

                // Update the progress bar on the UI thread
                this.Dispatcher.Dispatch(() =>
                {
                    progressBar.ProgressTo(percentage, 250, Easing.Linear);
                });
            }
        }
        else
        {
            // The process has finished, enable the button again
            this.Dispatcher.Dispatch(() =>
            {
                cloneButton.IsEnabled = true;
            });
        }
    }
}