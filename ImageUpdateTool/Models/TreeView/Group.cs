namespace ImageUpdateTool.Models.TreeView;

[Serializable]
public class Group
{
    public List<Group> Children { get; } = new();
    public List<Item> ItemList { get; } = new();

    public string Name { get; set; }
    public int Id { get; set; }
}
