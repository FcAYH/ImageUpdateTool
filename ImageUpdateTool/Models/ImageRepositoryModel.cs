using ImageUpdateTool.Utils;
using ImageUpdateTool.Utils.Exceptions;
using ImageUpdateTool.Utils.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ImageUpdateTool.Models
{
    internal partial class ImageRepositoryModel
    {
        #region Properties
        private string _repoGitURL;
        private string _localStorageLocation;
        private string _repoName;
        private string _userName;
        private int _repoSize;
        private string _latestUploadedImage; // TODO: 类型待定
        private string _latestRemovedImage; // TODO: 类型待定
        private ModelStatus _modelStatus;
        private string _currentSelectedDirectory; // TODO: 类型待定

        private GitStatus _gitStatus;
        private GitRunner _git;
        private string _gitUserName;
        private string _gitUserEmail;
        
        #endregion

        #region Attributes
        /// <summary>
        /// 图床仓库的git URL链接，格式：https://github.com/[UserName]/[RepoName].git
        /// </summary>
        public string RepoGitURL
        {
            get => _repoGitURL;
            set
            {
                if (_repoGitURL != value)
                {
                    _repoGitURL = value;

                    ExtractUserNameAndRepoName(value, out string userName, out string repoName);
                    _userName = userName;
                    RepoName = repoName;
                    OnRepoGitURLChanged?.Invoke(_repoGitURL);
                }
            }
        }

        /// <summary>
        /// 本地存储图床的位置，默认存储在LocalState中
        /// </summary>
        public string LocalStorageLocation
        {
            get => _localStorageLocation;
            set
            {
                if (_localStorageLocation != value)
                {
                    _localStorageLocation = value;
                    OnLocalStorageLocationChanged?.Invoke(_localStorageLocation);
                }
            }
        }

        public string RepoName
        {
            get => _repoName;
            private set
            {
                if (_repoName != value)
                {
                    _repoName = value;
                }
            }
        }

        public string UserName
        {
            get => _userName;
            private set
            {
                if (_userName != value)
                {
                    _userName = value;
                }
            }
        }

        /// <summary>
        /// 图床仓库中文件的数量
        /// </summary>
        public int RepoSize
        {
            get => _repoSize;
            private set
            {
                if (_repoSize != value)
                {
                    _repoSize = value;
                }
            }
        }

        /// <summary>
        /// 最近上传的图片的相对路径，例如：2023/5/21/[ImageName].png
        /// </summary>
        public string LatestUploadedImage
        {
            get => _latestUploadedImage;
            private set
            {
                if (_latestUploadedImage != value)
                {
                    _latestUploadedImage = value;
                    OnImageUploaded?.Invoke(_latestUploadedImage);
                }
            }
        }

        /// <summary>
        /// 最近删除的图片的相对路径，例如: 2023/5/21/[ImageName].png
        /// </summary>
        public string LatestRemovedImage
        {
            get => _latestRemovedImage;
            private set
            {
                if (_latestRemovedImage != value)
                {
                    _latestRemovedImage = value;
                    OnImageRemoved?.Invoke(_latestRemovedImage);
                }
            }
        }

        /// <summary>
        /// 记录Model的状态，<see cref="ImageUpdateTool.Utils.ModelStatus"/>
        /// </summary>
        public ModelStatus ModelStatus
        {
            get => _modelStatus;
            private set
            {
                if (_modelStatus != value)
                {
                    _modelStatus = value;
                    OnModelStatusChanged?.Invoke(_modelStatus);
                }
            }
        }

        /// <summary>
        /// 当前被选中，用于在<see>TODO</see>中展示的文件夹相对路径
        /// </summary>
        public string CurrentSelectedDirectory
        {
            get => _currentSelectedDirectory;
            set
            {
                if (_currentSelectedDirectory != value)
                {
                    _currentSelectedDirectory = value;
                    OnSelectionChanged?.Invoke(_currentSelectedDirectory);
                }
            }
        }
        #endregion

        #region Events
        public event Action<string> OnRepoGitURLChanged;
        public event Action<string> OnLocalStorageLocationChanged;
        /// <summary>
        /// 当某个图片被成功上传时，会触发此事件
        /// </summary>
        public event Action<string> OnImageUploaded;
        /// <summary>
        /// 当某个图片被成功删除时，会触发此事件
        /// </summary>
        public event Action<string> OnImageRemoved;
        public event Action<ModelStatus> OnModelStatusChanged;
        /// <summary>
        /// 当被选中用于在<see cref=""/>展示的文件夹(<see cref="CurrentSelectedDirectory"/>)更改时，会触发此事件
        /// </summary>
        public event Action<string> OnSelectionChanged;
        #endregion

        #region Constructors
        protected ImageRepositoryModel() { }

        public ImageRepositoryModel(AppSettings settings)
        {
            _repoGitURL = settings.ImageRepositoryURL;
            _localStorageLocation = settings.LocalStorageLocation;
            _gitUserName = settings.GitUserName;
            _gitUserEmail = settings.GitUserEmail;

            try
            {
                _gitStatus = Enum.Parse<GitStatus>(Preferences.Get("GitStatus", "Success"));
            }
            catch
            {
                _gitStatus = GitStatus.Success;
            }

            try
            {
                _modelStatus = Enum.Parse<ModelStatus>(Preferences.Get("ModelStatus", "Normal"));
            }
            catch
            {
                _modelStatus = ModelStatus.Normal;
            }

            _git = new GitRunner(_localStorageLocation);

            settings.OnLocalStorageLocationChanged += OnAppSettings_LocalStorageLocationChanged;
            settings.OnImageRepositoryURLChanged += OnAppSettings_OnImageRepositoryURLChanged;
            settings.OnUrlAndLocationChanged += OnAppSettings_OnUrlAndLocationChanged;
        }

        #endregion

        #region Event Handlers
        // 仅存储位置变化，仅移动文件
        // 仅URL变化，重新clone
        // 两者都变化，直接在新的地方clone，并删除原文件

        private void OnAppSettings_LocalStorageLocationChanged(string location)
        {
            // 当本地存储位置发生变化时，需要做三件事情
            // 1. 将原位置的数据转移到新位置
            // 2. 更新_git的工作目录（应该要重新设置userName和email）
            // 3. 将事件传出去，通知ViewModels更新

            var currentPath = Path.Combine(_localStorageLocation, _repoName);
            var newPath = Path.Combine(location, _repoName);

            // 1. 将原位置的数据转移到新位置
            DirectoryTools.MoveDirectory(currentPath, newPath);
            _localStorageLocation = location;
            _git.WorkingDirectory = newPath;

            // TODO：应该要重新设置userName和email
            OnLocalStorageLocationChanged?.Invoke(_localStorageLocation);
        }

        private async void OnAppSettings_OnImageRepositoryURLChanged(string url)
        {
            // 当图床仓库的URL发生变化时，需要做两件事情
            // 1. 重新clone仓库
            // 2. 重新生成_userName和_repoName
            // 3. 将事件传出去，通知ViewModels更新
            // 4. 删除原仓库
            await CloneRepositoryAsync();
            _repoGitURL = url;
        }

        private void OnAppSettings_OnUrlAndLocationChanged(string arg1, string arg2)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Methods
        /// <summary>
        /// 异步方法，用于上传图片
        /// <para>当ModelStatus不为Normal时，无法调用此方法</para>
        /// </summary>
        /// <param name="path">待上传图片的相对路径</param>
        /// <param name="progress">用于报告进度</param>
        /// <returns><see cref="string"/>类型, 若上传成功则内容为空，否则内容为错误信息</returns>
        /// <exception cref="ModelProcessingUnderErrorStatus"/>
        public async Task<string> UploadImageAsync(string path, IProgress<double> progress = null)
        {
            if (ModelStatus != ModelStatus.Normal)
            {
                throw new ModelProcessingUnderErrorStatus("Image Repository has unresolved error that prevent this operation");
            }

            LatestUploadedImage = path;
            ModelStatus = ModelStatus.Processing;

            // 上传图片的步骤，Add -> Commit -> Push
            var error = await _git.AddAsync(path).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(error))
            {
                ModelStatus = ModelStatus.FailToUpload;
                _gitStatus = GitStatus.FailToAdd;
                return error;
            }

            error = await _git.CommitAsync($"add new image {path}").ConfigureAwait(false);
            if (!string.IsNullOrEmpty(error))
            {
                ModelStatus = ModelStatus.FailToUpload;
                _gitStatus = GitStatus.FailToCommit;
                return error;
            }

            error = await _git.PushAsync(progress).ConfigureAwait(false);
            if (!string.IsNullOrEmpty (error))
            {
                ModelStatus = ModelStatus.FailToUpload;
                _gitStatus = GitStatus.FailToPush;
                return error;
            }

            ModelStatus = ModelStatus.Normal;
            _gitStatus = GitStatus.Success;
            return string.Empty;
        }

        /// <summary>
        /// 异步方法，用于移除图片
        /// </summary>
        /// <param name="path">待移除图片的相对路径</param>
        /// <param name="progress">用于报告进度</param>
        /// <returns><see cref="string"/>类型，若移除成功则内容为空，否则内容为错误信息</returns>
        /// <exception cref="ModelProcessingUnderErrorStatus"/>
        public async Task<string> RemoveImageAsync(string path, IProgress<double> progress)
        {
            if (ModelStatus != ModelStatus.Normal)
            {
                throw new ModelProcessingUnderErrorStatus("Image Repository has unresolved error that prevent this operation");
            }

            LatestRemovedImage = path;
            ModelStatus = ModelStatus.Processing;

            // 移除图片的步骤，Remove -> Commit -> Push
            var error = await _git.RemoveAsync(path).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(error))
            {
                ModelStatus = ModelStatus.FailToRemove;
                _gitStatus = GitStatus.FailToRemove;
                return error;
            }

            error = await _git.CommitAsync($"remove image {path}").ConfigureAwait(false);
            if (!string.IsNullOrEmpty(error))
            {

                ModelStatus = ModelStatus.FailToRemove;
                _gitStatus = GitStatus.FailToCommit;
                return error;
            }

            error = await _git.PushAsync(progress).ConfigureAwait(false);
            if (string.IsNullOrEmpty(error))
            {
                ModelStatus = ModelStatus.FailToRemove;
                _gitStatus = GitStatus.FailToPush;
                return error;
            }

            ModelStatus = ModelStatus.Normal;
            _gitStatus = GitStatus.Success;
            return string.Empty;
        }

        /// <summary>
        /// 异步方法，用于同步本地仓库与远程仓库，及先拉取(pull)再推送(push)
        /// <para>注：这里可以缺省<paramref name="needPush"/>参数，取消推送过程，仅拉取，
        /// 因为在正常情况下，所有上传、移除操作都会及时推送，故而无需在此处推送。</para>
        /// </summary>
        /// <param name="progress">用与报告进度</param>
        /// <param name="needPush">默认为false，仅拉取，若传入值为true，则会在拉取后推送</param>
        /// <returns><see cref="string"/>类型，若同步成功则内容为空，否则内容为错误信息</returns>
        /// <exception cref="ModelProcessingUnderErrorStatus"/>
        public async Task<string> SyncWithRemoteAsync(IProgress<double> progress, bool needPush = false)
        {
            if (ModelStatus != ModelStatus.Normal)
            {
                throw new ModelProcessingUnderErrorStatus("Image Repository has unresolved error that prevent this operation");
            }

            ModelStatus = ModelStatus.Processing;

            // 同步的步骤，Pull -> Push(if needPush == true)
            var error = await _git.PullAsync(progress).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(error))
            {
                ModelStatus = ModelStatus.FailToSync;
                _gitStatus = GitStatus.FailToPull;
                return error;
            }

            if (needPush)
            {
                error = await _git.PushAsync(progress).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(error))
                {
                    ModelStatus = ModelStatus.FailToSync;
                    _gitStatus = GitStatus.FailToPush;
                    return error;
                }
            }

            ModelStatus = ModelStatus.Normal;
            _gitStatus= GitStatus.Success;
            return string.Empty;
        }

        /// <summary>
        /// 异步方法，用于重试之前失败的操作。
        /// </summary>
        /// <returns><see cref="string"/>类型，若重试成功则内容为空，否则内容为错误信息</returns>
        public async Task<string> RetryAsync(IProgress<double> progress)
        {
            ModelStatus = ModelStatus.Processing;

            // 这里只处理一下三类失败：
            // 1. 上传图片失败 => 可能是Add、Commit、Push失败，需要将失败流程以及其后续流程全部重试
            // 2. 移除图片失败 => 可能是Remove、Commit、Push失败，需要将失败流程以及其后续流程全部重试
            // 3. 同步失败 => 可能是Pull、Push失败，需要将失败流程以及其后续流程全部重试
            // clone操作失败在CloneRepoAsync方法中处理

            if (ModelStatus == ModelStatus.FailToUpload)
            {
                if (_gitStatus == GitStatus.FailToAdd)
                {
                    var error = await _git.AddAsync(_latestUploadedImage).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(error))
                    {
                        _gitStatus = GitStatus.FailToAdd;
                        return error;
                    }
                    _gitStatus = GitStatus.FailToCommit;
                }

                if (_gitStatus == GitStatus.FailToCommit)
                {
                    var error = await _git.CommitAsync($"add new image {_latestUploadedImage}").ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(error))
                    {
                        _gitStatus = GitStatus.FailToCommit;
                        return error;
                    }
                    _gitStatus = GitStatus.FailToPush;
                }

                if (_gitStatus == GitStatus.FailToPush)
                {
                    var error = await _git.PushAsync(progress).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(error))
                    {
                        _gitStatus = GitStatus.FailToPush;
                        return error;
                    }
                }
            }
            else if (ModelStatus == ModelStatus.FailToRemove)
            {
                if (_gitStatus == GitStatus.FailToRemove)
                {
                    var error = await _git.RemoveAsync(_latestRemovedImage).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(error))
                    {
                        _gitStatus = GitStatus.FailToRemove;
                        return error;
                    }
                    _gitStatus = GitStatus.FailToCommit;
                }

                if (_gitStatus == GitStatus.FailToCommit)
                {
                    var error = await _git.CommitAsync($"remove image {_latestRemovedImage}").ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(error))
                    {
                        _gitStatus = GitStatus.FailToCommit;
                        return error;
                    }
                    _gitStatus = GitStatus.FailToPush;
                }

                if (_gitStatus == GitStatus.FailToPush)
                {
                    var error = await _git.PushAsync(progress).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(error))
                    {
                        _gitStatus = GitStatus.FailToPush;
                        return error;
                    }
                }
            }
            else if (ModelStatus == ModelStatus.FailToSync)
            {
                if (_gitStatus == GitStatus.FailToPull)
                {
                    var error = await _git.PullAsync(progress).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(error))
                    {
                        _gitStatus = GitStatus.FailToPull;
                        return error;
                    }
                    _gitStatus = GitStatus.FailToPush;
                }

                if (_gitStatus == GitStatus.FailToPush)
                {
                    var error = await _git.PushAsync(progress).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(error))
                    {
                        _gitStatus = GitStatus.FailToPush;
                        return error;
                    }
                }
            }

            ModelStatus = ModelStatus.Normal;
            _gitStatus = GitStatus.Success;

            return string.Empty;
        }

        public async Task<string> CloneRepositoryAsync(IProgress<double> progress)
        {
            // 注意clone时，git的工作目录与其他操作不同的
            _git.WorkingDirectory = LocalStorageLocation;

            ModelStatus = ModelStatus.Processing;
            var error = await _git.CloneAsync(RepoGitURL, progress).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(error))
            {
                ModelStatus = ModelStatus.FailToClone;
                return error;
            }

            ModelStatus = ModelStatus.Normal;

            // 完成clone后，此时要将工作目录切到仓库里面
            _git.WorkingDirectory = Path.Combine(LocalStorageLocation, RepoName);
            return string.Empty;
        }
        #endregion

        // 其中Group0是UserName，Group1是RepoName
        [GeneratedRegex("https://[a-zA-Z.]*/([a-zA-Z0-9_-])*/([a-zA-Z0-9_-])*\\.git", RegexOptions.Compiled)]
        private static partial Regex UrlRegex();
        private static void ExtractUserNameAndRepoName(string url, out string userName, out string repoName)
        { 
            var match = UrlRegex().Match(url);
            if (match.Success)
            {
                userName = match.Groups[0].Value;
                repoName = match.Groups[1].Value;
            }
            else
            {
                throw new RegexNotMatch("The git Url maybe in the wrong format!");
            }
        }
    }
}
