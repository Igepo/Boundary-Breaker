using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CostUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI woodText;
    [SerializeField] private TextMeshProUGUI stoneText;

    [SerializeField] private GameObject prefab;

    // Valeurs de ressources
    private int woodCost;
    private int stoneCost;

    void Start()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        var prefabGridTileBase = prefab.GetComponentInChildren<GridTileBase>();

        var costs = prefabGridTileBase.GetCosts();
        woodCost = costs.woodCost;
        stoneCost = costs.stoneCost;

        woodText.text = woodCost.ToString();
        stoneText.text = stoneCost.ToString();
    }
}
