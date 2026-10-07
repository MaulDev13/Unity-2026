using UnityEngine;

public class Slow_RSE : Runtime_SE
{
    private readonly Slow_SE slowData;
    private readonly string modifierId;

    public Slow_RSE(Slow_SE baseData, UnitController unit) : base(baseData, unit)
    {
        slowData = baseData;
        modifierId = baseData.effectName;

        ApplySlow();
    }

    private void ApplySlow()
    {
        if (unit.Stat == null)
        {
            Debug.LogError(
                $"{unit.gameObject.name}: Cannot apply Slow because Runtime_US is not initialized."
            );

            return;
        }

        if(slowData.isPercent == false && slowData.isFlat == false)
        {
            Debug.LogWarning(
                $"{unit.gameObject.name}: Slow_SE has no effect because both isPercent and isFlat are false."
            );

            return;
        }

        if(slowData.isPercent == true)
        {
            if(slowData.slowPercent <= 0f)
            {
                Debug.LogWarning(
                    $"{unit.gameObject.name}: Slow_SE has no effect because slowPercent is less than or equal to 0."
                );
            }
            
            float totalSlowPercent = slowData.slowPercent * CurrentStacks;

            float percentModifierValue = -(totalSlowPercent / 100f);

            StatModifier percentModifier = new StatModifier(
                $"{modifierId}_percent",
                "movementSpeed",
                percentModifierValue,
                StatModifierType.Percent,
                sourceUnit: unit
            );

            unit.Stat.AddModifier(percentModifier);
        }

        if(slowData.isFlat == true)
        {
            if(slowData.slowFlat <= 0f)
            {
                Debug.LogWarning(
                    $"{unit.gameObject.name}: Slow_SE has no effect because slowFlat is less than or equal to 0."
                );
            }
            
            float totalSlowFlat =
                slowData.slowFlat * CurrentStacks;
            
            float flatModifierValue = -totalSlowFlat;

            StatModifier flatModifier = new StatModifier(
                $"{modifierId}_flat",
                "movementSpeed",
                flatModifierValue,
                StatModifierType.Flat,
                sourceUnit: unit
            );
            
            unit.Stat.AddModifier(flatModifier);
        }
    }

    public override void AddStack()
    {
        int oldStacks = CurrentStacks;

        base.AddStack();

        if (CurrentStacks != oldStacks)
        {
            ApplySlow();
        }
    }

    public override void OnRemove()
    {
        if (unit == null || unit.Stat == null)
            return;

        if(slowData.isPercent == true)
            unit.Stat.RemoveModifier($"{modifierId}_percent");
        if(slowData.isFlat == true)
            unit.Stat.RemoveModifier($"{modifierId}_flat");
    }
}
