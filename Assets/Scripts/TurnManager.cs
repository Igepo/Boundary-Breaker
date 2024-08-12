using UnityEngine;
using System;
using UnityEngine.LowLevel;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    private int currentTurn = 0;

    // Événement déclenché au début d'un nouveau tour
    public event Action OnTurnStarted;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Démarre le premier tour
    private void Start()
    {
        StartNewTurn();
    }

    // Commence un nouveau tour
    public void StartNewTurn()
    {
        currentTurn++;
        Debug.Log($"Tour {currentTurn} commence");

        var resourceUIManager = FindObjectOfType<ResourceUIManager>();
        if (resourceUIManager != null)
        {
            resourceUIManager.SetTurn(currentTurn);
        }
        
        // Déclencher l'événement de début de tour
        OnTurnStarted?.Invoke();
    }

    // Fin du tour actuel, passe au suivant
    public void EndTurn()
    {
        Debug.Log($"Tour {currentTurn} terminé");

        // Ajoutez ici tout traitement nécessaire à la fin d'un tour (par exemple, sauvegarder l'état)

        StartNewTurn();
    }
}
