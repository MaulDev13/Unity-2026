using UnityEngine;

[CreateAssetMenu(
    fileName = "Slow_SE",
    menuName = "Game/Status Effects/Slow"
)]
public class Slow_SE : StatusEffectData
{
    [Header("Slow")]
    public bool isPercent = true;
    [Range(0f, 100f)]
    public float slowPercent = 20f;
    public bool isFlat = false;
    public float slowFlat = 0f;

    public override Runtime_SE CreateRuntime(UnitController unit)
    {
        return new Slow_RSE(this, unit);
    }
}
