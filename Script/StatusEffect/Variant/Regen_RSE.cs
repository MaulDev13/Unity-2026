using UnityEngine;

public class Regen_RSE : Runtime_SE
{
    private float tickTimer;

    private Regen_SE RegenData =>
        (Regen_SE)baseData;

    public Regen_RSE(Regen_SE data, UnitController unit) : base(data, unit)
    {
        tickTimer = 0f;
    }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);

        tickTimer += deltaTime;

        while (tickTimer >= RegenData.tickInterval)
        {
            tickTimer -= RegenData.tickInterval;

            ApplyRegen();

            Debug.Log(
                $"Applied Regen: " +
                $"{RegenData.healPerTick} " +
                $"to {unit.gameObject.name}. " +
                $"Remaining Duration: {remainingDuration}, " +
                $"Current Stacks: {currentStacks}"
            );
        }
    }

    private void ApplyRegen()
    {
        float heal = RegenData.healPerTick * CurrentStacks;

        unit.Heal(unit, heal);
    }
}
