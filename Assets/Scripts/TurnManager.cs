using UnityEngine;
using System;
using UnityEngine.LowLevel;

public class TurnManager : MonoBehaviour
{
    [SerializeField] private int maxTurn = 20;
    private int currentTurn = 0;

    public static event Action<TurnManager> OnGameOver;

    private void OnEnable()
    {
        GameManager.OnTurnEnded += EndTurn;
    }

    private void OnDisable()
    {
        GameManager.OnTurnEnded -= EndTurn;
    }

    private void Awake()
    {
        currentTurn = 0;
    }
    private void Start()
    {
        var resourceUIManager = FindObjectOfType<ResourceUIManager>();
        if (resourceUIManager != null)
        {
            resourceUIManager.SetMaxTurn(maxTurn);
        }


        StartNewTurn();        
    }

    public void StartNewTurn()
    {
        currentTurn++;

        var resourceUIManager = FindObjectOfType<ResourceUIManager>();
        if (resourceUIManager != null)
        {
            resourceUIManager.SetTurn(currentTurn);
        }
    }

    public void EndTurn(GameManager gameManager)
    {
        if (currentTurn >= maxTurn)
        {
            OnGameOver?.Invoke(this);
        }

        StartNewTurn();
    }
}
