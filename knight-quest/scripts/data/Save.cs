using System.Collections.Generic;
using Newtonsoft.Json;

namespace Game.Data;

public class Save
{
    [JsonProperty("account")] public Account Account = new();
    [JsonProperty("progression")] public Progression Progression = new();
    [JsonProperty("inventory")] public List<SavedItem> Inventory = [];
    [JsonProperty("shop")] public Shop Shop = new();
}