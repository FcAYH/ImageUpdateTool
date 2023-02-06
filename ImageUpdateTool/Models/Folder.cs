using System.Collections.ObjectModel;

namespace ImageUpdateTool.Models;

internal class Folder
{
    public virtual string Name { get; set; }
    public virtual IList<Folder> Children { get; set; } = new ObservableCollection<Folder>();

    public virtual bool IsExpanded { get; set; } = false;

    public virtual bool IsLeaf { get; set; } = true;

    public string Path { get; set; }
    public double ButtonWidthRequest { get; set; } = 200;

    public Folder() { }
    
    public Folder(string name)
    {
        Name = name;
    }

    public Folder(string name, string path)
    {
        Name = name;
        Path = path;
    }
}
