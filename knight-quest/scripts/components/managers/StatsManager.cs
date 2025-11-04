using System;
using Game.Data;
using Godot;
using Game.Utils;

using Logger = Game.Utils.Logger;

namespace Game.Components;

[GlobalClass]
public partial class StatsManager : Node
{
    [Signal] public delegate void StatChangedEventHandler(int value, Stat stat);
    [Signal] public delegate void StatIncreasedEventHandler(int value, Stat stat);
    [Signal] public delegate void StatDecreasedEventHandler(int value, Stat stat);
    [Signal] public delegate void StatDepletedEventHandler(Stat stat);
    [Signal] public delegate void DamageTakenEventHandler(int amount);
    [Signal] public delegate void AttackReceivedEventHandler(Attack attack);

    [Export] private StatusEffectManager statusEffectManager;
    [Export] private bool Invulnerable;
    [Export] public int MaxHealth = 10;

    public int Health
    {
        get => health;
        private set => SetStat(ref health, value, Stat.Health);
    }

    [Export]
    public int Damage
    {
        get => damage;
        private set => SetStat(ref damage, value, Stat.Damage);
    }

    [Export(PropertyHint.Range, "0,100,1")]
    public int CriticalChance
    {
        get => critChance;
        private set => SetStat(ref critChance, value, Stat.CriticalChance);
    }

    private float CriticalChancePercent => 1f / CriticalChance;

    private int damage = 1;
    private int health;
    private int critChance = 25;

    private int baseMaxHealth;
    private int baseDamage;
    private Entity entity;

    public override void _Ready()
    {
        health = MaxHealth;

        baseMaxHealth = MaxHealth;
        baseDamage = Damage;
        entity = GetParentOrNull<Entity>();

        if (entity != null) return;

        Logger.Error("StatsManager must be a child of an Entity node.");
    }

    public void Heal(int amount, ModifyMode mode = ModifyMode.Value)
    {
        Health += mode switch
        {
            ModifyMode.Percentage => Mathf.RoundToInt(MaxHealth * (amount / 100f)),
            ModifyMode.Value => amount,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }

    public void TakeDamage(int amount, ModifyMode mode = ModifyMode.Value)
    {
        if (Invulnerable) return;

        Health -= mode switch
        {
            ModifyMode.Percentage => Mathf.RoundToInt(MaxHealth * (amount / 100f)),
            ModifyMode.Value => amount,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };

        EmitSignalDamageTaken(amount);
    }

    // Damage
    public void IncreaseDamage(int amount, ModifyMode mode = ModifyMode.Value)
    {
        Damage += mode switch
        {
            ModifyMode.Percentage => Mathf.RoundToInt(Damage * (amount / 100f)),
            ModifyMode.Value => amount,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }

    public void DecreaseDamage(int amount, ModifyMode mode = ModifyMode.Value)
    {
        Damage -= mode switch
        {
            ModifyMode.Percentage => Mathf.RoundToInt(Damage * (amount / 100f)),
            ModifyMode.Value => amount,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }

    public void SetInvulnerable(bool value)
    {
        Invulnerable = value;
    }

    private void SetStat(ref int stat, int value, Stat statType)
    {
        if (stat == value) return;

        var oldValue = stat;
        stat = Math.Clamp(value, 0, statType switch
        {
            Stat.Health => MaxHealth,
            _ => int.MaxValue
        });

        var diff = Math.Abs(value - oldValue);

        EmitSignalStatChanged(value, statType);
        EmitSignal(value > oldValue ? SignalName.StatIncreased : SignalName.StatDecreased, diff, (int)statType);

        if (value <= 0)
            EmitSignalStatDepleted(statType);
    }

    public void ReceiveAttack(Attack attack)
    {
        TakeDamage(attack.Damage);
        EmitSignalAttackReceived(attack);

        statusEffectManager.OnAttackReceived(attack);
    }

    public Attack CreateAttack(StatusEffect[] pool = null)
    {
        if (pool == null)
            pool = [];

        var attack = new DamageFactory.AttackBuilder(entity)
            .ApplyCriticalChance(CriticalChancePercent)
            .SetStatusEffectPool(pool)
            .Build();

        statusEffectManager.OnAttackOutgoing(attack);
        return attack;
    }

    public void TickStatusEffects() => statusEffectManager.TickAllStatusEffects();

    public enum ModifyMode
    {
        Percentage,
        Value
    }

    public enum Stat
    {
        Damage,
        Health,
        CriticalChance
    }
}