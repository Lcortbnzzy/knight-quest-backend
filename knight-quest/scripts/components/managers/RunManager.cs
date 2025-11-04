using System.Collections.Generic;
using System.Linq;
using Game.Autoloads;
using Game.Data;
using Game.Entities;
using Game.UI;
using Game.Utils;
using Godot;
using Godot.Collections;
using GodotUtilities;
using Logger = Game.Utils.Logger;

namespace Game.Components;

[Scene]
public partial class RunManager : Node
{
    [Export] private HeadsUpDisplay hud;
    [Export] public CombatSequence Configuration { get; private set; }

    [Node] private Timer turnTimer;

    [Signal] public delegate void TurnEndedEventHandler();

    [Signal] public delegate void PlayerTurnTimeoutEventHandler();

    [Signal] public delegate void PlayerAnsweredEventHandler(bool correct);

    [Signal] public delegate void EncounterStartedEventHandler(Entity[] enemies);

    private PackedScene resultScene = GD.Load<PackedScene>("res://scenes/ui/overlays/result_screen.tscn");
    private ImmediateDelegateStateMachine stateMachine = new();
    private TurnEngine turnEngine = new();
    private Player player => this.GetPlayer();

    private Encounter currentEncounter;
    private readonly List<Entity> currentEnemies = [];
    private int currentEncounterIndex = -1;
    private Array<Entity> aliveEnemies => new(currentEnemies.Where(enemy => enemy.IsAlive).ToArray());
    private int currentTurn;
    private bool turnHandled;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        this.AddToGroup();

        stateMachine.AddState(Idle);
        stateMachine.AddState(PlayerTurn);
        stateMachine.AddState(EnemyTurn);
        stateMachine.AddState(EncounterTransition);
        stateMachine.AddState(Execute);
        stateMachine.AddState(Victory);
        stateMachine.AddState(Defeat);

        turnTimer.Timeout += OnTurnTimerTimeout;
        hud.AnswerSelected += OnAnswerSelected;

        GetTree().CreateTimer(1f).Timeout += Start;
    }

    public void Start()
    {
        stateMachine.ChangeState(EncounterTransition);
    }

    private void Idle() { }

    private void PlayerTurn()
    {
        if (aliveEnemies.Count == 0)
        {
            stateMachine.ChangeState(EncounterTransition);
            return;
        }

        QuestionManager.Instance.GetNextQuestion();
        turnHandled = false;
        turnTimer.Start(Configuration.TurnDuration);
        Logger.Debug("Player's turn started, waiting for answer.");
    }

    private async void EnemyTurn()
    {
        Logger.Debug("Enemy's turn started, selecting an enemy to act.");
        var enemy = aliveEnemies.PickRandom();

        if (enemy != null && IsInstanceValid(enemy) && enemy.IsAlive && player != null && player.IsAlive)
        {
            var action = enemy.GetTurnAction(player);
            await turnEngine.ExecuteAsync(this, action);
        }

        stateMachine.ChangeState(Execute);
    }

    private void Execute()
    {
        player.StatsManager.TickStatusEffects();

        foreach (var enemy in aliveEnemies)
        {
            enemy.StatsManager.TickStatusEffects();
        }

        EmitSignalTurnEnded();

        if (!player.IsAlive)
        {
            stateMachine.ChangeState(Defeat);
            return;
        }

        if (aliveEnemies.Count > 0)
        {
            stateMachine.ChangeState(PlayerTurn);
        }
        else
        {
            Logger.Debug("All enemies defeated, starting next encounter.");
            stateMachine.ChangeState(EncounterTransition);
        }
    }

    private void EncounterTransition()
    {
        if (currentEncounterIndex >= Configuration.Encounters.Length - 1)
        {
            Logger.Debug("All encounters completed, switching to victory state.");
            stateMachine.ChangeState(Victory);
            return;
        }

        currentEncounterIndex++;
        currentEncounter = Configuration.Encounters[currentEncounterIndex];
        currentEnemies.Clear();

        foreach (var enemy in currentEncounter.Enemies)
        {
            var enemyEntity = enemy.InstanceOrFree<Entity>();

            currentEnemies.Add(enemyEntity);
        }

        EmitSignalEncounterStarted(currentEnemies.ToArray());
        stateMachine.ChangeState(PlayerTurn);
    }

    private void Victory()
    {
        Logger.Debug("Victory state reached, handling victory logic.");
        var screen = resultScene.Instantiate();
        var levelResourcePath = GameManager.Config.Level.ResourcePath;
        var starsEarned = GameManager.Config.Level.StarCount;

        GetTree().Root.AddChild(screen);
        if (screen is ResultScreen resultScreen)
        {
            resultScreen.ShowResult(true, starsEarned);
        }

        SaveManager.SaveFinishedLevel(levelResourcePath, starsEarned);

        if (SaveManager.Data == null) return;

        SaveManager.Save();
    }

    private void Defeat()
    {
        Logger.Debug("Defeat state reached, handling defeat logic.");
        var screen = resultScene.Instantiate();
        GetTree().Root.AddChild(screen);
        if (screen is ResultScreen resultScreen)
        {
            resultScreen.ShowResult(false);
        }
    }

    private void OnTurnTimerTimeout()
    {
        if (stateMachine.GetCurrentState() != PlayerTurn || turnHandled) return;

        turnHandled = true;

        Logger.Debug("Turn timer timed out, switching to enemy turn.");
        EmitSignalPlayerTurnTimeout();
        stateMachine.ChangeState(EnemyTurn);
    }

    private async void OnAnswerSelected(int answerIndex)
    {
        if (stateMachine.GetCurrentState() != PlayerTurn || turnHandled) return;

        turnHandled = true;
        turnTimer.Stop();

        var correct = QuestionManager.Instance.IsAnswerCorrect(answerIndex);

        Logger.Debug($"Player answered  {(correct ? "correctly" : "incorrectly")}");

        EmitSignalPlayerAnswered(correct);

        if (correct)
        {
            if (aliveEnemies.Count > 0)
            {
                var enemy = aliveEnemies.PickRandom();
                if (
                    enemy != null && IsInstanceValid(enemy) &&
                    enemy.IsAlive &&
                    player is { IsAlive: true }
                )
                {
                    var action = player.GetTurnAction(enemy);
                    await turnEngine.ExecuteAsync(this, action);
                }
            }

            // TODO: make player take less damage or something else based on the answer
            stateMachine.ChangeState(Execute);
            return;
        }

        stateMachine.ChangeStateDeferred(EnemyTurn);
    }

    public void SetConfiguration(CombatSequence config)
    {
        Configuration = config;
    }
}