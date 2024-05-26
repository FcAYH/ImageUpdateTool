using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageUpdateTool.Utils.Tools
{
    // Write by Copliot 看上去挺蠢的，明明C#都提供了对应的方法，非要自己写一遍
    internal static class DirectoryTools
    {
        public static void MoveDirectory(string sourcePath, string destPath)
        {
            // 检查源文件夹是否存在，如果不存在，抛出异常
            if (!Directory.Exists(sourcePath))
            {
                throw new DirectoryNotFoundException($"Source directory does not exist: {sourcePath}");
            }

            // 检查目标文件夹是否存在，如果不存在，创建它
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }

            // 获取源文件夹下的所有文件的路径，并遍历它们
            string[] files = Directory.GetFiles(sourcePath);
            foreach (string file in files)
            {
                // 获取文件的名称（不包含路径）
                string fileName = Path.GetFileName(file);
                // 拼接目标文件的完整路径
                string destFile = Path.Combine(destPath, fileName);
                // 将文件移动到目标文件夹下，如果目标文件已存在，覆盖它
                File.Move(file, destFile, true);
            }

            // 获取源文件夹下的所有子文件夹的路径，并遍历它们
            string[] subDirs = Directory.GetDirectories(sourcePath);
            foreach (string subDir in subDirs)
            {
                // 获取子文件夹的名称（不包含路径）
                string dirName = Path.GetFileName(subDir);
                // 拼接目标子文件夹的完整路径
                string destDir = Path.Combine(destPath, dirName);
                // 递归调用 MoveDirectory 方法，将子文件夹下的所有文件和子文件夹移动到目标子文件夹下
                MoveDirectory(subDir, destDir);
            }

            // 删除源文件夹（如果为空）
            Directory.Delete(sourcePath, true);
        }
    
        public static void DeleteDirectory(string dirPath)
        {
            // 检查文件夹是否存在，如果不存在，抛出异常
            if (!Directory.Exists(dirPath))
            {
                throw new DirectoryNotFoundException($"Directory does not exist: {dirPath}");
            }

            // 获取文件夹下的所有文件的路径，并遍历它们
            string[] files = Directory.GetFiles(dirPath);
            foreach (string file in files)
            {
                // 删除文件
                File.Delete(file);
            }

            // 获取文件夹下的所有子文件夹的路径，并遍历它们
            string[] subDirs = Directory.GetDirectories(dirPath);
            foreach (string subDir in subDirs)
            {
                DeleteDirectory(subDir);
            }
        }
    }
}
