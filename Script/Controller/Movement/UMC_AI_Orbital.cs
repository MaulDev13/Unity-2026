using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class UMC_AI_Orbital : UnitMovementController
{
    [Header("Arena")]
    [SerializeField] private Vector2 arenaCenter = Vector2.zero;
    [SerializeField] private float centerPull = 2.0f;

    [Header("Movement")]
    [SerializeField] private float moveForce = 8.0f;
    [SerializeField] private float randomForce = 1.5f;

    [Header("Spin")]
    [SerializeField] private float spinSpeed = 1200f;
    [SerializeField] private float spinTorque = 50f;

    [Header("Physics")]
    [SerializeField] private float linearDamping = 0.5f;
    [SerializeField] private float angularDamping = 0.8f;

    private float spinDirection;

    public override void Init(UnitController controller)
    {
        base.Init(controller);

        Orbital_Init();
    }

    private void Orbital_Init()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;

        rb.drag = linearDamping;
        rb.angularDrag = angularDamping;

        spinDirection = Random.value > 0.5f ? 1f : -1f;

        rb.angularVelocity = spinSpeed * spinDirection;

        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        rb.AddForce(
            randomDirection * unit.Stat.MoveSpeed,
            ForceMode2D.Impulse
        );
    }

    public override void Tick()
    {
        timer += Time.deltaTime;
    }

    private void FixedUpdate()
    {
        ApplyBeybladeMovement();
        MaintainSpin();
    }

    private void ApplyBeybladeMovement()
    {
        Vector2 position = rb.position;

        // Direction from the unit toward the arena center.
        Vector2 toCenter = arenaCenter - position;

        if (toCenter.sqrMagnitude < 0.001f)
            return;

        Vector2 radialDirection = toCenter.normalized;

        // Rotate radial direction 90 degrees.
        // This creates the circular/tangential direction.
        Vector2 tangentDirection;

        if (spinDirection > 0)
        {
            tangentDirection = new Vector2(
                -radialDirection.y,
                radialDirection.x
            );
        }
        else
        {
            tangentDirection = new Vector2(
                radialDirection.y,
                -radialDirection.x
            );
        }

        // Pull slightly toward center.
        Vector2 inwardForce =
            radialDirection * centerPull;

        // Main circular movement.
        Vector2 orbitForce =
            tangentDirection * moveForce;

        // Small randomness so the movement isn't mathematically perfect.
        Vector2 random =
            Random.insideUnitCircle * randomForce;

        Vector2 totalForce =
            orbitForce +
            inwardForce +
            random;

        rb.AddForce(totalForce, ForceMode2D.Force);
    }

    private void MaintainSpin()
    {
        // Try to maintain spinning.
        float targetSpin = spinSpeed * spinDirection;

        float difference =
            targetSpin - rb.angularVelocity;

        rb.AddTorque(
            difference * spinTorque,
            ForceMode2D.Force
        );
    }

    protected override void OnHit(Collision2D collision)
    {
        // IMPORTANT:
        // Don't Reflect() the velocity anymore.
        //
        // Rigidbody2D collision physics will naturally
        // change the direction of the Beyblade.

        // Add a little impulse when hitting another object.
        Vector2 hitDirection =
            collision.relativeVelocity.normalized;

        if (hitDirection.sqrMagnitude > 0.01f)
        {
            rb.AddForce(
                hitDirection * unit.Stat.MoveSpeed * 0.5f,
                ForceMode2D.Impulse
            );
        }
    }

    protected override void OnDeath(UnitController deadUnit)
    {
        // Handle death logic.
    }
}