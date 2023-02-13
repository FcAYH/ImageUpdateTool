# ImageUpdateTool

这是一个用来维护Github图床的小工具。就像PicGo一样（不过功能肯定远不及PicGo强大啦）。

![preview](https://cdn.jsdelivr.net/gh/FcAYH/Images/2023/02/06/8d07ea9290936d18f3e30704f9062bb0.png)

使用方法：

1. 使用本软件需要先安装git，请确保将git的路径写入系统环境变量中；
2. 初次打开时，自动进入Settings界面，请在这个界面配置好各项信息，点击Apply；

![Settings界面](https://cdn.jsdelivr.net/gh/FcAYH/Images/2023/02/13/0deb8559103b0e8b891fd85dd52d39fe.png)

3. 点击Select Image按钮，就可以选择要上传的图片了，成功上传后点击Copy URL按钮，就可以复制下来最近上传的图片的链接。

注：

这个软件目前基本功能都在一个刚刚能用的状态 :joy:

1. 还没做进度条功能，所以在上传图片的时候你会感觉软件卡住了。
2. 由于在国内访问Github比较玄学（有时候能上，有时候上不去），所以git push/clone操作都可能会因为网络原因而失败。当由于网络原因git操作失败时，可以等一会点击Retry。如果为git挂梯子的话，基本不会出现失败的情况。
