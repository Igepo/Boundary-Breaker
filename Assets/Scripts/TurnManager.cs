using UnityEngine;
using System;
using UnityEngine.LowLevel;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    private int currentTurn = 0;

    // �v�nement d�clench� au d�but d'un nouveau tour
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

    // D�marre le premier tour
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
        
        // D�clencher l'�v�nement de d�but de tour
        OnTurnStarted?.Invoke();
    }

    // Fin du tour actuel, passe au suivant
    public void EndTurn()
    {
        Debug.Log($"Tour {currentTurn} termin�");

        // Ajoutez ici tout traitement n�cessaire � la fin d'un tour (par exemple, sauvegarder l'�tat)

        StartNewTurn();
    }
}
