using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageUpdateTool.Models
{
    internal class RepoDirectory
    {
        public string Name { get; set; }
        public string Path { get; set; }

        private DirectoryInfo _info;

        public FileInfo[] GetFiles()
        {
            return _info.GetFiles();
        }
    }
}
