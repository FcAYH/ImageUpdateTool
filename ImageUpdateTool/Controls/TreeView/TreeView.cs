using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Item = ImageUpdateTool.Models.TreeView.Item;
using Group = ImageUpdateTool.Models.TreeView.Group;

namespace ImageUpdateTool.Controls;

public class TreeView : ScrollView
{
    private readonly StackLayout _stackLayout = new StackLayout { Orientation = StackOrientation.Vertical };

    //TODO: This initialises the list, but there is nothing listening to INotifyCollectionChanged so no nodes will get rendered
    private IList<TreeViewNode> _rootNodes = new ObservableCollection<TreeViewNode>();
    private TreeViewNode _selectedItem;

    /// <summary>
    /// The item that is selected in the tree
    /// TODO: Make this two way - and maybe eventually a bindable property
    /// </summary>
    public TreeViewNode SelectedItem
    {
        get => _selectedItem;

        set
        {
            if (_selectedItem == value)
            {
                return;
            }

            if (_selectedItem != null)
            {
                _selectedItem.IsSelected = false;
            }

            _selectedItem = value;

            SelectedItemChanged?.Invoke(this, new EventArgs());
        }
    }


    public IList<TreeViewNode> RootNodes
    {
        get => _rootNodes;
        set
        {
            _rootNodes = value;

            if (value is INotifyCollectionChanged notifyCollectionChanged)
            {
                notifyCollectionChanged.CollectionChanged += (s, e) =>
                {
                    RenderNodes(_rootNodes, _stackLayout, e, null);
                };
            }

            RenderNodes(_rootNodes, _stackLayout, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset), null);
        }
    }

    /// <summary>
    /// Occurs when the user selects a TreeViewItem
    /// </summary>
    public event EventHandler SelectedItemChanged;

    public TreeView()
    {
        Content = _stackLayout;
    }

    private void RemoveSelectionRecursive(IEnumerable<TreeViewNode> nodes)
    {
        foreach (var treeViewItem in nodes)
        {
            if (treeViewItem != SelectedItem)
            {
                treeViewItem.IsSelected = false;
            }

            RemoveSelectionRecursive(treeViewItem.ChildrenList);
        }
    }

    private static void AddItems(IEnumerable<TreeViewNode> childTreeViewItems, StackLayout parent, TreeViewNode parentTreeViewItem)
    {
        foreach (var childTreeNode in childTreeViewItems)
        {
            if (!parent.Children.Contains(childTreeNode))
            {
                parent.Children.Add(childTreeNode);
            }

            childTreeNode.ParentNode = parentTreeViewItem;
        }
    }

    /// <summary>
    /// TODO: A bit stinky but better than bubbling an event up...
    /// </summary>
    internal void ChildSelected(TreeViewNode child)
    {
        SelectedItem = child;
        child.IsSelected = true;
        child.selectionBoxView.Color = child.SelectedBackgroundColor;
        child.selectionBoxView.Opacity = child.SelectedBackgroundOpacity;
        RemoveSelectionRecursive(RootNodes);
    }

    internal static void RenderNodes(IEnumerable<TreeViewNode> childTreeViewItems, StackLayout parent, NotifyCollectionChangedEventArgs e, TreeViewNode parentTreeViewItem)
    {
        if (e.Action != NotifyCollectionChangedAction.Add)
        {
            //TODO: Reintate this...
            //parent.Children.Clear();
            AddItems(childTreeViewItems, parent, parentTreeViewItem);
        }
        else
        {
            AddItems(e.NewItems.Cast<TreeViewNode>(), parent, parentTreeViewItem);
        }
    }

    // Main code: 
    private TreeViewNode CreateTreeViewNode(object bindingContext, Label label, bool isItem)
    {
        var node = new TreeViewNode
        {
            BindingContext = bindingContext,
            Content = new StackLayout
            {
                Children =
                    {
                        new ResourceImage
                        {
                            Resource = isItem? "item.png" :"folderopen.png" ,
                            HeightRequest= 16,
                            WidthRequest = 16
                        },
                        label
                    },
                Orientation = StackOrientation.Horizontal
            }
        };

        //set DataTemplate for expand button content
        node.ExpandButtonTemplate = new DataTemplate(() => new ExpandButtonContent { BindingContext = node });

        return node;
    }

    private void CreateItem(IList<TreeViewNode> children, Item item)
    {
        var label = new Label
        {
            VerticalOptions = LayoutOptions.Center,
            TextColor = Colors.White
        };
        label.SetBinding(Label.TextProperty, "Key");

        var itemTreeViewNode = CreateTreeViewNode(item, label, true);
        children.Add(itemTreeViewNode);
    }

    //private static void ProcessItems(TreeViewNode node, Group Group)
    //{
    //    var children = new ObservableCollection<TreeViewNode>();
    //    foreach (var item in Group.ItemList.OrderBy(i => i.Key))
    //    {
    //        CreateItem(children, item);
    //    }
    //    node.ChildrenList = children;
    //}
    
    public ObservableCollection<TreeViewNode> ProcessGroups(Group groups)
    {
        var rootNodes = new ObservableCollection<TreeViewNode>();

        foreach (var group in groups.Children.OrderBy(g => g.Name))
        {
            var label = new Label
            {
                VerticalOptions = LayoutOptions.Center,
                TextColor = Colors.White
            };
            label.SetBinding(Label.TextProperty, "Name");

            var groupTreeViewNode = CreateTreeViewNode(group, label, false);

            rootNodes.Add(groupTreeViewNode);

            groupTreeViewNode.ChildrenList = ProcessGroups(group);

            foreach (var item in group.ItemList)
            {
                CreateItem(groupTreeViewNode.ChildrenList, item);
            }
        }

        return rootNodes;
    }
}
