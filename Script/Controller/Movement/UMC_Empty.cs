using UnityEngine;

public class UMC_Empty : UnitMovementController
{
    public override void Tick()
    {
        // Intentionally empty.
    }

    protected override void OnHit(Collision2D collision)
    {
        // Intentionally empty.
    }

    protected override void OnDeath(UnitController deadUnit)
    {
        // Intentionally empty.
    }
    
}