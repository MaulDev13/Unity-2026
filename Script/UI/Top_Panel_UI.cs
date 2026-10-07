using UnityEngine;

public class Top_Panel_UI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform content;
    [SerializeField] private Bio_Panel bioPanelPrefab;

    [SerializeField] private UnitController[] units = new UnitController[0];

    private void Start()
    {
        CreateUnitPanels();
    }

    private void CreateUnitPanels()
    {
        if(units == null || units.Length == 0)
        {
            units = FindObjectsByType<UnitController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        }

        if (units == null || units.Length == 0)
        {
            Debug.LogWarning("No units found in the scene.");
            return;
        }

        foreach (UnitController unit in units)
        {
            if (unit == null || unit.baseStat == null)
                continue;

            Bio_Panel panel = Instantiate(bioPanelPrefab, content);

            panel.SetUnit(unit);
        }
    }
}
