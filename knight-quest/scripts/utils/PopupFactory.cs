using System.Collections.Generic;
using System.Linq;
using Godot;
using Popup = Game.UI.Popup;

namespace Game.Utils;

public static class PopupFactory
{
    private static readonly Dictionary<string, Popup> activePopups = new();

    public static void ShowMessage(string title, string message, string buttonText = "OK")
    {
        var popupScene = ResourceLoader.Load<PackedScene>("res://scenes/ui/overlays/popup.tscn");
        if (popupScene == null)
        {
            GD.PrintErr("Failed to load popup scene");
            return;
        }

        var popup = popupScene.Instantiate<Popup>();
        if (popup == null)
        {
            GD.PrintErr("Failed to instantiate popup");
            return;
        }

        popup.Label.Text = title;
        popup.RichTextLabel.Text = message;
        popup.Button.Text = buttonText;

        var sceneTree = Engine.GetMainLoop() as SceneTree;
        var root = sceneTree?.CurrentScene;
        if (root == null) 
        {
            GD.PrintErr("No current scene found");
            return;
        }

        popup.Layer = 100;
        root.AddChild(popup);
        popup.Show();
        
        popup.TreeExiting += () => {
            var popupId = popup.GetInstanceId().ToString();
            activePopups.Remove(popupId);
        };

        activePopups[popup.GetInstanceId().ToString()] = popup;
        GD.Print($"Popup created and added to scene. Layer: {popup.Layer}, Visible: {popup.Visible}");
    }

    public static void ShowError(string message, string title = "Error")
    {
        ShowMessage(title, message, "OK");
    }

    public static void ShowSuccess(string message, string title = "Success")
    {
        ShowMessage(title, message, "OK");
    }

    public static void ShowWarning(string message, string title = "Warning")
    {
        ShowMessage(title, message, "OK");
    }

    public static void ShowInfo(string message, string title = "Information")
    {
        ShowMessage(title, message, "OK");
    }

    public static void ClearAllPopups()
    {
        foreach (var popup in activePopups.Values.Where(popup => GodotObject.IsInstanceValid(popup)))
        {
            popup.QueueFree();
        }
        activePopups.Clear();
    }
}