using System;
using UnityEngine;

[Serializable]
public abstract class Runtime_SE
{
    [SerializeField]
    protected StatusEffectData baseData;

    [SerializeField]
    protected float remainingDuration;

    [SerializeField]
    protected int currentStacks;

    protected UnitController unit;

    // Particle belonging to this runtime status effect
    protected GameObject effectObject;

    public StatusEffectData BaseData => baseData;

    public float RemainingDuration => remainingDuration;

    public int CurrentStacks => currentStacks;

    public TriggerType TriggerType =>
        baseData != null ? baseData.triggerType : TriggerType.Default;

    public TargetType TargetType =>
        baseData != null ? baseData.targetType : TargetType.Default;

    public bool IsExpired =>
        baseData != null &&
        baseData.duration > 0f &&
        remainingDuration <= 0f;

    protected Runtime_SE(StatusEffectData baseData, UnitController unit)
    {
        if (baseData == null)
            throw new ArgumentNullException(nameof(baseData));

        if (unit == null)
            throw new ArgumentNullException(nameof(unit));

        this.baseData = baseData;
        this.unit = unit;

        remainingDuration = baseData.duration;
        currentStacks = 1;

        SpawnEffect();
    }

    public virtual void Tick(float deltaTime)
    {
        if (baseData == null)
            return;

        if (baseData.duration > 0f)
        {
            remainingDuration -= deltaTime;
        }
    }

    public virtual void RefreshDuration()
    {
        remainingDuration = baseData.duration;
    }

    public virtual void AddStack()
    {
        currentStacks = Mathf.Min(
            currentStacks + 1,
            baseData.maxStacks
        );
    }

    public virtual void RemoveStack()
    {
        currentStacks = Mathf.Max(
            currentStacks - 1,
            0
        );
    }

    protected virtual void SpawnEffect()
    {
        if (baseData.effectPrefab == null)
            return;

        effectObject = UnityEngine.Object.Instantiate(
            baseData.effectPrefab,
            unit.transform.position,
            Quaternion.identity,
            unit.transform
        );
    }

    public virtual void OnRemove()
    {
        DestroyEffect();
    }

    protected virtual void DestroyEffect()
    {
        if (effectObject == null)
            return;

        UnityEngine.Object.Destroy(effectObject);
        effectObject = null;
    }
}