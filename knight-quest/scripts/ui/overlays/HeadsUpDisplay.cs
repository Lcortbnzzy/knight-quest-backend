using System.Collections.Generic;
using System.Linq;
using Game.Autoloads;
using Game.Components;
using Game.Data;
using Godot;
using GodotUtilities;
using Game.Utils;

using Logger = Game.Utils.Logger;

namespace Game.UI;

[Scene]
public partial class HeadsUpDisplay : MarginContainer
{
    [Node] private Marker2D playerPosition;
    [Node] private Marker2D enemyPosition;
    [Node] private RichTextLabel questionLabel;
    [Node] private Button firstAnswerButton;
    [Node] private GridContainer itemContainer;
    [Node] private HealthBar enemyHealthBar;
    [Node] private HealthBar playerHealthBar;

    private List<Slot> slots;
    private Entity currentEnemy;

    [Signal]
    public delegate void AnswerSelectedEventHandler(int index);

    public Vector2 PlayerGlobalPosition => playerPosition.GetGlobalPosition();
    public Vector2 EnemyGlobalPosition => enemyPosition.GetGlobalPosition();

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;
        WireNodes();
    }

    public override void _Ready()
    {
        this.AddToGroup();

        slots = itemContainer.GetChildrenOfType<Slot>().ToList();
        foreach (var slot in slots)
        {
            slot.Pressed += UseConsumable;
        }

        PopulateSlots();

        InventoryManager.Instance.Updated += OnInventoryUpdate;

        var answerButtonGroup = firstAnswerButton.ButtonGroup;
        answerButtonGroup.Pressed += OnAnswerButtonPressed;
        QuestionManager.Instance.QuestionRequested += OnQuestionRequested;
    }

    public override void _ExitTree()
    {
        QuestionManager.Instance.QuestionRequested -= OnQuestionRequested;
        InventoryManager.Instance.Updated -= OnInventoryUpdate;
    }

    public void setPlayer(Entity player)
    {
        if (player?.StatsManager != null)
        {
            playerHealthBar.MaxHealth = player.StatsManager.MaxHealth;
            playerHealthBar.CurrentHealth = player.StatsManager.Health;
            playerHealthBar.UpdateHearts();
            player.StatsManager.StatChanged += UpdatePlayerHp;
        }
    }

    private void UpdatePlayerHp(int newValue, StatsManager.Stat stat)
    {
        if (stat == StatsManager.Stat.Health)
        {
            playerHealthBar.CurrentHealth = newValue;
            playerHealthBar.UpdateHearts();
        }
    }


    public void SetEnemy(Entity enemy)
    {
        currentEnemy = enemy;
        if (currentEnemy?.StatsManager != null)
        {
            enemyHealthBar.MaxHealth = currentEnemy.StatsManager.MaxHealth;
            enemyHealthBar.CurrentHealth = currentEnemy.StatsManager.Health;
            enemyHealthBar.UpdateHearts();
            currentEnemy.StatsManager.StatChanged += UpdateEnemyHp;
        }
    }

    private void UpdateEnemyHp(int newValue, StatsManager.Stat stat)
    {
        if (stat == StatsManager.Stat.Health)
        {
            enemyHealthBar.CurrentHealth = newValue;
            enemyHealthBar.UpdateHearts();
        }
    }

    // ===============================
    // QUESTION SYSTEM
    // ===============================
    private void Reset()
    {
        questionLabel.Text = string.Empty;

        var answerButtons = firstAnswerButton.ButtonGroup.GetButtons();
        foreach (var button in answerButtons)
        {
            if (button is not Button answerButton) continue;
            answerButton.Text = string.Empty;
            answerButton.Visible = false;
            answerButton.ButtonPressed = false;
            answerButton.Disabled = false;
        }
    }

    public void ToggleAnswerButtons(bool disabled = false)
    {
        var answerButtons = firstAnswerButton.ButtonGroup.GetButtons();
        foreach (var button in answerButtons)
        {
            if (button is not Button answerButton) continue;
            if (answerButton.ButtonPressed)
            {
                answerButton.ButtonPressed = false;
                continue;
            }

            answerButton.Disabled = disabled;
        }
    }

    private void OnAnswerButtonPressed(BaseButton button)
    {
        AudioManager.Instance.PlayClick();
        var answerIndex = button.GetMeta("answer_index").As<int>();
        Logger.Debug($"Answer selected: {answerIndex}");
        EmitSignalAnswerSelected(answerIndex);
        ToggleAnswerButtons(true);
    }

    private void OnQuestionRequested(Question question)
    {
        Reset();
        questionLabel.Text = question.QuestionText;

        var answerButtons = firstAnswerButton.ButtonGroup.GetButtons();
        for (var i = 0; i < answerButtons.Count; i++)
        {
            if (answerButtons[i] is not Button answerButton) continue;
            if (i < question.Answers.Length)
            {
                answerButton.Text = question.Answers[i];
                answerButton.Visible = true;
            }
            else
            {
                answerButton.Visible = false;
            }
        }
    }

    public void RevealCorrectAnswer()
    {
        var question = QuestionManager.Instance.CurrentQuestion;
        if (question == null) return;

        var answerButtons = firstAnswerButton.ButtonGroup.GetButtons();
        var correctIndex = question.CorrectAnswerIndex;

        Logger.Debug($"highlighted answer:{correctIndex}");

        if (correctIndex < 0 || correctIndex >= answerButtons.Count) return;
        if (answerButtons[correctIndex] is not Button correctButton) return;

        correctButton.Modulate = Colors.Yellow;

        GetTree().CreateTimer(1f).Timeout += () => { correctButton.Modulate = Colors.White; };
    }

    private void UseConsumable(Consumable consumable)
    {
        if (consumable == null || this.GetPlayer() is null) return;

        Logger.Info($"Used consumable: {consumable.Name}");

        InventoryManager.Instance.UseItem(consumable, this.GetPlayer());
    }

    private void PopulateSlots()
    {
        // TODO: Load from config
        var items = InventoryManager.Instance.Items.Values
            .Where(ig => ig.Quantity > 0)
            .ToList();

        Logger.Info($"Populating slots: {items.Count} items.");
        foreach (var slot in slots)
        {
            slot.ItemGroup = null;
        }


        foreach (var (item, quantity) in items)
        {
            var slot = slots.FirstOrDefault(s => s.ItemGroup != null && item.Equals(s.ItemGroup.Item));
            if (slot != null)
            {
                slot.ItemGroup.Quantity = quantity;
                Logger.Debug($"Updated slot for {item.Name} with quantity {quantity}.");
                continue;
            }

            slot = slots.FirstOrDefault(s => s.ItemGroup == null);
            if (slot != null)
            {
                slot.ItemGroup = new ItemGroup
                {
                    Item = item,
                    Quantity = quantity
                };
                Logger.Debug($"Populated slot with {item.Name}.");
                continue;
            }

            Logger.Warn("No empty slots available to populate.");
        }
    }


    private void OnInventoryUpdate(ItemGroup itemGroup)
    {
        Logger.Info("InventoryUpdated");
        PopulateSlots();
    }
}