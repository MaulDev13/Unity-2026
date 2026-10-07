using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class UMC_AI2D : UnitMovementController
{
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        PickRandomDirection();
    }

    public override void Tick()
    {
        timer += Time.deltaTime;

        if (timer >= directionChangeTimer && randomMovement)
        {
            directionChangeTimer += directionChangeInterval;
            PickRandomDirection();
        }
    }

    private void FixedUpdate()
    {
        rb.velocity = moveDirection * unit.Stat.MoveSpeed;
    }

    private void PickRandomDirection()
    {
        moveDirection = Random.insideUnitCircle.normalized;
    }

    protected override void OnHit(Collision2D collision)
    {
        moveDirection = Vector2.Reflect(moveDirection, collision.contacts[0].normal).normalized;
    }

    protected override void OnDeath(UnitController deadUnit)
    {
        // Handle death logic for AI units if needed.
    }
}