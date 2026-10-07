using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(
    fileName = "UnitStat",
    menuName = "Game/Stats/Unit Stat")]
public class UnitStat : ScriptableObject
{
    [Header("Info")]
    public string unitName;

    public List<StatusEffectData> startingStatusEffects = new List<StatusEffectData>();
    public List<StatusEffectData> statusEffects = new List<StatusEffectData>();

    [Header("Health")]
    public float maxHealth = 100f;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 360f;
    public bool randomMovement = true;
    public float directionChangeInterval = 3f;

    [Header("Combat")]
    public float attackDamage = 10f;
    public float attackRange = 2f;
    public float attackRate = 1f;

    [Header("Defense")]
    public float defense = 0f;

    [Header("Detection")]
    public float visionRange = 10f;
    
}