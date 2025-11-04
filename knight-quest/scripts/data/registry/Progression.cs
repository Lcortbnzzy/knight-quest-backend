using System.Collections.Generic;
using Newtonsoft.Json;

namespace Game.Data;

public class Progression
{
    [JsonProperty("totalStarsEarned")] public int TotalStarsEarned { get; set; }
    [JsonProperty("levelsFinished")] public List<FinishedLevel> LevelsFinished { get; set; } = new();
}