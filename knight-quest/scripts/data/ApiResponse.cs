using System;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Game.Data;
#nullable enable
public class ApiResponse<T>
{
    [JsonProperty("code")] public int Code { get; set; }
    [JsonProperty("success")] public bool Success { get; set; }
    [JsonProperty("message")] public string Message { get; set; } = string.Empty;
    [JsonProperty("data")] public T? Data { get; set; }
    [JsonProperty("error")] public object? Error { get; set; }
    [JsonProperty("errorCode")] public string? ErrorCode { get; set; }
}

public class ApiResponse : ApiResponse<object> { }

public class AuthResponseData
{
    [JsonProperty("id")] public int Id { get; set; }
    [JsonProperty("username")] public string Username { get; set; } = string.Empty;
    [JsonProperty("firstName")] public string FirstName { get; set; } = string.Empty;
    [JsonProperty("lastName")] public string LastName { get; set; } = string.Empty;
    [JsonProperty("role")] public string Role { get; set; } = string.Empty;
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
    [JsonProperty("token")] public string Token { get; set; } = string.Empty;
}