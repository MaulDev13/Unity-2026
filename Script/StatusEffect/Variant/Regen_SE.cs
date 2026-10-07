using UnityEngine;

[CreateAssetMenu(
    fileName = "Regen_SE",
    menuName = "Status Effects/Regen"
)]
public class Regen_SE : StatusEffectData
{
    [Header("Regen")]
    [Min(0f)]
    public float healPerTick = 5f;

    [Header("Tick")]
    [Min(0.01f)]
    public float tickInterval = 1f;

    public override Runtime_SE CreateRuntime(UnitController unit)
    {
        return new Regen_RSE(this, unit);
    }
}
