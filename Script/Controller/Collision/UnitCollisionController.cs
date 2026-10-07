using UnityEngine;

public abstract class UnitCollisionController : MonoBehaviour
{
    protected UnitController unitController;

    public virtual void Init(UnitController controller)
    {
        unitController = controller;

        unitController.OnHit += OnHit;
        unitController.OnDeath += OnDeath;
    }

    public virtual void OnDisable()
    {
        if (unitController != null)
        {
            unitController.OnHit -= OnHit;
            unitController.OnDeath -= OnDeath;
        }
    }

    protected virtual void OnHit(Collision2D collision)
    {
        // Collision logic goes here.
    }

    protected virtual void OnDeath(UnitController deadUnit)
    {
        // Cleanup / death-related collision logic.
    }
}
