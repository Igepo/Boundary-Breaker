using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject _prefabToInstantiate;

    private GameObject _instantiatedFloatingObject;
    private Camera _mainCamera;
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
            GameObject newTile = Instantiate(_prefabToInstantiate, tile.transform.position, Quaternion.identity, tile.transform.parent);

            Destroy(tile.gameObject);
            Destroy(_instantiatedFloatingObject);
            _instantiatedFloatingObject = null;

            PolygonCollider2D newTileCollider = newTile.GetComponentInChildren<PolygonCollider2D>();
            GridTileBase newTileGridTileBase = newTile.GetComponentInChildren<GridTileBase>();

            if (newTileCollider != null)
            {
                newTileCollider.enabled = true;
            }
            if (newTileGridTileBase != null)
            {
                Debug.Log($"newTile revealtype : {newTileGridTileBase.TileRevealType}");
            }
        }
    }
    void Start()
    {
        _mainCamera = Camera.main;
        PolygonCollider2D prefabCollider = _prefabToInstantiate.GetComponentInChildren<PolygonCollider2D>();

        if (prefabCollider != null)
        {
            prefabCollider.enabled = false;
        }

        if (_prefabToInstantiate != null)
        {
            _instantiatedFloatingObject = Instantiate(_prefabToInstantiate);
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
