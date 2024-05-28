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
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ImageRepositoryModel _model;
        private double _progress;
        private bool _isProgressBarVisible;
        private bool _errorOccurred;
        private Progress<double> _progressValue;

        public event Action OnErrorOccurred;

        public double Progress
        {
            get => _progress;
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    OnPropertyChanged(nameof(Progress));
                }
            }
        }

        public bool IsProgressBarVisible
        {
            get => _isProgressBarVisible;
            set
            {
                if (_isProgressBarVisible != value)
                {
                    _isProgressBarVisible = value;
                    OnPropertyChanged(nameof(IsProgressBarVisible));
                }
            }
        }

        public bool ErrorOccurred
        {
            get => _errorOccurred;
            set
            {
                if (_errorOccurred != value)
                {
                    _errorOccurred = value;
                    OnPropertyChanged(nameof(ErrorOccurred));
                }
            }
        }

        // 实现INotifyPropertyChanged接口
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public MainViewModel(ImageRepositoryModel model)
        {
            _model = model;
            _progressValue = new Progress<double>(value =>
            {
                Progress = value;
                Debug.WriteLine("Progress Value: " + value);
            });

            _model.Progress = _progressValue;
            _model.OnModelStatusChanged += ImageRepositoryModel_OnModelStatusChanged;
        }

        private void ImageRepositoryModel_OnModelStatusChanged(ModelStatus status)
        {
            if (status == ModelStatus.Normal)
            {
                // Model 运行正常，隐藏进度条
                IsProgressBarVisible = false;
            }
            else if (status == ModelStatus.Processing)
            {
                // Model 正在运行，显示进度条
                IsProgressBarVisible = true;
            }
            else
            {
                // Model 运行出错，此时会让进度条变红，随后再隐藏
                ErrorOccurred = true;

                // 延迟 2.5s 后隐藏进度条
                Task.Delay(2500).ContinueWith(t =>
                {
                    IsProgressBarVisible = false;
                });
            }
        }

        public async void UpdateSettings()
        {
            string error = await _model.Initialize();
            if (!string.IsNullOrEmpty(error))
            {
                // TODO: 说明初始化失败，此时应该显示错误信息

            }
        }
    }
}
