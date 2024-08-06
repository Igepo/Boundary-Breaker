using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionManager : MonoBehaviour
{
    public GameObject optionsPanel;

    private void Start()
    {
        optionsPanel.SetActive(false);
    }

    public void onMouseClicked()
    {
        optionsPanel.SetActive(true);
    }
}
