using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    private int playerMaxTurn;

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
    public void SetMaxTurn(int MaxTurn)
    {
        playerMaxTurn = MaxTurn;
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
            turnText.text = $"Day: {playerTurn} / {playerMaxTurn}";
        }
    }
}
