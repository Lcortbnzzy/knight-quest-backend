using System;
using Godot;
using Game.Autoloads;
using Game.Utils;
using Game.Data;
using GodotUtilities;
using Logger = Game.Utils.Logger;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Game.UI;

[Scene]
public partial class Login : CanvasLayer
{
    [Node] private LineEdit usernameField;
    [Node] private LineEdit passwordField;
    [Node] private Button loginButton;
    [Node] private Button registerButton;
    [Node] private Button googleButton;
    [Node] private Node deeplink;

    [Export] private string googleAuthUrl;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        loginButton.Pressed += OnLoginPressed;
        registerButton.Pressed += OnRegisterPressed;
        googleButton.Pressed += OnGooglePressed;

        deeplink.Call("initialize");
    }

    private void OnDeepLinkReceived(RefCounted variant)
    {
        var link = new DeepLinkUrl(variant);
        var data = link.GetData();

        if (data == null) return;

        Logger.Debug($"Deep link received: {data}");

        if (link.GetHost() == "login")
        {
            HandleLoginDeeplink(link);
        }
    }

    private void HandleLoginDeeplink(DeepLinkUrl link)
    {
        var query = link.GetQuery();
        Logger.Debug($"Login deeplink query: {query}");

        var queryParams = ParseQueryString(query);

        if (queryParams.TryGetValue("token", out var token) && !string.IsNullOrEmpty(token))
        {
            Logger.Debug($"JWT token found: {token}");

            SaveManager.Data.Account.Token = token;

            Logger.Info($"User authenticated successfully: {SaveManager.Data.Account.Username}");

            Navigator.Push("res://scenes/ui/screens/main_menu.tscn");
        }
        else
        {
            Logger.Error("No token found in login deeplink");
        }
    }

    private Dictionary<string, string> ParseQueryString(string query)
    {
        var result = new Dictionary<string, string>();

        if (string.IsNullOrEmpty(query)) return result;

        var pairs = query.Split('&');
        foreach (var pair in pairs)
        {
            var keyValue = pair.Split('=', 2);
            if (keyValue.Length == 2)
            {
                result[keyValue[0]] = keyValue[1];
            }
        }

        return result;
    }

    private async void OnLoginPressed()
    {
        var username = usernameField.Text.Trim();
        var password = passwordField.Text;

        // Validate input
        if (string.IsNullOrEmpty(username))
        {
            PopupFactory.ShowError("Please enter your username.");
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            PopupFactory.ShowError("Please enter your password.");
            return;
        }

        loginButton.Disabled = true;
        loginButton.Text = "Logging in...";

        try
        {
            var body = new
            {
                username = username,
                password = password
            };

            var response = await ApiClient.Post<AuthResponseData>("/auth/login", body);

            switch (response)
            {
                case null:
                    PopupFactory.ShowError(
                        "Unable to connect to server. Please check your internet connection and try again.");
                    Logger.Error("Login failed: No response from server");
                    return;
                case { Success: true, Data: not null }:
                    SaveManager.Data.Account.Username = response.Data.Username;
                    SaveManager.Data.Account.Token = response.Data.Token;

                    Logger.Info($"User authenticated successfully: {response.Data.Username}");
                    PopupFactory.ShowSuccess(response.Message);

                    AudioManager.Instance.PlayClick();
                    Navigator.Push("res://scenes/ui/screens/main_menu.tscn");
                    break;
                default:
                {
                    var errorMessage = response.Message;

                    switch (response.Code)
                    {
                        case 401:
                            PopupFactory.ShowError(
                                "Invalid username or password. Please check your credentials and try again.");
                            break;
                        case 0:
                            PopupFactory.ShowError(
                                "Unable to connect to server. Please check your internet connection and try again.");
                            break;
                        default:
                            PopupFactory.ShowError(errorMessage);
                            break;
                    }

                    Logger.Error($"Login failed: {errorMessage}");
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Unexpected error during login: {ex.Message}");
            PopupFactory.ShowError("An unexpected error occurred. Please try again later.");
        }
        finally
        {
            loginButton.Disabled = false;
            loginButton.Text = "Login";
        }
    }

    private void OnRegisterPressed()
    {
        AudioManager.Instance.PlayClick();
        Navigator.Push("res://scenes/ui/screens/register.tscn");
    }

    private void OnGooglePressed()
    {
        AudioManager.Instance.PlayClick();
        OS.ShellOpen(googleAuthUrl);
    }
}