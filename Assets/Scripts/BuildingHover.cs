using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] GameObject popupPanel;

    private void Start()
    {
        popupPanel.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        popupPanel.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        popupPanel.SetActive(false);
    }
}
