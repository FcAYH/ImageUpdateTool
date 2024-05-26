# ImageUpdateTool

[Chinese version](README.md)

This is a tool to maintain the Github image bed. Just like PicGo (but the function is definitely not as powerful as PicGo).

![preview](https://cdn.jsdelivr.net/gh/FcAYH/Images/2024/05/26/583978c4ab8e78c088136968f3e7355d.png)

Usage:

1. You need to install git before using this software. Make sure to add the path of git to the system environment variables;
2. When you first open it, you will automatically enter the Settings interface. Please configure the information in this interface and click `Apply`;

![Settings Page](https://cdn.jsdelivr.net/gh/FcAYH/Images/2023/02/13/0deb8559103b0e8b891fd85dd52d39fe.png)

3. Click the `Upload Image` button, you can select the image you want to upload. After a successful upload, click the `Copy URL` button to copy the link of the most recently uploaded image.

Note:

The basic functions of this software are just in a state that can be used :joy:

1. Since accessing Github in China is quite mysterious (sometimes it can be accessed, sometimes it can't), git push/clone operations may fail due to network reasons. When git operations fail due to network reasons, you can wait a while and click Retry. If you hang a ladder for git, there will be basically no failure.


TODO:

- [x] git clone/push/pull asynchronous
- [x] Preview image function
- [x] Delete image function
- [ ] Progress bar
- [ ] Multilingual support
- [ ] Drag and drop upload
- [ ] The main interface ctrl + v can directly upload the content of the clipboard
- [ ] Support for functions that do not need to maintain the repository locally
- [ ] Optimize memory usage
- [ ] Add right-click menu for png, jpeg and other image formats, one-click upload