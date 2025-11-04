using Newtonsoft.Json;

namespace Game.Data;

public class PurchaseHistory
{
    [JsonProperty("id")] public string Id;
    [JsonProperty("quantity")] public int Quantity;
    [JsonProperty("cost")] public int Cost;
    [JsonProperty("purchasedAt")] public string PurchasedAt;
}