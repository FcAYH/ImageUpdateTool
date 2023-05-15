

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
        // 创建一个半透明的容器
        var overlay = new BoxView
        {
            Color = Color.FromRgba(0, 0, 0, 0.5),
        };

        grid.Add(overlay);
        grid.SetRow(overlay, 0);
        grid.SetColumn(overlay, 0);
        grid.SetRowSpan(overlay, 2);

        // 创建一个返回按钮
        var backButton = new Button
        {
            Text = "Back",
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        grid.Add(backButton);
        grid.SetRow(backButton, 0);
        grid.SetColumn(backButton, 0);

        // 为返回按钮添加点击事件处理器，移除容器和按钮
        backButton.Clicked += (s, e) =>
        {
            grid.Remove(overlay);
            grid.Remove(backButton);
        };
    }
}