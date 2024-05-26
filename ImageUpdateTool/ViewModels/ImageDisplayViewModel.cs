using ImageUpdateTool.Models;
using ImageUpdateTool.Utils;
using ImageUpdateTool.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ImageUpdateTool.ViewModels
{
    public class ImageDisplayViewModel : INotifyPropertyChanged
    {
        private readonly ImageRepositoryModel _model;

        #region Fields
        private bool _isLabelVisible = true;
        private bool _isDisplayGridVisible = false;
        private bool _isPreviewing = false;
        private int _currentRowCount = 1;
        private int _currentColumnCount = 1;
        private string _previewImagePath = string.Empty;
        private Command _exitPreviewCommand;

        private string _currentSelectedDirectory = string.Empty;
        private List<ImageArea> _imageList;

        #endregion

        #region Properties
        public List<ImageArea> ImageList => _imageList;

        public bool IsLabelVisible
        {
            get => _isLabelVisible;
            set
            {
                if (_isLabelVisible != value)
                {
                    _isLabelVisible = value;
                    OnPropertyChanged(nameof(IsLabelVisible));
                }
            }
        }
        public bool IsDisplayGridVisible
        {
            get => _isDisplayGridVisible;
            set
            {
                if (_isDisplayGridVisible != value)
                {
                    _isDisplayGridVisible = value;
                    OnPropertyChanged(nameof(IsDisplayGridVisible));
                }
            }
        }
        public bool IsPreviewing
        {
            get => _isPreviewing;
            set
            {
                if (_isPreviewing != value)
                {
                    _isPreviewing = value;
                    OnPropertyChanged(nameof(IsPreviewing));
                }
            }
        }
        public int CurrentRowCount
        {
            get => _currentRowCount;
            set
            {
                if (_currentRowCount != value)
                {
                    _currentRowCount = value;
                    OnPropertyChanged(nameof(CurrentRowCount));
                }
            }
        }
        public int CurrentColumnCount
        {
            get => _currentColumnCount;
            set
            {
                if (_currentColumnCount != value)
                {
                    _currentColumnCount = value;
                    OnPropertyChanged(nameof(CurrentColumnCount));
                }
            }
        }
        public string PreviewImagePath
        {
            get => _previewImagePath;
            set
            {
                if (_previewImagePath != value)
                {
                    _previewImagePath = value;
                    OnPropertyChanged(nameof(PreviewImagePath));
                }
            }
        }
        public Command ExitPreviewCommand
        {
            get
            {
                _exitPreviewCommand ??= new Command((obj) =>
                {
                    IsPreviewing = false;
                });

                return _exitPreviewCommand;
            }
        }

        #endregion

        public event Action OnImageListChanged;

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public ImageDisplayViewModel(ImageRepositoryModel model)
        {
            _model = model;
            _model.OnSelectionChanged += ImageRepositoryModel_OnSelectionChanged;
            _imageList = new List<ImageArea>(8);
        }

        private void ImageRepositoryModel_OnSelectionChanged(string path)
        {
            IsDisplayGridVisible = true;
            IsLabelVisible = false;

            if (path != _currentSelectedDirectory)
            {
                _currentSelectedDirectory = path;
                GenerateImageAreas();
                OnImageListChanged.Invoke();
            }
        }

        private void GenerateImageAreas()
        {
            if (!Directory.Exists(_currentSelectedDirectory))
                return;

            _imageList.Clear();
            var dir = new DirectoryInfo(_currentSelectedDirectory);
            foreach (var img in dir.GetFiles())
            {
                if (img.Extension == ".db")
                    continue;

                ImageArea area = new()
                {
                    ImageSize = img.Length,
                    ImageURL = _model.LocalPathToUrl(img.FullName),
                    ImageSource = Enum.IsDefined(typeof(ImageExtension), img.Extension.ToLower().Trim('.'))
                                    ? img.FullName
                                    : "unable_to_preview.png"
                };
                area.OnImageAreaClicked += ImageArea_OnImageClicked;
                area.OnImageDeleted += ImageArea_OnImageDeleted;
                _imageList.Add(area);
            }
        }

        private void ImageArea_OnImageDeleted(string relativePath)
        {
            _ = _model.RemoveImageAsync(relativePath);
        }

        private void ImageArea_OnImageClicked(string path)
        {
            PreviewImagePath = path;
            IsPreviewing = true;
        }
    }
}
