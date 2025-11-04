using Godot;
using System;
using GodotUtilities;

namespace Game.UI;

[Scene]
public partial class Popup : CanvasLayer
{
	[Node] public Label Label;
	[Node] public RichTextLabel RichTextLabel;
	[Node] public Button Button;

	public override void _Notification(int what)
	{
		if (what != NotificationSceneInstantiated) return;
		WireNodes();
	}

	public override void _Ready()
	{
		Button.Pressed += OnButtonPressed;
		GD.Print($"Popup _Ready called. Visible: {Visible}, Layer: {Layer}");
	}

	private void OnButtonPressed()
	{
		QueueFree();
	}

	public void Show()
	{
		Visible = true;
		GD.Print($"Popup Show() called. Visible: {Visible}");
	}

	public void Hide()
	{
		Visible = false;
		GD.Print($"Popup Hide() called. Visible: {Visible}");
	}
}
