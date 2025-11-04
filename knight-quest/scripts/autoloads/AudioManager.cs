using Game.Data;
using Godot;
using GodotUtilities;

namespace Game.Autoloads;

[Scene]
public partial class AudioManager : Autoload<AudioManager>
{
    [Node] private AudioStreamPlayer2D buttonClick;
    [Node] private AudioStreamPlayer2D menuMusic;
    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
    }

    public void StopMusic()
    {
        menuMusic.Stop();
    }

    public void ResumeMusic()
    {
        menuMusic.Play();
    }

    public void PlayClick()
    {
        buttonClick.Play();
    }
}