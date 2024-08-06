using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTileBase : MonoBehaviour
{
    [SerializeField] private GameObject _highlight;

    public void Init(Vector3 coordinate)
    {
        transform.position = coordinate;
    }

    void OnMouseEnter()
    {
        _highlight.SetActive(true);
    }

    void OnMouseExit()
    {
        _highlight.SetActive(false);
    }
}
