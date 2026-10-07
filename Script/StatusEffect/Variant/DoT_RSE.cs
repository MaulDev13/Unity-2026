using UnityEngine;

public class DoT_RSE : Runtime_SE
{
    private float tickTimer;

    private DoT_SE DoTData =>
        (DoT_SE)baseData;

    public DoT_RSE(DoT_SE data, UnitController unit) : base(data, unit)
    {
        tickTimer = 0f;
    }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);

        tickTimer += deltaTime;

        while (tickTimer >= DoTData.tickInterval)
        {
            tickTimer -= DoTData.tickInterval;

            ApplyDoTDamage();

            Debug.Log($"Applied DoT damage: {DoTData.damagePerTick} to {unit.gameObject.name}. Remaining Duration: {remainingDuration}, Current Stacks: {currentStacks}");
        }
    }

    private void ApplyDoTDamage()
    {
        float damage = DoTData.damagePerTick * CurrentStacks;

        unit.TakeDamage(unit, damage);
    }
}