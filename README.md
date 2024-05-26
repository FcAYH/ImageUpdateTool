# ImageUpdateTool

> [English version](README_en.md)

这是一个用来维护 Github 图床的小工具。就像 PicGo 一样（不过功能肯定远不及 PicGo 强大啦）。

![preview](https://cdn.jsdelivr.net/gh/FcAYH/Images/2024/05/26/1c7b044204a76b6d89e0125f8f712259.png)

使用方法：

1. 使用本软件需要先安装 git，请确保将 git 的路径写入系统环境变量中；
2. 初次打开时，自动进入 Settings 界面，请在这个界面配置好各项信息，点击 `Apply`；

![Settings界面](https://cdn.jsdelivr.net/gh/FcAYH/Images/2023/02/13/0deb8559103b0e8b891fd85dd52d39fe.png)

3. 点击 `Upload Image` 按钮，就可以选择要上传的图片了，成功上传后点击 `Copy URL` 按钮，就可以复制下来最近上传的图片的链接。

注：

这个软件目前基本功能都在一个刚刚能用的状态 :joy:

1. 由于在国内访问 Github 比较玄学（有时候能上，有时候上不去），所以 git push/clone 操作都可能会因为网络原因而失败。当由于网络原因git操作失败时，可以等一会点击Retry。如果为 git 挂梯子的话，基本不会出现失败的情况。

后续计划：

功能：

- [ ] 上传、同步、删除图片时，使用进度条显示进度
- [ ] 拖拽上传
- [ ] 主界面ctrl + v可直接上传剪切板的内容
- [ ] 多语言支持
- [ ] 对png、jpeg等图片格式，添加右键菜单，一键上传

体验优化：

- [ ] 图片异步加载
- [ ] 图片预览时支持滚轮缩放
- [ ] 优化内存占用
- [ ] 优化 TreeView 在上传、删除图片后的刷新逻辑

