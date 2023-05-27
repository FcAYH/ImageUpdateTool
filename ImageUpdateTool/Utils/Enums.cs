namespace ImageUpdateTool.Utils;

public enum ImageExtension
{
    png,
    jpeg,
    gif,
    tiff,
    bmp,
    jpg,
}

public enum GitStatus
{
    FailToPull, 
    Success, 
    FailToAdd, 
    FailToCommit, 
    FailToPush,
    FailToRemove
}

public enum ModelStatus
{
    Normal,
    Processing,
    FailToClone,
    FailToUpload,
    FailToRemove,
    FailToSync,
}

public enum Language
{
    English,
    简体中文
}