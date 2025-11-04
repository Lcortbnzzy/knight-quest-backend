using System.Collections.Generic;
using Newtonsoft.Json;

namespace Game.Data;

public class Shop
{
    [JsonProperty("stars")] public int Stars;
    [JsonProperty("purchaseHistory")] public List<PurchaseHistory> PurchaseHistory = [];
}