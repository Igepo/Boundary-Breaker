using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public TextMeshProUGUI tutorialText; // Le texte du tutoriel
    public int step = 0; // Le numéro d'étape du tutoriel
    private bool canMove = false; // Pour bloquer/débloquer les actions

    void Start()
    {
        // Au début, on affiche la première instruction
        ShowTutorialMessage("Your objective is simple: to reveal all the squares on the board before the 20 days are up. To do this, you'll need to make use of the buildings at your disposal.\r\nStart by placing your castle.");
    }

    void Update()
    {
    }

    // Fonction pour afficher un message à l'écran
    void ShowTutorialMessage(string message)
    {
        tutorialText.text = message;
    }

    // Exemples de fonctions bloquées
    public bool CanPlayerMove()
    {
        return canMove;
    }

    public void StepUp()
    {
        step++;
        switch (step)
        {
            case 1:
                ShowTutorialMessage("Great! Now build a mine on a mountain square! It will allow you to generate stone every day");
                break;
            case 2:
                ShowTutorialMessage("Awesome! Now build a sawmill on a forest square! This building generates wood for you every day.");
                break;
            case 3:
                ShowTutorialMessage("You've consumed a lot of resources, you should skip to the next day!");
                break;
            case 4:
                ShowTutorialMessage("Now place a house on a plain square. You'll be able to have your first villagers!");
                break;
            case 5:
                ShowTutorialMessage("And finally, place a watchtower, it's the building that reveals the most squares!");
                break;
        }
    }
}
