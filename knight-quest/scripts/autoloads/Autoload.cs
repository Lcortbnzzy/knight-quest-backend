using Godot;

namespace Game.Autoloads;

public abstract partial class Autoload<T> : Node where T : Autoload<T>
{
    public static T Instance { get; private set; }

    public override void _EnterTree()
    {
        Instance?.QueueFree();

        Instance = (T)this;
    }

    public override void _ExitTree()
    {
        Instance?.QueueFree();
        Instance = null;
    }

    public static void ConnectToSignal(StringName signal, Callable target, uint flags = 0)
    {
        if (Instance == null)
        {
            return;
        }

        if (Instance.IsConnected(signal, target))
        {
            return;
        }

        Instance.Connect(signal, target, flags);
    }

    public static void DisconnectFromSignal(StringName signal, Callable target)
    {
        if (Instance == null)
        {
            return;
        }

        if (!Instance.IsConnected(signal, target))
        {
            return;
        }

        Instance.Disconnect(signal, target);
    }
}