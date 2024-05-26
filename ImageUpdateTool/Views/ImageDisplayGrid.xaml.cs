using ImageUpdateTool.ViewModels;
using ServiceProvider = ImageUpdateTool.Utils.Tools.ServiceProvider;

namespace ImageUpdateTool.Views;

public partial class ImageDisplayGrid : ContentView
{
    private const int IMAGE_AREA_WIDTH = 220;
    private const int IMAGE_AREA_HEIGHT = 180;

    private ImageDisplayViewModel _displayVM;

    public int ImageCount => _displayVM.ImageList.Count;

    public ImageDisplayGrid()
	{
		InitializeComponent();

        var vm = ServiceProvider.GetService<ImageDisplayViewModel>();
        _displayVM = vm;
        BindingContext = _displayVM;

        _displayVM.OnImageListChanged += () =>
        {
            Resize();
            Rearrange();
        };
    }

    private void ImageDisplayGrid_Loaded(object sender, EventArgs e)
    {

    }

    private void ImageDisplayGrid_SizeChanged(object sender, EventArgs e)
    {
        Resize();
        Rearrange();
    }

    private void Resize()
    {
        int currentGridColumnCount = DisplayGrid.ColumnDefinitions.Count;
        int currentGridRowCount = DisplayGrid.RowDefinitions.Count;
        double gridWidth = DisplayGrid.Width;
        double gridHeight = DisplayGrid.Height;
        int gridCountRequest = ImageCount;

        int newColumnCount = Math.Max(1, (int)Math.Floor(gridWidth / IMAGE_AREA_WIDTH));
        
        int newRowCount = Math.Max(1, Math.Max((int)Math.Floor(gridHeight / IMAGE_AREA_HEIGHT),
                            (int)Math.Ceiling(gridCountRequest / (double)newColumnCount)));

        _displayVM.CurrentRowCount = newRowCount;
        _displayVM.CurrentColumnCount = newColumnCount;

        if (currentGridRowCount == newRowCount
               && currentGridColumnCount == newColumnCount)
            return;

        DisplayGrid.RowDefinitions = new RowDefinitionCollection(
                                                GenerateGridRow(newRowCount));
        DisplayGrid.ColumnDefinitions = new ColumnDefinitionCollection(
                                                GenerateGridColumn(newColumnCount));
    }

    private void Rearrange()
    {
        // 移除Grid中的所有ImageArea
        for (int i = DisplayGrid.Children.Count - 1; i >= 0; i--)
        {
            if (DisplayGrid.Children[i] is ImageArea)
            {
                DisplayGrid.Children.RemoveAt(i);
            }
        }

        int index = 0;
        for (int i = 0; i < DisplayGrid.RowDefinitions.Count; i++)
        {
            for (int j = 0; j < DisplayGrid.ColumnDefinitions.Count; j++)
            {
                if (index >= ImageCount)
                    return;
                var content = _displayVM.ImageList[index];
                DisplayGrid.Add(content, j, i);
                index++;
            }
        }
    }

    private static RowDefinition[] GenerateGridRow(int count)
    {
        var row = new RowDefinition[count];
        for (int i = 0; i < count; i++)
        {
            row[i] = new RowDefinition(IMAGE_AREA_HEIGHT);
        }
        return row;
    }

    private static ColumnDefinition[] GenerateGridColumn(int count)
    {
        var column = new ColumnDefinition[count];
        for (int i = 0; i < count; i++)
        {
            column[i] = new ColumnDefinition();
        }
        return column;
    }
}