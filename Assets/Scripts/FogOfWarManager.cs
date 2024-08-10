using System.Collections.Generic;
using UnityEngine;

public class FogOfWarManager : MonoBehaviour
{
    public static FogOfWarManager Instance { get; private set; }

    private List<GridTileBase> tilesList = new List<GridTileBase>();
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

    public void RegisterTile(GridTileBase tile)
    {
        if (!tilesList.Contains(tile))
        {
            tilesList.Add(tile);
        }
    }

    public void UnregisterTile(GridTileBase tile)
    {
        if (tilesList.Contains(tile))
        {
            tilesList.Remove(tile);
        }
    }

    public List<GridTileBase> GetTilesList()
    {
        return new List<GridTileBase>(tilesList);
    }
}
