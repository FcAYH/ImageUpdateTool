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
    public partial class ImageRepositoryModel
    {
        private const string URL_FORMAT = "https://cdn.jsdelivr.net/gh/{0}/{1}";

        #region Fields
        private readonly AppSettings _settings;

        private string _repoGitURL;
        private string _localStorageLocation;
        private string _repoName;
        private string _userName;
        private int _repoSize;
        private string _latestUploadedImage; // 最近上传的图片的相对路径
        private string _latestRemovedImage; // 最近删除的图片的相对路径
        private ModelStatus _modelStatus;
        private string _currentSelectedDirectory; // 当前选中的文件夹的绝对路径
        private IProgress<double> _progress; // 用于在 git 操作时，汇报进度

        private GitStatus _gitStatus;
        private GitRunner _git;
        private string _gitUserName;
        private string _gitUserEmail;

        // 仅改变 url、仅改变 location、同时改变 url 和 location 需要用不同的逻辑处理
        private bool _urlChanged;
        private bool _locationChanged;
        #endregion

        #region Properties
        /// <summary>
        /// 图床仓库的 git URL链接，格式：https://github.com/[UserName]/[RepoName].git
        /// </summary>
        public string RepoGitURL
        {
            get => _repoGitURL;
            set
            {
                if (_repoGitURL != value)
                {
                    _repoGitURL = value;
                    _urlChanged = true;

                    ExtractUserNameAndRepoName(value, out string userName, out string repoName);
                    UserName = userName;
                    RepoName = repoName;
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
                    _locationChanged = true;
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
                    Preferences.Set(nameof(ModelStatus), value.ToString());
                }
            }
        }

        /// <summary>
        /// 记录 git 操作的状态，<see cref="ImageUpdateTool.Utils.GitStatus"/>
        /// </summary>
        public GitStatus GitStatus
        {
            get => _gitStatus;
            private set
            {
                if (_gitStatus != value)
                {
                    _gitStatus = value;
                    Preferences.Set(nameof(GitStatus), value.ToString());
                }
            }
        }

        /// <summary>
        /// 当前被选中，用于在<see cref="ImageUpdateTool.Views.ImageDisplayGrid"/>中展示的文件夹绝对路径
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

        public IProgress<double> Progress
        {
            get => _progress;
            set
            {
                if (_progress != value)
                {
                    _progress = value;
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
                    // TODO: 更新git的用户名
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
                    // TODO: 更新git的用户邮箱
                }
            }
        }

        /// <summary>
        /// 最近上传的图片的URL
        /// </summary>
        public string LatestUploadedImageUrl
        {
            get
            {
                if (string.IsNullOrEmpty(LatestUploadedImage))
                {
                    return string.Empty;
                }
                else
                {
                    return string.Format(URL_FORMAT, UserName, RepoName + "/" + LatestUploadedImage);
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
        protected ImageRepositoryModel()
        {
            var gitStatusStr = Preferences.Get(nameof(GitStatus), GitStatus.Success.ToString());
            if (!Enum.TryParse(gitStatusStr, out _gitStatus))
            {
                _gitStatus = GitStatus.Success;
            }

            var modelStatusStr = Preferences.Get(nameof(ModelStatus), ModelStatus.Normal.ToString());
            if (!Enum.TryParse(modelStatusStr, out _modelStatus))
            {
                _modelStatus = ModelStatus.Normal;
            }
        }

        public ImageRepositoryModel(AppSettings settings) : this()
        {
            _settings = settings;

            RepoGitURL = settings.ImageRepositoryURL;
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

            // 如果启动起来发现工作目录不存在，则创建，并 clone 仓库
            var gitWorkPath = Path.Combine(LocalStorageLocation, RepoName);
            if (!Directory.Exists(gitWorkPath))
            {
                Directory.CreateDirectory(gitWorkPath);
            }

            _git = new GitRunner(Path.Combine(LocalStorageLocation, RepoName));
        }

        #endregion

        private void LocalStorageLocationChanged(string lastLocation)
        {
            // 当本地存储位置发生变化时，需要做三件事情
            // 1. 将原位置的数据转移到新位置
            // 2. 更新_git的工作目录（应该要重新设置userName和email）
            // 3. 将事件传出去，通知ViewModels更新

            var lastPath = Path.Combine(lastLocation, _repoName);
            var currentPath = Path.Combine(_localStorageLocation, _repoName);

            // 1. 将原位置的数据转移到新位置
            try
            {
                DirectoryTools.MoveDirectory(lastPath, currentPath);
            }
            catch
            {
                // 当 url 和 location 同时变化时，这里会接收到一个：DirectoryNotFoundException
                // 因为在 url 变化时，将原文件夹删除了，所以这里会找不到原文件夹
                // 不过无所谓，最后要自检，如果移动不成功会直接在新位置 clone
            }
            finally
            {
                // 2. 更新_git的工作目录
                _git.WorkingDirectory = currentPath; // TODO：应该要重新设置 userName 和 email

                // TODO：移除这个事件，更换为自检通过后开启
                OnLocalStorageLocationChanged?.Invoke(_localStorageLocation);
            }
        }

        private void ImageRepositoryURLChanged(string lastUrl)
        {
            // 当图床仓库的URL发生变化时，仅需删除原仓库
            // 无需 clone，
            ExtractUserNameAndRepoName(lastUrl, out _, out string lastRepoName);
            string lastRepoPath = Path.Combine(LocalStorageLocation, lastRepoName);
            DirectoryTools.DeleteDirectory(lastRepoPath);
        }

        public async Task<string> Initialize()
        {
            _urlChanged = false;
            _locationChanged = false;

            GitUserName = _settings.GitUserName;
            GitUserEmail = _settings.GitUserEmail;
            var lastUrl = RepoGitURL;
            RepoGitURL = _settings.ImageRepositoryURL;
            var lastLocation = LocalStorageLocation;
            LocalStorageLocation = _settings.LocalStorageLocation;

            // url 和 location 发生变化要做清除或者迁移的工作
            // 而不用重新 clone，重新 clone 的工作放在 SelfCheck 中执行
            // 只要 url 发生变化，就要删除原来的仓库
            // 只要 location 发生变化，就要将原来的仓库转移到新位置
            if (_urlChanged)
                ImageRepositoryURLChanged(lastUrl);
            if (_locationChanged)
                LocalStorageLocationChanged(lastLocation);

            return await SelfCheck().ConfigureAwait(false);
        }

        /// <summary>
        /// 自检，验证当前 LocalStorageLocation 下有没有 Repo，
        /// 没有的话要重新 clone
        /// </summary>
        private async Task<string> SelfCheck()
        {
            if (!Directory.Exists(LocalStorageLocation))
            {
                Directory.CreateDirectory(LocalStorageLocation);
            }

            if (!Directory.Exists(Path.Combine(LocalStorageLocation, RepoName)))
            {
                return await CloneRepositoryAsync().ConfigureAwait(false);
            }

            // 检查仓库中是否有 .git 文件夹，如果没有（说明 git 环境被破坏了）
            // 清空后重新 clone

            string gitFolderPath = Path.Combine(LocalStorageLocation, RepoName, ".git");
            if (!Directory.Exists(gitFolderPath))
            {
                // 清空原仓库
                DirectoryTools.DeleteDirectory(Path.Combine(LocalStorageLocation, RepoName));

                // 重新 clone
                return await CloneRepositoryAsync().ConfigureAwait(false);
            }

            return string.Empty;
        }

        public string LocalPathToUrl(string localPath)
        {
            var relativePath = localPath.Replace(LocalStorageLocation, "").Replace("\\", "/").Trim('/');
            return string.Format(URL_FORMAT, UserName, relativePath);
        }

        #region Git-Methods
        /// <summary>
        /// 异步方法，用于上传图片
        /// <para>当ModelStatus不为Normal时，无法调用此方法</para>
        /// </summary>
        /// <param name="path">待上传图片的相对路径</param>
        /// <returns><see cref="string"/>类型, 若上传成功则内容为空，否则内容为错误信息</returns>
        /// <exception cref="ModelProcessingUnderErrorStatus"/>
        public async Task<string> UploadImageAsync(string path)
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

            error = await _git.PushAsync(Progress).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(error))
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
        /// <returns><see cref="string"/>类型，若移除成功则内容为空，否则内容为错误信息</returns>
        /// <exception cref="ModelProcessingUnderErrorStatus"/>
        public async Task<string> RemoveImageAsync(string path)
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

            error = await _git.PushAsync(Progress).ConfigureAwait(false);
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
        /// <param name="needPush">默认为false，仅拉取，若传入值为true，则会在拉取后推送</param>
        /// <returns><see cref="string"/>类型，若同步成功则内容为空，否则内容为错误信息</returns>
        /// <exception cref="ModelProcessingUnderErrorStatus"/>
        public async Task<string> SyncWithRemoteAsync(bool needPush = false)
        {
            if (ModelStatus != ModelStatus.Normal)
            {
                throw new ModelProcessingUnderErrorStatus("Image Repository has unresolved error that prevent this operation");
            }

            ModelStatus = ModelStatus.Processing;

            // 同步的步骤，Pull -> Push(if needPush == true)
            var error = await _git.PullAsync(Progress).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(error))
            {
                ModelStatus = ModelStatus.FailToSync;
                _gitStatus = GitStatus.FailToPull;
                return error;
            }

            if (needPush)
            {
                error = await _git.PushAsync(Progress).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(error))
                {
                    ModelStatus = ModelStatus.FailToSync;
                    _gitStatus = GitStatus.FailToPush;
                    return error;
                }
            }

            ModelStatus = ModelStatus.Normal;
            _gitStatus = GitStatus.Success;
            return string.Empty;
        }

        /// <summary>
        /// 异步方法，用于重试之前失败的操作。
        /// </summary>
        /// <returns><see cref="string"/>类型，若重试成功则内容为空，否则内容为错误信息</returns>
        public async Task<string> RetryAsync()
        {
            // 可能会出现的错误有以下四种：
            // 1. 上传图片失败 => 可能是 Add、Commit、Push 失败，需要将失败流程以及其后续流程全部重试
            // 2. 移除图片失败 => 可能是 Remove、Commit、Push失败，需要将失败流程以及其后续流程全部重试
            // 3. 同步失败 => 可能是 Pull、Push 失败，需要将失败流程以及其后续流程全部重试
            // 4. clone 操作失败 => 重新 clone

            if (ModelStatus == ModelStatus.FailToUpload)
            {
                ModelStatus = ModelStatus.Processing;

                if (_gitStatus == GitStatus.FailToAdd)
                {
                    var error = await _git.AddAsync(_latestUploadedImage).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(error))
                    {
                        ModelStatus = ModelStatus.FailToUpload;
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
                        ModelStatus = ModelStatus.FailToUpload;
                        _gitStatus = GitStatus.FailToCommit;
                        return error;
                    }
                    _gitStatus = GitStatus.FailToPush;
                }

                if (_gitStatus == GitStatus.FailToPush)
                {
                    var error = await _git.PushAsync(Progress).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(error))
                    {
                        ModelStatus = ModelStatus.FailToUpload;
                        _gitStatus = GitStatus.FailToPush;
                        return error;
                    }
                }
            }
            else if (ModelStatus == ModelStatus.FailToRemove)
            {
                ModelStatus = ModelStatus.Processing;

                if (_gitStatus == GitStatus.FailToRemove)
                {
                    var error = await _git.RemoveAsync(_latestRemovedImage).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(error))
                    {
                        ModelStatus = ModelStatus.FailToRemove;
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
                        ModelStatus = ModelStatus.FailToRemove;
                        _gitStatus = GitStatus.FailToCommit;
                        return error;
                    }
                    _gitStatus = GitStatus.FailToPush;
                }

                if (_gitStatus == GitStatus.FailToPush)
                {
                    var error = await _git.PushAsync(Progress).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(error))
                    {
                        ModelStatus = ModelStatus.FailToRemove;
                        _gitStatus = GitStatus.FailToPush;
                        return error;
                    }
                }
            }
            else if (ModelStatus == ModelStatus.FailToSync)
            {
                ModelStatus = ModelStatus.Processing;

                if (_gitStatus == GitStatus.FailToPull)
                {
                    var error = await _git.PullAsync(Progress).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(error))
                    {
                        ModelStatus = ModelStatus.FailToSync;
                        _gitStatus = GitStatus.FailToPull;
                        return error;
                    }
                    _gitStatus = GitStatus.FailToPush;
                }

                if (_gitStatus == GitStatus.FailToPush)
                {
                    var error = await _git.PushAsync(Progress).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(error))
                    {
                        ModelStatus = ModelStatus.FailToSync;
                        _gitStatus = GitStatus.FailToPush;
                        return error;
                    }
                }
            }
            else if (ModelStatus == ModelStatus.FailToClone)
            {
                ModelStatus = ModelStatus.Processing;
                var error = await _git.CloneAsync(RepoGitURL, Progress).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(error))
                {
                    ModelStatus = ModelStatus.FailToClone;
                    return error;
                }
            }

            ModelStatus = ModelStatus.Normal;
            _gitStatus = GitStatus.Success;

            return string.Empty;
        }

        public async Task<string> CloneRepositoryAsync()
        {
            // 注意 clone 时，git 的工作目录与其他操作不同的
            _git.WorkingDirectory = LocalStorageLocation;

            ModelStatus = ModelStatus.Processing;
            var error = await _git.CloneAsync(RepoGitURL, Progress).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(error))
            {
                ModelStatus = ModelStatus.FailToClone;
                return error;
            }

            ModelStatus = ModelStatus.Normal;
            _gitStatus = GitStatus.Success;

            // 完成 clone 后，此时要将工作目录切到仓库里面
            _git.WorkingDirectory = Path.Combine(LocalStorageLocation, RepoName);
            return string.Empty;
        }
        #endregion

        // 其中Group0是UserName，Group1是RepoName
        [GeneratedRegex("https://[a-zA-Z.]*/([a-zA-Z0-9_-]*)/([a-zA-Z0-9_-]*)\\.git", RegexOptions.Compiled)]
        private static partial Regex UrlRegex();
        private static void ExtractUserNameAndRepoName(string url, out string userName, out string repoName)
        {
            var match = UrlRegex().Match(url);
            if (match.Success)
            {
                userName = match.Groups[1].ToString();
                repoName = match.Groups[2].ToString();
            }
            else
            {
                throw new RegexNotMatch("The git Url maybe in the wrong format!");
            }
        }
    }
}
