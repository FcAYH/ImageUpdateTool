

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
        // ����һ����͸��������
        var overlay = new BoxView
        {
            Color = Color.FromRgba(0, 0, 0, 0.5),
        };

        grid.Add(overlay);
        grid.SetRow(overlay, 0);
        grid.SetColumn(overlay, 0);
        grid.SetRowSpan(overlay, 2);

        // ����һ�����ذ�ť
        var backButton = new Button
        {
            Text = "Back",
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        grid.Add(backButton);
        grid.SetRow(backButton, 0);
        grid.SetColumn(backButton, 0);

        // Ϊ���ذ�ť��ӵ���¼����������Ƴ������Ͱ�ť
        backButton.Clicked += (s, e) =>
        {
            grid.Remove(overlay);
            grid.Remove(backButton);
        };
    }
}