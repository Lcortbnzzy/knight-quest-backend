using Newtonsoft.Json;

namespace Game.Data;

public class FinishedLevel
{
    [JsonProperty("id")] public string Id { get; set; }
    [JsonProperty("duration")] public int Duration { get; set; }
    [JsonProperty("starsEarned")] public int StarsEarned { get; set; }
    [JsonProperty("completedAt")] public string CompletedAt { get; set; }
}