using UnityEngine;

public class Basic_UCC : UnitCollisionController
{
    protected override void OnHit(Collision2D collision)
    {
        if (collision == null)
            return;

        UnitController otherUnit = collision.gameObject.GetComponentInParent<UnitController>();

        if (otherUnit == null)
            return;

        if (otherUnit.IsDead)
            return;

        Debug.Log(
            $"{unitController.baseStat.unitName} collided with " +
            $"{otherUnit.baseStat.unitName}"
        );

        // Calculate damage here.
        float damage = unitController.Stat.AttackDamage;

        otherUnit.TakeDamage(unitController, damage);
    }

    protected override void OnDeath(UnitController deadUnit)
    {
        // Optional cleanup.
    }

}
