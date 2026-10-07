using UnityEngine;

[CreateAssetMenu(
    fileName = "SE_DamageOverTime",
    menuName = "Game/Status Effects/Damage Over Time"
)]
public class DoT_SE : StatusEffectData
{
    [Header("DoT")]
    [Min(0f)]
    public float damagePerTick = 5f;

    [Min(0.01f)]
    public float tickInterval = 1f;

    public override Runtime_SE CreateRuntime(UnitController unit)
    {
        return new DoT_RSE(this, unit);
    }
}