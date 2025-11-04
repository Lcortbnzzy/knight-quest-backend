using Newtonsoft.Json;

namespace Game.Data;

public class SavedItem
{
    [JsonProperty("id")] public string Id;
    [JsonProperty("quantity")] public int Quantity;
    [JsonProperty("acquiredAt")] public string AcquiredAt;
}