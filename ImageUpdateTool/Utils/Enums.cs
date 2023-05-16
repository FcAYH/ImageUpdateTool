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