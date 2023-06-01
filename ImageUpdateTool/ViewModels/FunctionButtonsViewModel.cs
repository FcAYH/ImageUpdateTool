using ImageUpdateTool.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ImageUpdateTool.ViewModels
{
    public class FunctionButtonsViewModel : INotifyPropertyChanged
    {
        private ImageRepositoryModel _model;

        #region Properties
        private bool _isSyncRemoteButtonEnabled;
        private bool _isSelectImageButtonEnabled;
        private bool _isCopyUrlButtonEnabled;
        private bool _isRetryButtonEnabled;
        private bool _isOpenLocalDirectoryButtonEnabled;
        private string _copyUrlButtonTooltip;

        private Command _syncRemoteCommand;
        private Command _selectImageCommand;
        private Command _copyUrlCommand;
        private Command _retryCommand;
        private Command _openLocalDirectoryCommand;
        #endregion

        #region Attributes
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
        
        public bool IsSelectImageButtonEnabled
        {
            get => _isSelectImageButtonEnabled;
            set
            {
                if (_isSelectImageButtonEnabled != value)
                {
                    _isSelectImageButtonEnabled = value;
                    OnPropertyChanged(nameof(IsSelectImageButtonEnabled));
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

        public Command SelectImageButtonCommand
        {
            get
            {
                _selectImageCommand ??= new Command(SelectImageButtonCommandExecute);
                return _selectImageCommand;
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

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public FunctionButtonsViewModel(ImageRepositoryModel model) 
        { 
            _model = model;
        }

        #region Command Methods
        private void SyncRemoteButtonCommandExecute()
        {
            throw new NotImplementedException();
        }

        private void SelectImageButtonCommandExecute()
        {
            throw new NotImplementedException();
        }

        private void CopyUrlButtonCommandExecute()
        {
            throw new NotImplementedException();
        }
        
        private void RetryButtonCommandExecute()
        {
            throw new NotImplementedException();
        }
        
        private void OpenLocalDirectoryButtonCommandExecute()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
