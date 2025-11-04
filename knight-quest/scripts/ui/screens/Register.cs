using System;
using Game.Autoloads;
using Game.Data;
using Game.Utils;
using Godot;
using GodotUtilities;
using Logger = Game.Utils.Logger;

namespace Game;

[Scene]
public partial class Register : CanvasLayer
{
    [Node] private LineEdit firstName;
    [Node] private LineEdit lastName;
    [Node] private LineEdit username;
    [Node] private LineEdit password;
    [Node] private LineEdit confirmPassword;
    [Node] private Button registerButton;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        registerButton.Pressed += OnRegisterButtonPressed;
    }

    private async void OnRegisterButtonPressed()
    {
        var firstNameText = firstName.Text.Trim();
        var lastNameText = lastName.Text.Trim();
        var usernameText = username.Text.Trim();
        var passwordText = password.Text;
        var confirmPasswordText = confirmPassword.Text;

        // Validate input
        if (string.IsNullOrEmpty(firstNameText))
        {
            PopupFactory.ShowError("Please enter your first name.");
            return;
        }

        if (string.IsNullOrEmpty(lastNameText))
        {
            PopupFactory.ShowError("Please enter your last name.");
            return;
        }

        if (string.IsNullOrEmpty(usernameText))
        {
            PopupFactory.ShowError("Please enter a username.");
            return;
        }

        if (string.IsNullOrEmpty(passwordText))
        {
            PopupFactory.ShowError("Please enter a password.");
            return;
        }

        if (passwordText != confirmPasswordText)
        {
            PopupFactory.ShowError("Passwords do not match. Please try again.");
            return;
        }

        if (passwordText.Length < 6)
        {
            PopupFactory.ShowError("Password must be at least 6 characters long.");
            return;
        }

        registerButton.Disabled = true;
        registerButton.Text = "Creating Account...";

        try
        {
            var body = new
            {
                firstName = firstNameText,
                lastName = lastNameText,
                username = usernameText,
                password = passwordText,
            };

            var response = await ApiClient.Post<AuthResponseData>("/auth/register", body);

            if (response == null)
            {
                PopupFactory.ShowError("Unable to connect to server. Please check your internet connection and try again.");
                Logger.Error("Registration failed: No response from server");
                return;
            }

            if (response.Success && response.Data != null)
            {
                SaveManager.Data.Account.Username = response.Data.Username;
                SaveManager.Data.Account.Token = response.Data.Token;
                SaveManager.Save();

                Logger.Info($"User registered successfully: {response.Data.Username}");
                PopupFactory.ShowSuccess(response.Message);

                AudioManager.Instance.PlayClick();
                Navigator.Push("res://scenes/ui/screens/main_menu.tscn");
            }
            else
            {
                var errorMessage = response.Message;

                switch (response.Code)
                {
                    case 400 when errorMessage.Contains("Username already taken"):
                        PopupFactory.ShowError("Username is already taken. Please choose a different username.");
                        break;
                    case 400 when errorMessage.Contains("Invalid teacherId"):
                        PopupFactory.ShowError("Invalid teacher ID. Please contact your teacher for assistance.");
                        break;
                    case 400 when errorMessage.Contains("Invalid parentId"):
                        PopupFactory.ShowError("Invalid parent ID. Please contact your parent for assistance.");
                        break;
                    case 400:
                        PopupFactory.ShowError(errorMessage);
                        break;
                    case 0:
                        PopupFactory.ShowError(
                            "Unable to connect to server. Please check your internet connection and try again.");
                        break;
                    default:
                        PopupFactory.ShowError(errorMessage);
                        break;
                }

                Logger.Error($"Registration failed: {errorMessage}");
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Unexpected error during registration: {ex.Message}");
            PopupFactory.ShowError("An unexpected error occurred. Please try again later.");
        }
        finally
        {
            registerButton.Disabled = false;
            registerButton.Text = "Register";
        }
    }
}