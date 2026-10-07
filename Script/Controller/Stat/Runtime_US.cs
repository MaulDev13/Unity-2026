using System;
using System.Collections.Generic;

[Serializable]
public class Runtime_US
{
    public string name;

    public List<StatusEffectData> startingStatusEffects = new List<StatusEffectData>();
    public List<StatusEffectData> statusEffects = new List<StatusEffectData>();

    // =========================
    // Health
    // =========================

    public float MaxHealth;
    public float CurrentHealth;


    // =========================
    // Base Stats
    // =========================

    public float BaseMoveSpeed;
    public float BaseRotationSpeed;
    public bool BaseRandomMovement;
    public float BaseDirectionChangeInterval;

    public float BaseAttackDamage;
    public float BaseAttackRange;
    public float BaseAttackRate;

    public float BaseDefense;
    public float BaseVisionRange;


    // =========================
    // Final Stats
    // =========================

    public float MoveSpeed => GetModifiedValue("movementSpeed", BaseMoveSpeed);
    public float RotationSpeed => GetModifiedValue("rotationSpeed", BaseRotationSpeed);
    public float DirectionChangeInterval => GetModifiedValue("directionChangeInterval", BaseDirectionChangeInterval);

    public float AttackDamage => GetModifiedValue("attackDamage", BaseAttackDamage);
    public float AttackRange => GetModifiedValue("attackRange", BaseAttackRange);
    public float AttackRate => GetModifiedValue("attackRate", BaseAttackRate);

    public float Defense => GetModifiedValue("defense", BaseDefense);
    public float VisionRange => GetModifiedValue("visionRange", BaseVisionRange);


    // =========================
    // Modifiers
    // =========================

    private readonly List<StatModifier> modifiers = new List<StatModifier>();


    // =========================
    // Constructor
    // =========================

    public Runtime_US(UnitStat stat)
    {
        name = stat.unitName;

        startingStatusEffects = new List<StatusEffectData>(
            stat.startingStatusEffects
        );

        statusEffects = new List<StatusEffectData>(
            stat.statusEffects
        );

        // Health
        MaxHealth = stat.maxHealth;
        CurrentHealth = stat.maxHealth;

        // Base stats
        BaseMoveSpeed = stat.moveSpeed;
        BaseRotationSpeed = stat.rotationSpeed;
        BaseRandomMovement = stat.randomMovement;
        BaseDirectionChangeInterval = stat.directionChangeInterval;

        BaseAttackDamage = stat.attackDamage;
        BaseAttackRange = stat.attackRange;
        BaseAttackRate = stat.attackRate;

        BaseDefense = stat.defense;
        BaseVisionRange = stat.visionRange;
    }


    // =========================
    // Modifier Management
    // =========================

    public void AddModifier(StatModifier modifier)
    {
        if (modifier == null)
            return;

        // Prevent duplicate IDs.
        RemoveModifier(modifier.Id);

        modifiers.Add(modifier);
    }


    public void RemoveModifier(string modifierId)
    {
        modifiers.RemoveAll(
            modifier => modifier.Id == modifierId
        );
    }


    public void UpdateModifier(string modifierId, float newValue)
    {
        StatModifier modifier = modifiers.Find(
            modifier => modifier.Id == modifierId
        );

        if (modifier == null)
            return;

        modifier.SetValue(newValue);
    }


    public bool HasModifier(string modifierId)
    {
        return modifiers.Exists(
            modifier => modifier.Id == modifierId
        );
    }


    // =========================
    // Calculate Final Stat
    // =========================

    private float GetModifiedValue(string statName, float baseValue)
    {
        float finalValue = baseValue;

        foreach (StatModifier modifier in modifiers)
        {
            if (modifier.StatName != statName)
                continue;

            if (modifier.Type == StatModifierType.Flat)
            {
                finalValue += modifier.Value;
            }
            else if (modifier.Type == StatModifierType.Percent)
            {
                finalValue *= 1f + modifier.Value;
            }
        }

        return finalValue;
    }

}
