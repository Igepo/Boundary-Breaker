using UnityEngine;
using UnityEngine.UI; // Utilisez ceci si vous utilisez le composant Text standard
using TMPro; // Utilisez ceci si vous utilisez TextMeshPro

public class ResourceUIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI woodText;
    [SerializeField] private TextMeshProUGUI stoneText;
    [SerializeField] private TextMeshProUGUI villagerText;

    [SerializeField] private TextMeshProUGUI turnText;

    private int playerWood;
    private int playerStone;
    private int playerVillager;

    private int playerTurn;

    private void Start()
    {
        UpdateUI();
    }

    public void SetWood(int woodAmount)
    {
        playerWood = woodAmount;
        UpdateUI();
    }
    
    public void SetStone(int stoneAmount)
    {
        playerStone = stoneAmount;
        UpdateUI();
    }
    public void SetVillager(int villagerAmount)
    {
        playerVillager = villagerAmount;
        UpdateUI();
    }
    public void SetTurn(int currentTurn)
    {
        playerTurn = currentTurn;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (woodText != null)
        {
            woodText.text = "Wood: " + playerWood;
        }

        if (stoneText != null)
        {
            stoneText.text = "Stone: " + playerStone;
        }

        if (villagerText != null)
        {
            villagerText.text = "Villager: " + playerVillager;
        }

        if (turnText != null)
        {
            turnText.text = "Day: " + playerTurn;
        }
    }
}
