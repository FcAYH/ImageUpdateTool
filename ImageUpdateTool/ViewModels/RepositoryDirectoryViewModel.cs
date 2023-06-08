using ImageUpdateTool.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ImageUpdateTool.ViewModels
{
    public class RepositoryDirectoryViewModel : INotifyPropertyChanged
    {
        private readonly ImageRepositoryModel _model;
        private TreeNode _rootNode;
        private bool isLabelVisible = false;

        public ObservableCollection<TreeNode> Nodes
        {
            get => _rootNode.Children as ObservableCollection<TreeNode>;
        }
        
        public bool IsLabelVisible
        {
            get => isLabelVisible;
            private set
            {
                if (isLabelVisible != value)
                {
                    isLabelVisible = value;
                    OnPropertyChanged(nameof(IsLabelVisible));
                }
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public RepositoryDirectoryViewModel(ImageRepositoryModel model)
        {
            _model = model;

            GenerateTreeNodes();
        }

        private void GenerateTreeNodes()
        {
            var dir = new DirectoryInfo(Path.Combine(_model.LocalStorageLocation, _model.RepoName));
            if (!dir.Exists)
            {
                return;
            }

            int layer = 0;
            _rootNode = new TreeNode("Root", dir.FullName, layer);

            var queue = new Queue<TreeNode>();
            queue.Enqueue(_rootNode);
            while (queue.Count > 0)
            {
                int size = queue.Count;
                layer++;
                for (int i = 0; i < size; i++)
                {
                    var current = queue.Dequeue();
                    var dirInfo = new DirectoryInfo(current.Path);
                    var subDirs = dirInfo.GetDirectories();
                    foreach (var subDir in subDirs)
                    {
                        if (subDir.Name.Contains(".git")) continue;
                        var node = new TreeNode(subDir.Name, subDir.FullName, layer);
                        current.Children.Add(node);
                        queue.Enqueue(node);
                    }

                    current.IsLeaf = current.Children.Count == 0;
                }
            }
        }
    }

    public class TreeNode : INotifyPropertyChanged
    {
        private string _folderIcon = "📁";
        private bool _isExpanded = false;
        private int _layer; // 用于控制缩进
        private Thickness _marginLeft;

        public string FolderIcon
        {
            get => _folderIcon;
            private set
            {
                if (_folderIcon != value)
                {
                    _folderIcon = value;
                    OnPropertyChanged(nameof(FolderIcon));
                }
            }
        }

        public virtual string Name { get; set; }
        public virtual IList<TreeNode> Children { get; set; } = new ObservableCollection<TreeNode>();

        public virtual bool IsExpanded
        {
            get => _isExpanded;
            private set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged(nameof(IsExpanded));
                }
            }
        }

        public virtual bool IsLeaf { get; set; } = true;

        public string Path { get; set; }

        public Thickness MarginLeft
        {
            get => _marginLeft; 
            set
            {
                if (_marginLeft != value)
                {
                    _marginLeft = value;
                    OnPropertyChanged(nameof(MarginLeft));
                }
            }
        }

        private ICommand _nodeClickedCommand;
        public ICommand NodeClickedCommand
        {
            get
            {
                _nodeClickedCommand ??= new Command(OnNodeButton_Clicked);
                return _nodeClickedCommand;
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public TreeNode() { }

        public TreeNode(string name)
        {
            Name = name;
        }

        public TreeNode(string name, string path)
        {
            Name = name;
            Path = path;
        }

        public TreeNode(string name, string path, int layer)
        {
            Name = name;
            Path = path;
            _layer = layer;

            MarginLeft = new Thickness(-10 * _layer, 0, 0, 0);
        }

        private void OnNodeButton_Clicked()
        {
            if (IsLeaf)
            {
                Debug.WriteLine("Leaf node clicked");
                // TODO
            }
            else
            {
                IsExpanded = !IsExpanded;
                FolderIcon = IsExpanded ? "📂" : "📁";
            }
        }
    }
}
