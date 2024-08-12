using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingClick : MonoBehaviour
{
    public static event Action<BuildingClick> OnBuildingClicked;
    [SerializeField] private GameObject _buildingPrefab;

    public GameObject BuildingPrefab
    {
        get => _buildingPrefab;
        private set => _buildingPrefab = value;
    }

    public void OnClicked()
    {
        OnBuildingClicked?.Invoke(this);
    }
}
