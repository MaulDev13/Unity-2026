using System;
using UnityEngine;
using System.Collections.Generic;

public class UnitController : MonoBehaviour
{
    public event Action<Collision2D> OnHit; // Collision2D info
    public event Action<UnitController, float> OnTakeDamage; // Attacker and Damage amount
    public event Action<UnitController, float> OnHeal; // Healer and Heal amount
    public event Action<UnitController> OnDeath; // Dead unit

    [Header("Stats")]
    public UnitStat baseStat;

    public List<UnitController> teams = new List<UnitController>();

    public Runtime_US Stat { get; private set; } 

    [Header("Controllers")]
    [SerializeField]
    public UnitMovementController movement;
    [SerializeField]
    public UnitCollisionController collision;
    [SerializeField]
    public UnitSEController statusEffect;

    [SerializeField]
    private bool isDead;

    [SerializeField]
    public bool IsDead => isDead;

    private void Awake()
    {
        if (movement == null)
        {
            movement = GetComponent<UnitMovementController>();
        }

        if (collision == null)
            collision = GetComponent<UnitCollisionController>();

        if (statusEffect == null)
            statusEffect = GetComponent<UnitSEController>();
    }

    private void Start()
    {
        if (baseStat != null)
        {
            Stat = new Runtime_US(baseStat);
        }
        else
        {
            Debug.LogError("Base stat is not assigned.");
        }

        if (movement != null)
        {
            movement.Init(this);
        }

        if (collision != null)
        {
            collision.Init(this);
        }

        if (statusEffect != null)
        {
            statusEffect.Init(this);
        }
    }

    private void Update()
    {
        if (isDead) return;

        if (movement != null)
            movement.Tick();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        // Debug.Log($"{gameObject.name} hit {collision.gameObject.name}.");

        OnHit?.Invoke(collision);
    }

    public void TakeDamage(UnitController attacker, float damage)
    {
        if (isDead) return;

        if (Stat == null)
        {
            Debug.LogError("Stat is not initialized.");
            return;
        }

        damage = Mathf.Max(0f, damage);

        Stat.CurrentHealth -= damage;

        Stat.CurrentHealth = Mathf.Max(0f, Stat.CurrentHealth);

        OnTakeDamage?.Invoke(attacker, damage);

        if (Stat.CurrentHealth <= 0f)
        {
            Dead();
        }
    }

    public void Heal(UnitController healer, float amount)
    {
        if (isDead) return;

        if (Stat == null)
        {
            Debug.LogError("Stat is not initialized.");
            return;
        }

        amount = Mathf.Max(0f, amount);

        Stat.CurrentHealth += amount;

        Stat.CurrentHealth = Mathf.Min(Stat.CurrentHealth, Stat.MaxHealth);

        OnHeal?.Invoke(healer, amount);

        Debug.Log($"{gameObject.name} healed {amount}. Current HP: {Stat.CurrentHealth}");
    }

    public void Dead()
    {
        if (isDead) return;

        Debug.Log($"{gameObject.name} is dead.");

        isDead = true;
        OnDeath?.Invoke(this);

        this.gameObject.SetActive(false);
    }
    
}