using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject _castlePrefab;

    [SerializeField] private List<GameObject> _UI = new();

    private GameObject _instantiatedFloatingObject;
    private Camera _mainCamera;
    private bool isCastlePut = false;

    private void OnEnable()
    {
        // S'abonner à l'événement de révélation des tuiles
        GridTileBase.OnTileClicked += HandleTileClicked;
    }

    private void OnDisable()
    {
        // Se désabonner de l'événement lorsqu'il n'est plus nécessaire
        GridTileBase.OnTileClicked -= HandleTileClicked;
    }

    private void HandleTileClicked(GridTileBase tile)
    {
        // Condition qui est seulement appelé au démarrage
        if (_instantiatedFloatingObject != null)
        {
            GameObject newTile = Instantiate(_castlePrefab, tile.transform.position, Quaternion.identity, tile.transform.parent);

            Destroy(tile.gameObject);
            Destroy(_instantiatedFloatingObject);
            _instantiatedFloatingObject = null;

            PolygonCollider2D newTileCollider = newTile.GetComponentInChildren<PolygonCollider2D>();
            GridTileBase newTileGridTileBase = newTile.GetComponentInChildren<GridTileBase>();
            var newTileGridTileBaseRevealType = newTileGridTileBase.TileRevealType;
            if (newTileCollider != null)
            {
                newTileCollider.enabled = true;
            }
            if (newTileGridTileBase != null)
            {
                // Assuming the tile's position is a `Vector3`:
                var coord = new Vector2(tile.transform.position.x, tile.transform.position.y);
                Debug.Log("coord " + coord);

                Debug.Log("_tileRevealType " + newTileGridTileBase.TileRevealType);
                var gridManager = FindObjectOfType<GridManager>();
                var adjacentTiles = gridManager.GetAdjacentTiles(coord, newTileGridTileBase.TileRevealType);

                foreach (var adjacentTile in adjacentTiles)
                {
                    adjacentTile.SetReveal(true);
                }
            }
            isCastlePut = true;
        }
    }
    void Start()
    {
        _mainCamera = Camera.main;
        OnBegin();
    }
    void OnBegin()
    {
        PolygonCollider2D prefabCollider = _castlePrefab.GetComponentInChildren<PolygonCollider2D>();

        if (prefabCollider != null)
        {
            prefabCollider.enabled = false;
        }

        if (_castlePrefab != null)
        {
            _instantiatedFloatingObject = Instantiate(_castlePrefab);
        }
    }
    void Update()
    {
        if (_instantiatedFloatingObject != null)
        {
            Vector3 mousePosition = Input.mousePosition;

            Vector3 worldPosition = _mainCamera.ScreenToWorldPoint(mousePosition);
            worldPosition.z = 0;

            _instantiatedFloatingObject.transform.position = worldPosition;
        }
    }
}
