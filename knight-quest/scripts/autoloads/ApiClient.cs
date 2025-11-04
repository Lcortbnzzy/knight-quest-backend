using Godot;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http;
using System.Threading.Tasks;
using HttpClient = System.Net.Http.HttpClient;
using Game.Data;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Game.Autoloads;

#nullable enable
public partial class ApiClient : Autoload<ApiClient>
{
    [Export] private string baseUrl = "";

    [Signal] public delegate void RequestCompletedEventHandler(Variant result);
    [Signal] public delegate void RequestFailedEventHandler(string error);

    private static readonly HttpClient client = new();

    public override void _Ready()
    {
        if (baseUrl == string.Empty) return;

        client.BaseAddress = new Uri(baseUrl);
    }

    public static void SetBaseUrl(string baseUrl)
    {
        Instance.baseUrl = baseUrl;
        client.BaseAddress = new Uri(baseUrl);
    }

    public static void SetDefaultHeader(string key, string value)
    {
        client.DefaultRequestHeaders.Add(key, value);
    }

    public static void SetAuthorizationBearer(string token)
    {
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    public static void SetTimeout(float seconds)
    {
        client.Timeout = TimeSpan.FromSeconds(seconds);
    }

    public static async Task<ApiResponse<T>?> Get<T>(string endpoint)
    {
        try
        {
            var response = await client.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<T>>(content);
            return apiResponse;
        }
        catch (Exception e)
        {
            Utils.Logger.Error($"GET request to {endpoint} failed: {e.Message}");
            return null;
        }
    }

    public static async Task<ApiResponse<T>?> Post<T>(string endpoint, object data)
    {
        try
        {
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<T>>(responseContent);
            return apiResponse;
        }
        catch (Exception e)
        {
            Utils.Logger.Error($"POST request to {endpoint} failed: {e.Message}");
            return null;
        }
    }

    public static async Task<ApiResponse<T>?> Put<T>(string endpoint, object data)
    {
        try
        {
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await client.PutAsync(endpoint, content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<T>>(responseContent);
            return apiResponse;
        }
        catch (Exception e)
        {
            Utils.Logger.Error($"PUT request to {endpoint} failed: {e.Message}");
            return null;
        }
    }
}