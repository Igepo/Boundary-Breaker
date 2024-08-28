using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CostUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI woodText;
    [SerializeField] private TextMeshProUGUI stoneText;
    [SerializeField] private TextMeshProUGUI villagerText;

    [SerializeField] private TextMeshProUGUI produceResourceText;

    [SerializeField] private GameObject prefab;
    [SerializeField] private Image prefabImage;

    // Valeurs de ressources
    private int _woodCost;
    private int _stoneCost;
    private int _villagerCost;

    private int _produceResource;

    private bool _isCraftable = false;
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
        if (this.name == "Button House")
            produceResourceText.text = _produceResource.ToString();
        else
            produceResourceText.text = _produceResource.ToString() + "/day";
    }

    private void Update()
    {
        var gameManager = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
        var currentRessources = gameManager.GetCurrentRessources();
        
        var playerWood = currentRessources.playerWood;
        var playerStone = currentRessources.playerStone;
        var playerVillager = currentRessources.playerVillager;
        if (_woodCost > playerWood || _stoneCost > playerStone || _villagerCost > playerVillager)
        {
            Debug.Log("_woodCost");
            _isCraftable = false;
        }
        else
        {
            _isCraftable = true;
        }

        prefabImage.color = _isCraftable ? Color.white : Color.red;
    }
}
