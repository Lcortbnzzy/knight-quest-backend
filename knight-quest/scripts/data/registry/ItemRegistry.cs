using System.Collections.Generic;
using System.Linq;
using Game.Utils;
using Godot;

namespace Game.Data;

[GlobalClass]
public partial class ItemRegistry : Registry<Item, ItemRegistry>
{
    // Path in your Godot project where item .tres files are stored
    protected override string ResourcePath => "res://resources/item/";
    public static ItemRegistry PublicInstance => Instance.Value;
    public static Dictionary<string, Item> PublicResources => Resources;

    public static List<Item> GetItems(int maxCost) => Resources.Values
        .Where(item => item.Cost <= maxCost)
        .ToList();

    public static List<T> GetItemsByType<T>() where T : Item
    {
        return Resources.Values
            .OfType<T>()
            .ToList();
    }

    protected override void LoadResources()
    {
        var files = DirAccessUtils.GetFilesRecursively(ResourcePath);

        foreach (var file in files)
        {
            if (!file.EndsWith(".tres") && !file.EndsWith(".tres.remap")) continue;

            var loaded = ResourceLoader.Load<Item>(file);
            if (loaded == null) continue;
            
            loaded.ResourcePath = file;

            var id = file.GetFile().GetBaseName();
            loaded.Id = id; 
            Resources[id] = loaded;

        }
    }

}