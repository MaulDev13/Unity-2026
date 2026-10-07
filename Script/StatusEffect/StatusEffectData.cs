using UnityEngine;

public enum TargetType
{
    Self,
    Ally,
    Enemy,
    All,
    Default
}

public enum TriggerType
{
    OnHit,
    OnTakeDamage,
    OnHeal,
    OnDeath,
    // OnUpdate
    Default
}

public abstract class StatusEffectData : ScriptableObject
{
    [Header("Basic")]
    public string effectName;

    [TextArea]
    public string description;

    [Header("Duration")]
    [Min(0f)]
    public float duration = 5f;

    [Header("Stack")]
    [Min(1)]
    public int maxStacks = 1;

    [Header("Target")]
    public TargetType targetType;

    [Header("Trigger")]
    public TriggerType triggerType;

    public GameObject effectPrefab;

    public abstract Runtime_SE CreateRuntime(UnitController unit);
}