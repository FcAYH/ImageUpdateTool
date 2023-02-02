using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace ImageUpdateTool.Controls;

public class TreeViewNode : StackLayout
{
    private DataTemplate _expandButtonTemplate = null;

    private TreeViewNode _parentNode;

    private DateTime _expandButtonClickedTime;

    private readonly BoxView _spacerBoxView = new() { Color = Colors.Transparent };
    private readonly BoxView _emptyBox = new() { BackgroundColor = Colors.Blue, Opacity = .5 };

    private const int EXPAND_BUTTON_WIDTH = 32;
    private ContentView _expandButtonContent = new();

    private readonly Grid _mainGrid = new()
    {
        VerticalOptions = LayoutOptions.Start,
        HorizontalOptions = LayoutOptions.Fill,
        RowSpacing = 2
    };

    private readonly StackLayout _contentStackLayout = new() { Orientation = StackOrientation.Horizontal };

    private readonly ContentView _contentView = new() { HorizontalOptions = LayoutOptions.Fill };

    private readonly StackLayout _childrenStackLayout = new() { Orientation = StackOrientation.Vertical, Spacing = 0, IsVisible = false };

    private IList<TreeViewNode> _children = new ObservableCollection<TreeViewNode>();
    private readonly TapGestureRecognizer _tapGestureRecognizer = new();
    private readonly TapGestureRecognizer _expandButtonGestureRecognizer = new();
    private readonly TapGestureRecognizer _doubleClickGestureRecognizer = new();

    internal readonly BoxView selectionBoxView = new() { Color = Colors.Blue, Opacity = .5, IsVisible = false };

    private TreeView _parentTreeView => Parent?.Parent as TreeView;
    private double _indentWidth => _depth * _spacerWidth;
    private int _spacerWidth { get; } = 30;
    private int _depth => ParentNode?._depth + 1 ?? 0;

    private bool _showExpandButtonIfEmpty = false;
    private Color _selectedBackgroundColor = Colors.Blue;
    private double _selectedBackgroundOpacity = .3;

    public event EventHandler Expanded;

    /// <summary>
    /// Occurs when the user double clicks on the node
    /// </summary>
    public event EventHandler DoubleClicked;

    protected override void OnParentSet()
    {
        base.OnParentSet();
        Render();
    }

    public bool IsSelected
    {
        get => selectionBoxView.IsVisible;
        set => selectionBoxView.IsVisible = value;
    }
    public bool IsExpanded
    {
        get => _childrenStackLayout.IsVisible;
        set
        {
            _childrenStackLayout.IsVisible = value;

            Render();
            if (value)
            {
                Expanded?.Invoke(this, new EventArgs());
            }
        }
    }

    /// <summary>
    /// set to true to show the expand button in case we need to poulate the child nodes on demand
    /// </summary>
    public bool ShowExpandButtonIfEmpty
    {
        get { return _showExpandButtonIfEmpty; }
        set { _showExpandButtonIfEmpty = value; }
    }

    /// <summary>
    /// set BackgroundColor when node is tapped/selected
    /// </summary>
    public Color SelectedBackgroundColor
    {
        get { return _selectedBackgroundColor; }
        set { _selectedBackgroundColor = value; }
    }

    /// <summary>
    /// SelectedBackgroundOpacity when node is tapped/selected
    /// </summary>
    public Double SelectedBackgroundOpacity
    {
        get { return _selectedBackgroundOpacity; }
        set { _selectedBackgroundOpacity = value; }
    }

    /// <summary>
    /// customize expand icon based on isExpanded property and or data 
    /// </summary>
    public DataTemplate ExpandButtonTemplate
    {
        get { return _expandButtonTemplate; }
        set { _expandButtonTemplate = value; }
    }

    public View Content
    {
        get => _contentView.Content;
        set => _contentView.Content = value;
    }

    public IList<TreeViewNode> ChildrenList
    {
        get => _children;
        set
        {
            if (_children is INotifyCollectionChanged notifyCollectionChanged)
            {
                notifyCollectionChanged.CollectionChanged -= ItemsSource_CollectionChanged;
            }

            _children = value;

            if (_children is INotifyCollectionChanged notifyCollectionChanged2)
            {
                notifyCollectionChanged2.CollectionChanged += ItemsSource_CollectionChanged;
            }

            TreeView.RenderNodes(_children, _childrenStackLayout, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset), this);

            Render();
        }
    }

    /// <summary>
    /// TODO: Remove this. We should be able to get the ParentTreeViewNode by traversing up through the Visual Tree by 'Parent', but this not working for some reason.
    /// </summary>
    public TreeViewNode ParentNode
    {
        get => _parentNode;
        set
        {
            _parentNode = value;
            Render();
        }
    }

    /// <summary>
    /// Constructs a new TreeViewItem
    /// </summary>
    public TreeViewNode()
    {
        var itemsSource = (ObservableCollection<TreeViewNode>)_children;
        itemsSource.CollectionChanged += ItemsSource_CollectionChanged;

        _tapGestureRecognizer.Tapped += TapGestureRecognizer_Tapped;
        GestureRecognizers.Add(_tapGestureRecognizer);

        _mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        _mainGrid.Children.Add(selectionBoxView);

        _contentStackLayout.Children.Add(_spacerBoxView);
        _contentStackLayout.Children.Add(_expandButtonContent);
        _contentStackLayout.Children.Add(_contentView);

        SetExpandButtonContent(_expandButtonTemplate);

        _expandButtonGestureRecognizer.Tapped += ExpandButton_Tapped;
        _expandButtonContent.GestureRecognizers.Add(_expandButtonGestureRecognizer);

        _doubleClickGestureRecognizer.NumberOfTapsRequired = 2;
        _doubleClickGestureRecognizer.Tapped += DoubleClick;
        _contentView.GestureRecognizers.Add(_doubleClickGestureRecognizer);

        _mainGrid.SetRow((IView)_childrenStackLayout, 1);
        _mainGrid.SetColumn((IView)_childrenStackLayout, 0);

        _mainGrid.Children.Add(_contentStackLayout);
        _mainGrid.Children.Add(_childrenStackLayout);

        base.Children.Add(_mainGrid);

        HorizontalOptions = LayoutOptions.Fill;
        VerticalOptions = LayoutOptions.Start;

        Render();
    }

    private void DoubleClickGestureRecognizer_Tapped(object sender, EventArgs e)
    {
    }

    private void ChildSelected(TreeViewNode child)
    {
        //Um? How does this work? The method here is a private method so how are we calling it?
        ParentNode?.ChildSelected(child);
        _parentTreeView?.ChildSelected(child);
    }

    private void Render()
    {
        _spacerBoxView.WidthRequest = _indentWidth;
        SetExpandButtonContent(_expandButtonTemplate);
        
        if ((ChildrenList == null || ChildrenList.Count == 0) && !ShowExpandButtonIfEmpty)
        {
            return;
        }

        foreach (var item in ChildrenList)
        {
            item.Render();
        }
    }

    /// <summary>
    /// Use DataTemplae 
    /// </summary>
    private void SetExpandButtonContent(DataTemplate expandButtonTemplate)
    {
        if (expandButtonTemplate != null)
        {
            _expandButtonContent.Content = (View)expandButtonTemplate.CreateContent();
        }
        else
        {
            _expandButtonContent.Content = new ContentView { Content = _emptyBox };
        }
    }

    private void ExpandButton_Tapped(object sender, EventArgs e)
    {
        _expandButtonClickedTime = DateTime.Now;
        //var node = _parentNode;
        IsExpanded = !IsExpanded;
        //while (node != null)
        //{
        //    node.IsExpanded = false;
        //    node.IsExpanded = true;
        //    node = node.ParentNode;
        //}
    }

    private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
    {
        //TODO: Hack. We don't want the node to become selected when we are clicking on the expanded button
        if (DateTime.Now - _expandButtonClickedTime > new TimeSpan(0, 0, 0, 0, 50))
        {
            ChildSelected(this);
        }
    }

    private void DoubleClick(object sender, EventArgs e)
    {
        DoubleClicked?.Invoke(this, new EventArgs());
    }

    private void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        TreeView.RenderNodes(_children, _childrenStackLayout, e, this);
        Render();
    }
}
