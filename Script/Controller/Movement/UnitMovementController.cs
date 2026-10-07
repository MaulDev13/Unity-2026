using UnityEngine;

public abstract class UnitMovementController : MonoBehaviour
{
    protected UnitController unit;

    public float speed = 5f;
    public float rotationSpeed = 0f;

    public bool randomMovement = true;
    public float directionChangeInterval = 3f;
    protected float directionChangeTimer = 0f;

    protected Rigidbody2D rb;
    protected Vector2 moveDirection;
    protected float timer;

    public virtual void Init(UnitController controller)
    {
        unit = controller;

        unit.OnHit += OnHit;
        unit.OnDeath += OnDeath;

        speed = unit.Stat.MoveSpeed;
        rotationSpeed = unit.Stat.RotationSpeed;
        randomMovement = unit.Stat.BaseRandomMovement;
        directionChangeInterval = unit.Stat.BaseDirectionChangeInterval;
    }

    public virtual void OnDisable()
    {
        if (unit != null)
        {
            unit.OnHit -= OnHit;
            unit.OnDeath -= OnDeath;
        }
    }

    public abstract void Tick();

    protected virtual void OnHit(Collision2D collision)
    {
        // Default implementation does nothing.
    }

    protected virtual void OnDeath(UnitController deadUnit)
    {
        // Default implementation does nothing.
    }
}