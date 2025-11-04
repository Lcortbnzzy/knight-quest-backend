using System.Collections.Generic;
using Newtonsoft.Json;

namespace Game.Data;

public class Account
{
    [JsonProperty("token")] public string Token { get; set; }
    [JsonProperty("username")] public string Username { get; set; }
    [JsonProperty("firstName")] public string FirstName { get; set; }
    [JsonProperty("lastName")] public string LastName { get; set; }
    [JsonProperty("role")] public string Role { get; set; }
}