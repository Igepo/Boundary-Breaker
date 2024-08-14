using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CostUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI woodText;
    [SerializeField] private TextMeshProUGUI stoneText;
    [SerializeField] private TextMeshProUGUI villagerText;

    [SerializeField] private TextMeshProUGUI produceResourceText;

    [SerializeField] private GameObject prefab;

    // Valeurs de ressources
    private int _woodCost;
    private int _stoneCost;
    private int _villagerCost;

    private int _produceResource;

    void Start()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        var prefabGridTileBase = prefab.GetComponentInChildren<GridTileBase>();

        var costs = prefabGridTileBase.GetCosts();
        _woodCost = costs.woodCost;
        _stoneCost = costs.stoneCost;
        _villagerCost = costs.villagerCost;

        woodText.text = _woodCost.ToString();
        stoneText.text = _stoneCost.ToString();
        if (villagerText != null)
            villagerText.text = _villagerCost.ToString();

        var production = prefabGridTileBase.GetProduction();
        _produceResource = production.woodProduction + production.stoneProduction + production.villagerProduction;
        produceResourceText.text = _produceResource.ToString();
    }
}
