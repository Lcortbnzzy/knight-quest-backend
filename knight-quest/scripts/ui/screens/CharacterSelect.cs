using Game.Autoloads;
using Game.Data;
using Godot;
using GodotUtilities;

namespace Game;

[Scene]
public partial class CharacterSelect : CanvasLayer
{
    [Export] private Character[] characters = [];

    [Node] private ResourcePreloader resourcePreloader;
    [Node] private HBoxContainer panelContainer;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        foreach (var character in characters)
        {
            var panel = resourcePreloader.InstanceSceneOrNull<CharacterPanel>();

            panel.character = character;
            panel.GuiInput += e => OnCharacterPanelGuiInput(e, character);

            panelContainer.AddChild(panel);
        }
    }

    private void OnCharacterPanelGuiInput(InputEvent @event, Character character)
    {
        var pressed = @event switch
        {
            InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } => true,
            InputEventScreenTouch { Pressed: true } => true,
            _ => false
        };

        if (!pressed) return;
        AudioManager.Instance.PlayClick();
        GameManager.SetCharacter(character);
        Navigator.Push("res://scenes/ui/screens/grade_level_select.tscn");
    }
}