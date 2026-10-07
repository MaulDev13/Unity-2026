using TMPro;
using UnityEngine;

public class Bio_Panel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text statText;

    [SerializeField]
    private UnitController unit;

    public void SetUnit(UnitController owner)
    {
        unit = owner;

        if (unit == null || unit.Stat == null)
        {
            nameText.text = "";
            hpText.text = "";
            statText.text = "";
            return;
        }

        nameText.text = unit.Stat.name;
        hpText.text = $"HP: {unit.Stat.MaxHealth} / {unit.Stat.CurrentHealth}";
        statText.text = $"Stat: {unit.Stat.MoveSpeed}";

        unit.OnHit += UpdateHPText;
        unit.OnTakeDamage += UpdateHPText;
        unit.OnHeal += UpdateHPText;
        unit.OnDeath += OnUnitDeath;
    }

    private void OnDisable()
    {
        if (unit != null)
        {
            unit.OnHit -= UpdateHPText;
            unit.OnTakeDamage -= UpdateHPText;
            unit.OnHeal -= UpdateHPText;
            unit.OnDeath -= OnUnitDeath;
        }
    }

    private void UpdateHPText(Collision2D target)
    {
        UpdateHPText(unit, 0f);
    }

    private void UpdateHPText(UnitController target, float damage)
    {
        if (unit.IsDead)
        {
            OnUnitDeath(unit);
            return;
        }

        if (unit.Stat.CurrentHealth < 0)
            unit.Stat.CurrentHealth = 0;

        hpText.text = $"HP: {unit.Stat.CurrentHealth} / {unit.Stat.MaxHealth}";
        statText.text = $"Stat: {unit.Stat.MoveSpeed}";
    }

    private void OnUnitDeath(UnitController owner)
    {
        if (unit == null || unit.Stat == null)
            return;

        if(!unit.IsDead)
        {
            return;
        }

        hpText.text = $"Dead";
        statText.text = "";
    }
}
