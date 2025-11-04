using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game.Data;
using Godot;
using Newtonsoft.Json;
using Logger = Game.Utils.Logger;

namespace Game.Autoloads;

public partial class SaveManager : Autoload<SaveManager>
{
    public static Save Data { get; private set; } = new();
    private static readonly string dir = GetSaveDir();
    private static readonly string path = $"{dir}/save.json";

    private static bool loaded;

    public override void _ExitTree()
    {
        Save();
    }

    private static string GetSaveDir()
    {
        var isMobile = OS.HasFeature("mobile") || OS.HasFeature("android") || OS.HasFeature("ios");
        var isDesktop = OS.HasFeature("desktop") || OS.HasFeature("windows") || OS.HasFeature("linux") ||
                        OS.HasFeature("macos");

        if (isDesktop && OS.IsDebugBuild())
        {
            return "res://data";
        }

        return "user://data";
    }

    public static void StartSaving()
    {
        Load();
    }

    public static async void Load()
    {
        if (loaded) return;
        loaded = true;

        Logger.Info("Loading save data...");

        var serverData = await LoadFromServer();
        if (serverData != null)
        {
            Logger.Info("Successfully loaded save data from server");
            Data = serverData;
            LoadInventory();
            return;
        }

        if (FileAccess.FileExists(path))
        {
            var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
            var content = file.GetAsText();
            file.Close();

            Data = JsonConvert.DeserializeObject<Save>(content) ?? new Save();
            LoadInventory();
        }
        else
        {
            Logger.Debug("Save data not found creating new save data...");
            DirAccess.MakeDirAbsolute(dir);
            Data = new Save();
        }
    }

    public static async void Save()
    {
        Logger.Info("Saving data locally...");
        var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        var json = JsonConvert.SerializeObject(Data, Formatting.Indented);
        file.StoreString(json);
        file.Close();

        await SaveToServer();
    }

    public static void SaveInventory()
    {
        if (InventoryManager.Instance == null || InventoryManager.Instance.Items == null)
        {
            Logger.Error("[SaveInventory] InventoryManager or Items not initialized.");
            return;
        }

        Logger.Debug($"[SaveInventory] Saving {InventoryManager.Instance.Items.Count} items...");

        foreach (var group in InventoryManager.Instance.Items.Values)
        {
            if (group.Item == null)
            {
                Logger.Error("[SaveInventory] Tried to save null item, skipping...");
                continue;
            }

            var id = group.Item.Id;

            if (string.IsNullOrEmpty(id))
            {
                Logger.Error($"[SaveInventory] Could not resolve ID for item '{group.Item.Name}', skipping...");
                continue;
            }


            var existing = Data.Inventory.FirstOrDefault(i => i.Id == id);
            if (existing != null)
            {
                existing.Quantity = group.Quantity;
            }
            else
            {
                Data.Inventory.Add(new SavedItem
                {
                    Id = id,
                    Quantity = group.Quantity,
                    AcquiredAt = System.DateTime.UtcNow.ToString("s")
                });
            }
        }

        Save();
    }

    public static void LoadInventory()
    {
        if (Data == null) return;

        InventoryManager.Instance.ClearInventory();

        foreach (var saved in Data.Inventory)
        {
            if (string.IsNullOrEmpty(saved.Id))
            {
                Logger.Error("Skipping item with null/empty Id");
                continue;
            }

            var item = ItemRegistry.Get(saved.Id);
            if (item != null)
            {
                for (var i = 0; i < saved.Quantity; i++)
                    InventoryManager.Instance.AddItem(item);
            }
            else
            {
                Logger.Error($"Failed to load item with id: {saved.Id}");
            }
        }
    }

    public static void SaveFinishedLevel(string levelId, int starsEarned)
    {
        if (Data == null) return;
        var finishedLevel = new FinishedLevel
        {
            Id = levelId,
            StarsEarned = starsEarned,
            CompletedAt = System.DateTime.UtcNow.ToString("s")
        };
        Data.Progression.LevelsFinished.Add(finishedLevel);
        Data.Shop.Stars += starsEarned;
        Data.Progression.TotalStarsEarned += starsEarned;
        Save();
    }

    public static List<FinishedLevel> LoadFinishedLevels()
    {
        return Data?.Progression.LevelsFinished ?? [];
    }

    public static void SaveShopPurchase(string itemId, int quantity, int cost)
    {
        if (Data == null) return;
        var purchase = new PurchaseHistory
        {
            Id = itemId,
            Quantity = quantity,
            Cost = cost,
            PurchasedAt = System.DateTime.UtcNow.ToString("s")
        };
        Data.Shop.PurchaseHistory.Add(purchase);
        Save();
    }

    public static List<PurchaseHistory> LoadShopHistory()
    {
        return Data?.Shop.PurchaseHistory ?? [];
    }

    private static async Task<Save> LoadFromServer()
    {
        try
        {
            Logger.Info("Attempting to load save data from server...");

            if (!string.IsNullOrEmpty(Data.Account.Token))
            {
                ApiClient.SetAuthorizationBearer(Data.Account.Token);
            }

            var response = await ApiClient.Get<Save>("/save");

            if (response == null)
            {
                Logger.Warn("No response from server when loading save data.");
                return null;
            }

            if (response.Success && response.Data != null)
            {
                var serverSave = response.Data;

                serverSave.Account = Data.Account;

                return serverSave;
            }

            Logger.Warn($"Failed to load from server: {response.Message}");
            return null;
        }
        catch (System.Exception ex)
        {
            Logger.Error($"Error loading from server: {ex.Message}");
            return null;
        }
    }

    private static async Task<bool> SaveToServer()
    {
        try
        {
            Logger.Info("Attempting to save data to server...");

            if (!string.IsNullOrEmpty(Data.Account.Token))
            {
                ApiClient.SetAuthorizationBearer(Data.Account.Token);
            }

            var response = await ApiClient.Put<Save>("/save", new Save
            {
                Account = null,
                Progression = Data.Progression,
                Inventory = Data.Inventory,
                Shop = Data.Shop
            });

            if (response == null)
            {
                Logger.Warn("No response from server when saving data.");
                return false;
            }

            if (response.Success)
            {
                Logger.Info("Successfully saved to server");
                return true;
            }

            Logger.Warn($"Failed to save to server: {response.Message}");
            return false;
        }
        catch (System.Exception ex)
        {
            Logger.Error($"Error saving to server: {ex.Message}");
            return false;
        }
    }
}