using ImageUpdateTool.Models;
using ImageUpdateTool.Utils;
using ImageUpdateTool.Utils.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ErrorEventArgs = ImageUpdateTool.Utils.Events.ErrorEventArgs;

namespace ImageUpdateTool.ViewModels
{
    public class FunctionButtonsViewModel : INotifyPropertyChanged
    {
        private ImageRepositoryModel _model;

        #region Fields
        private bool _isSyncRemoteButtonEnabled = true;
        private bool _isUploadImageButtonEnabled = true;
        private bool _isCopyUrlButtonEnabled = false;
        private bool _isRetryButtonEnabled = false;
        private bool _isOpenLocalDirectoryButtonEnabled = true;
        private string _copyUrlButtonTooltip = string.Empty;

        private Command _syncRemoteCommand;
        private Command _uploadImageCommand;
        private Command _copyUrlCommand;
        private Command _retryCommand;
        private Command _openLocalDirectoryCommand;
        #endregion

        #region Properties
        public bool IsSyncRemoteButtonEnabled
        {
            get => _isSyncRemoteButtonEnabled;
            set
            {
                if (_isSyncRemoteButtonEnabled != value)
                {
                    _isSyncRemoteButtonEnabled = value;
                    OnPropertyChanged(nameof(IsSyncRemoteButtonEnabled));
                }
            }
        }

        public bool IsUploadImageButtonEnabled
        {
            get => _isUploadImageButtonEnabled;
            set
            {
                if (_isUploadImageButtonEnabled != value)
                {
                    _isUploadImageButtonEnabled = value;
                    OnPropertyChanged(nameof(IsUploadImageButtonEnabled));
                }
            }
        }

        public bool IsCopyUrlButtonEnabled
        {
            get => _isCopyUrlButtonEnabled;
            set
            {
                if (_isCopyUrlButtonEnabled != value)
                {
                    _isCopyUrlButtonEnabled = value;
                    OnPropertyChanged(nameof(IsCopyUrlButtonEnabled));
                }
            }
        }

        public bool IsRetryButtonEnabled
        {
            get => _isRetryButtonEnabled;
            set
            {
                if (_isRetryButtonEnabled != value)
                {
                    _isRetryButtonEnabled = value;
                    OnPropertyChanged(nameof(IsRetryButtonEnabled));
                }
            }
        }

        public bool IsOpenLocalDirectoryButtonEnabled
        {
            get => _isOpenLocalDirectoryButtonEnabled;
            set
            {
                if (_isOpenLocalDirectoryButtonEnabled != value)
                {
                    _isOpenLocalDirectoryButtonEnabled = value;
                    OnPropertyChanged(nameof(IsOpenLocalDirectoryButtonEnabled));
                }
            }
        }

        public string CopyUrlButtonTooltip
        {
            get => _copyUrlButtonTooltip;
            set
            {
                if (_copyUrlButtonTooltip != value)
                {
                    _copyUrlButtonTooltip = value;
                    OnPropertyChanged(nameof(CopyUrlButtonTooltip));
                }
            }
        }

        public Command SyncRemoteButtonCommand
        {
            get
            {
                _syncRemoteCommand ??= new Command(SyncRemoteButtonCommandExecute);
                return _syncRemoteCommand;
            }
        }

        public Command UploadImageButtonCommand
        {
            get
            {
                _uploadImageCommand ??= new Command(UploadImageButtonCommandExecute);
                return _uploadImageCommand;
            }
        }

        public Command CopyUrlButtonCommand
        {
            get
            {
                _copyUrlCommand ??= new Command(CopyUrlButtonCommandExecute);
                return _copyUrlCommand;
            }
        }

        public Command RetryButtonCommand
        {
            get
            {
                _retryCommand ??= new Command(RetryButtonCommandExecute);
                return _retryCommand;
            }
        }

        public Command OpenLocalDirectoryButtonCommand
        {
            get
            {
                _openLocalDirectoryCommand ??= new Command(OpenLocalDirectoryButtonCommandExecute);
                return _openLocalDirectoryCommand;
            }
        }
        #endregion

        public event Action<ErrorEventArgs> OnErrorOccurred;

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public FunctionButtonsViewModel() { }
        public FunctionButtonsViewModel(ImageRepositoryModel model)
        {
            _model = model;
            _model.OnModelStatusChanged += ImageRepositoryModel_OnModelStatusChanged;
            _model.OnImageUploaded += ImageRepositoryModel_OnImageUploaded;
        }

        #region Command Methods
        private async void SyncRemoteButtonCommandExecute()
        {
            var error = await _model.SyncWithRemoteAsync();
            if (!string.IsNullOrEmpty(error))
            {
                OnErrorOccurred?.Invoke(new ErrorEventArgs(error, "SyncRemote"));
            }
        }

        private async void UploadImageButtonCommandExecute()
        {
            var photo = await MediaPicker.PickPhotoAsync();
            if (photo == null) return;

            string dateTime = DateTime.Now.ToString("yyyy/MM/dd");
            string folderPath = Path.Combine(_model.LocalStorageLocation, _model.RepoName, dateTime);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // 使用md5作为文件名
            string extensionName = Path.GetExtension(photo.FileName);
            FileStream file = new(photo.FullPath, System.IO.FileMode.Open);
            MD5 md5 = MD5.Create();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();
            StringBuilder sb = new();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            sb.Append(extensionName);
            string newFileName = sb.ToString();
            string newFilePath = Path.Combine(folderPath, newFileName);
            File.Move(photo.FullPath, newFilePath);

            var relativePath = $"{dateTime}/{newFileName}";

            var error = await _model.UploadImageAsync(relativePath);
            if (!string.IsNullOrEmpty(error))
            {
                OnErrorOccurred?.Invoke(new ErrorEventArgs(error, "UploadImage"));
            }
        }

        private async void CopyUrlButtonCommandExecute()
        {
            await Clipboard.SetTextAsync(_model.LatestUploadedImageUrl);
        }

        private async void RetryButtonCommandExecute()
        {
            var error = await _model.RetryAsync();
            if (!string.IsNullOrEmpty(error))
            {
                OnErrorOccurred?.Invoke(new ErrorEventArgs(error, "Retry"));
            }
        }

        private void OpenLocalDirectoryButtonCommandExecute()
        {
            Process.Start("explorer.exe", _model.LocalStorageLocation);
        }

        #endregion

        private void ImageRepositoryModel_OnModelStatusChanged(ModelStatus status)
        {
            if (status == ModelStatus.Processing)
            {
                IsSyncRemoteButtonEnabled = false;
                IsUploadImageButtonEnabled = false;
                IsRetryButtonEnabled = false;
            }
            else if (status == ModelStatus.Normal)
            {
                IsSyncRemoteButtonEnabled = true;
                IsUploadImageButtonEnabled = true;
                IsRetryButtonEnabled = false;
            }
            else
            {
                IsSyncRemoteButtonEnabled = false;
                IsUploadImageButtonEnabled = false;
                IsRetryButtonEnabled = true;
            }

            if (!string.IsNullOrEmpty(_model.LatestUploadedImage))
            {
                IsCopyUrlButtonEnabled = true;
            }
        }

        private void ImageRepositoryModel_OnImageUploaded(string obj)
        {
            CopyUrlButtonTooltip = _model.LatestUploadedImageUrl;
        }
    }
}
