using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitSEController : MonoBehaviour
{
    private UnitController unit;

    [Header("Status Effects (Readonly)")]
    [SerializeField]
    private List<Runtime_SE> statusEffects = new();

    public IReadOnlyList<Runtime_SE> StatusEffects => statusEffects;

    public void Init(UnitController owner)
    {
        unit = owner;

        unit.OnHit += OnHit;
        unit.OnTakeDamage += OnTakeDamage;
        unit.OnHeal += OnHeal;
        unit.OnDeath += OnDeath;

        InitializeStatusEffects();
    }

    public void OnDisable()
    {
        if (unit != null)
        {
            unit.OnHit -= OnHit;
            unit.OnTakeDamage -= OnTakeDamage;
            unit.OnHeal -= OnHeal;
            unit.OnDeath -= OnDeath;
        }
    }

    private void InitializeStatusEffects()
    {
        statusEffects.Clear();

        foreach (StatusEffectData data in unit.baseStat.startingStatusEffects)
        {
            if (data == null)
                continue;

            AddStatusEffect(data);
        }
    }

    private void Update()
    {
        if (unit == null)
            return;

        if (unit.IsDead)
            return;

        TickStatusEffects();
    }

    private void TickStatusEffects()
    {
        for (int i = statusEffects.Count - 1; i >= 0; i--)
        {
            Runtime_SE statusEffect = statusEffects[i];

            if (statusEffect == null)
            {
                statusEffects.RemoveAt(i);
                continue;
            }

            statusEffect.Tick(Time.deltaTime);

            if (statusEffect.IsExpired)
            {
                RemoveStatusEffect(statusEffect);
            }
        }
    }

    public void AddStatusEffect(StatusEffectData data)
    {
        if (data == null)
        {
            Debug.LogWarning(
                $"{unit.gameObject.name}: Cannot add null status effect."
            );

            return;
        }

        if (unit.IsDead)
            return;

        foreach (Runtime_SE existingSE in statusEffects)
        {
            if (existingSE.BaseData == data)
            {
                existingSE.AddStack();
                existingSE.RefreshDuration();

                Debug.Log(
                    $"{unit.gameObject.name} refreshed status effect: {data.effectName}. " +
                    $"Current Stacks: {existingSE.CurrentStacks}, Remaining Duration: {existingSE.RemainingDuration}"
                );

                return;
            }
        }

        Runtime_SE runtimeSE = data.CreateRuntime(unit);

        statusEffects.Add(runtimeSE);

        Debug.Log(
            $"{unit.gameObject.name} received status effect: {data.effectName}"
        );
    }

    public void SpawnParticle(Runtime_SE data)
    {
        GameObject particle = Instantiate(
            data.BaseData.effectPrefab,
            transform.position,
            Quaternion.identity,
            transform
        );
    }

    public void RemoveStatusEffect(Runtime_SE runtimeSE)
    {
        if (runtimeSE == null)
            return;

        runtimeSE.OnRemove();
        statusEffects.Remove(runtimeSE);
    }

    public void RemoveStatusEffect(StatusEffectData data)
    {
        if (data == null)
            return;

        for (int i = statusEffects.Count - 1; i >= 0; i--)
        {
            Runtime_SE runtimeSE = statusEffects[i];

            if (runtimeSE != null &&
                runtimeSE.BaseData == data)
            {
                runtimeSE.OnRemove();
                statusEffects.RemoveAt(i);
            }
        }
    }

    public Runtime_SE GetStatusEffect(StatusEffectData data)
    {
        if (data == null)
            return null;

        for (int i = 0; i < statusEffects.Count; i++)
        {
            Runtime_SE runtimeSE = statusEffects[i];

            if (runtimeSE != null &&
                runtimeSE.BaseData == data)
            {
                return runtimeSE;
            }
        }

        return null;
    }

    public bool HasStatusEffect(StatusEffectData data)
    {
        return GetStatusEffect(data) != null;
    }

    public void ClearStatusEffects()
    {
        statusEffects.Clear();
    }

    // ========================= Trigger Handlers =========================
    public void OnHit(Collision2D collision)
    {
        collision.gameObject.TryGetComponent<UnitController>(out UnitController targetUnit);

        if(targetUnit == null)
        {
            return;
        }

        Debug.LogWarning($"{unit.gameObject.name} hit {collision.gameObject.name}. Unit status effects: {unit.Stat.statusEffects.Count}");

        if (targetUnit != null)
        {
            foreach (StatusEffectData statusEffect in unit.baseStat.statusEffects)
            {
                Debug.LogWarning(
                    $"{unit.gameObject.name} has status effect: {statusEffect.effectName} with trigger type: {statusEffect.triggerType}"
                );

                if (statusEffect.triggerType == TriggerType.OnHit)
                {
                    switch (statusEffect.targetType)
                    {
                        case TargetType.Self:
                            unit.statusEffect.AddStatusEffect(statusEffect);
                            Debug.Log(
                                $"{unit.gameObject.name} applied {statusEffect.effectName} to self."
                            );
                            break;

                        case TargetType.Enemy:
                            targetUnit.statusEffect.AddStatusEffect(statusEffect);
                            Debug.Log(
                                $"{unit.gameObject.name} applied {statusEffect.effectName} to {targetUnit.gameObject.name}."
                            );
                            break;

                        case TargetType.All:
                            unit.statusEffect.AddStatusEffect(statusEffect);
                            targetUnit.statusEffect.AddStatusEffect(statusEffect);
                            Debug.Log(
                                $"{unit.gameObject.name} applied {statusEffect.effectName} to self and {targetUnit.gameObject.name}."
                            );
                            break;
                    }
                }
            }
        }
        
    }

    public void OnTakeDamage(UnitController attacker, float damage)
    {
        foreach (Runtime_SE statusEffect in statusEffects)
        {
            if (statusEffect.TriggerType == TriggerType.OnTakeDamage)
            {
                attacker.statusEffect.AddStatusEffect(statusEffect.BaseData);
            }
        }
    }

    public void OnHeal(UnitController healer, float amount)
    {
        foreach (Runtime_SE statusEffect in statusEffects)
        {
            if (statusEffect.TriggerType == TriggerType.OnHeal)
            {
                healer.statusEffect.AddStatusEffect(statusEffect.BaseData);
            }
        }
    }

    public void OnDeath(UnitController owner)
    {
        foreach (Runtime_SE statusEffect in statusEffects)
        {
            if (statusEffect.TriggerType == TriggerType.OnDeath)
            {
            
            }
        }
    }


}