using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System.ComponentModel;

namespace ImageUpdateTool.Controls;

internal class FolderButton : Label, IButton, IButtonController, INotifyPropertyChanged
{
    public static readonly BindableProperty BorderColorProperty = BorderElement.BorderColorProperty;

    public static readonly BindableProperty BorderWidthProperty = BindableProperty.Create(
                                    nameof(BorderWidth), typeof(double), typeof(FolderButton), -1d);

    public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(
                                    nameof(CornerRadius), typeof(int), typeof(FolderButton), 
                                    defaultValue: BorderElement.DefaultCornerRadius);

    public event EventHandler Clicked;
    public event EventHandler Pressed;
    public event EventHandler Released;

    public Color BorderColor
    {
        get { return (Color)GetValue(BorderElement.BorderColorProperty); }
        set { SetValue(BorderElement.BorderColorProperty, value); }
    }

    public double BorderWidth
    {
        get { return (double)GetValue(BorderWidthProperty); }
        set { SetValue(BorderWidthProperty, value); }
    }

    public int CornerRadius
    {
        get { return (int)GetValue(CornerRadiusProperty); }
        set { SetValue(CornerRadiusProperty, value); }
    }

    Color IButtonStroke.StrokeColor => (Color)GetValue(BorderColorProperty);

    double IButtonStroke.StrokeThickness => (double)GetValue(BorderWidthProperty);

    int IButtonStroke.CornerRadius => (int)GetValue(CornerRadiusProperty);

    void IButton.Clicked()
    {
        throw new NotImplementedException();
    }

    void IButton.Pressed()
    {
        throw new NotImplementedException();
    }

    void IButton.Released()
    {
        throw new NotImplementedException();
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SendClicked()
    { }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SendPressed()
    {
        throw new NotImplementedException();
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SendReleased()
    {
        throw new NotImplementedException();
    }
}
