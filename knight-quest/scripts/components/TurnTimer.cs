using Game.Utils;
using Godot;

using Logger = Game.Utils.Logger;

namespace Game.Components;

[GlobalClass]
public partial class TurnTimer : Node
{
    [Export] public int TurnWait = 3;
    [Export] public bool OneShot;
    [Export] public bool AutoStart;

    [Signal] public delegate void TimeoutEventHandler();

    public int TurnLeft { get; private set; }
    public bool Paused { get; private set; }

    public override void _Ready()
    {
        var runManager = this.GetRunManager();

        if (runManager == null)
        {
            Logger.Error("RunManager does not exist in the scene tree.");
            return;
        }

        runManager.TurnEnded += OnTurnEnded;

        if (AutoStart) Start();
    }

    public void Start(int turns = -1)
    {
        TurnLeft = turns > 0 ? turns : TurnWait;
        Paused = false;
    }

    public void Stop()
    {
        Paused = true;
    }

    private void OnTurnEnded()
    {
        if (Paused) return;

        TurnLeft--;

        if (TurnLeft > 0) return;

        EmitSignal(SignalName.Timeout);

        if (!OneShot) TurnLeft = TurnWait;
    }
}