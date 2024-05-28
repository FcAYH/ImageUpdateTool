using System.Diagnostics;
using ANi = Microsoft.Maui.Controls.AnimationExtensions;

namespace ImageUpdateTool.Views;

public partial class ProgressBar : ContentView
{
    public static BindableProperty ProgressProperty = BindableProperty.Create(
                nameof(Progress), typeof(double), typeof(ProgressBar), 0.0,
                propertyChanged: (bindable, oldValue, newValue) =>
                {
                    var progressBar = (ProgressBar)bindable;

                    // 根据BackgroundLabel的长度，和Progress的值，改变ProgressLabel的长度
                    progressBar.ProgressLabel.WidthRequest = (double)newValue * progressBar.BackgroundLabel.Width;
                }
            );

    public static BindableProperty ErrorTriggerProperty = BindableProperty.Create(
                nameof(ErrorTrigger), typeof(bool), typeof(ProgressBar), false,
                propertyChanged: (bindable, oldValue, newValue) =>
                {
                    var progressBar = (ProgressBar)bindable;
                    if ((bool)newValue == true)
                    {
                        progressBar.OnErrorOccurred();
                    }
                }
            );

    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public bool ErrorTrigger
    {
        get => (bool)GetValue(ErrorTriggerProperty);
        set => SetValue(ErrorTriggerProperty, value);
    }

    public ProgressBar()
    {
        InitializeComponent();
    }

    private void BackgroundLabel_SizeChanged(object sender, EventArgs e)
    {
        ProgressLabel.WidthRequest = Progress * BackgroundLabel.Width;
    }

    private async void OnErrorOccurred()
    {
        // Save the original background color of the label
        var originalColor = ProgressBorder.BackgroundColor;
        // Create an animation that changes the background color of the label to red
        var animation = new Animation(v => ProgressBorder.BackgroundColor = Color.FromRgb(v, 0, 0), 0, 1);
        // Run the animation for 500 milliseconds
        animation.Commit(this, "ColorAnimation", 16, 500);
        // Wait for 2 seconds
        await Task.Delay(2000);
        // Create an animation that changes the background color of the label back to original color
        var reverseAnimation = new Animation(v => ProgressBorder.BackgroundColor = Blend(originalColor, Colors.Red, v), 1, 0);
        reverseAnimation.Commit(this, "ColorAnimation", 16, 500);

        ErrorTrigger = false;
    }

    private static Color Blend(Color color1, Color color2, double ratio)
    {
        return Color.FromRgb(
            (float)(color1.Red + (color2.Red - color1.Red) * ratio),
            (float)(color1.Green + (color2.Green - color1.Green) * ratio),
            (float)(color1.Blue + (color2.Blue - color1.Blue) * ratio));
    }
}