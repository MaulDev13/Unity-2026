using System;

public enum StatModifierType
{
    Flat,
    Percent
}


public class StatModifier
{
    public string Id { get; }
    public string StatName { get; }
    public float Value { get; private set; }
    public StatModifierType Type { get; }
    public UnitController SourceUnit { get; set; } // Optional: The unit that applied this modifier

    public StatModifier(string id, string statName, float value, StatModifierType type = StatModifierType.Flat, UnitController sourceUnit = null)
    {
        Id = id;
        StatName = statName;
        Value = value;
        Type = type;
        SourceUnit = sourceUnit;
    }

    public void SetValue(float value)
    {
        Value = value;
    }
}
