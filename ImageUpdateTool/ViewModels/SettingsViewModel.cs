using ImageUpdateTool.Models;
using ImageUpdateTool.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageUpdateTool.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly AppSettings _appSettings;

        // 在页面中修改属性时，并不会直接修改Model的数据，而是在
        // 点击Apply后才会一并修改。

        #region Fields
        private AppTheme _colorTheme;
        private Language _language;
        private string _gitUserName;
        private string _gitUserEmail;
        private string _imageRepositoryURL;
        private string _localStorageLocation;

        private bool _isApplyButtonEnabled = true;
        #endregion

        #region Properties
        public AppTheme ColorTheme
        {
            get => _colorTheme;
            set
            {
                if (_colorTheme != value)
                {
                    _colorTheme = value;
                    _appSettings.ColorTheme = value;
                    OnPropertyChanged(nameof(ColorTheme));
                }
            }
        }

        public Language Language
        {
            get => _language;
            set
            {
                if (_language != value)
                {
                    _language = value;
                    _appSettings.Language = value;
                    OnPropertyChanged(nameof(Language));
                }
            }
        }

        public string GitUserName
        {
            get => _gitUserName;
            set
            {
                if (_gitUserName != value)
                {
                    _gitUserName = value;
                    OnPropertyChanged(nameof(GitUserName));
                }
            }
        }

        public string GitUserEmail
        {
            get => _gitUserEmail;
            set
            {
                if (_gitUserEmail != value)
                {
                    _gitUserEmail = value;
                    OnPropertyChanged(nameof(GitUserEmail));
                }
            }
        }

        public string ImageRepositoryURL
        {
            get => _imageRepositoryURL;
            set
            {
                if (_imageRepositoryURL != value)
                {
                    _imageRepositoryURL = value;
                    OnPropertyChanged(nameof(ImageRepositoryURL));
                }
            }
        }

        public string LocalStorageLocation
        {
            get => _localStorageLocation;
            set
            {
                if (_localStorageLocation != value)
                {
                    _localStorageLocation = value;
                    OnPropertyChanged(nameof(LocalStorageLocation));
                }
            }
        }

        public bool IsApplyButtonEnabled
        {
            get => _isApplyButtonEnabled;
            set
            {
                if (_isApplyButtonEnabled != value)
                {
                    _isApplyButtonEnabled = value;
                    OnPropertyChanged(nameof(IsApplyButtonEnabled));
                }
            }
        }
        #endregion

        #region Constructors
        public SettingsViewModel() { }

        public SettingsViewModel(AppSettings appSettings)
        {
            _appSettings = appSettings;

            // 从AppSettings中读取数据
            _imageRepositoryURL = _appSettings.ImageRepositoryURL;
            _localStorageLocation = _appSettings.LocalStorageLocation;
            _gitUserName = _appSettings.GitUserName;
            _gitUserEmail = _appSettings.GitUserEmail;
            _colorTheme = _appSettings.ColorTheme;
            _language = _appSettings.Language;
        }
        #endregion

        // 实现INotifyPropertyChanged接口
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ApplyChanges()
        {
            _appSettings.GitUserName = GitUserName;
            _appSettings.GitUserEmail = GitUserEmail;
            _appSettings.ImageRepositoryURL = ImageRepositoryURL;
            _appSettings.LocalStorageLocation = LocalStorageLocation;
            _appSettings.Save();
        }

        public void SetLocalStorageLocationToDefault()
        {
            LocalStorageLocation = AppSettings.GenerateDefaultStorageLocation();
        }
    }
}
