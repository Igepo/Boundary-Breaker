using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using static GridTileBase;
using Random = System.Random;

public class GridManager : MonoBehaviour
{
    [SerializeField] private Vector2Int _size;
    [SerializeField] private Vector2 _gap;
    [SerializeField, Range(0, 0.8f)] private float _skipAmount = 0.1f;
    [SerializeField, Range(0, 1)] private float _forestAmount = 0.2f;
    [SerializeField, Range(0, 1)] private float _mountainAmount = 0.1f;
    [SerializeField] private GridType _gridType;

    [Header("Grid Configurations")]
    [SerializeField] private GridLayout.CellLayout _layout;
    [SerializeField] private Vector3 _cellSize;
    [SerializeField] private GridLayout.CellSwizzle _gridSwizzle;
    [SerializeField] private GridTileBase _plainPrefab;
    [SerializeField] private GridTileBase _forestPrefab;
    [SerializeField] private GridTileBase _mountainPrefab;

    private bool _requiresGeneration = true;
    private Grid _grid;

    private Vector2 _currentGap;
    private Vector2 _gapVel;

    private Bounds bounds = new();
    private bool boundsInitialized = false;

    private Dictionary<Vector3, GridTileBase> _tileDictionary = new Dictionary<Vector3, GridTileBase>();

    public List<GridTileBase> TilesList { get; private set; } = new List<GridTileBase>();
    public static event Action OnTilesInitialized;

    private CameraController _cameraController;

    private void OnEnable()
    {
        hideFlags = HideFlags.DontUnloadUnusedAsset;
        GridTileBase.OnTileRevealed += HandleTileRevealed;
    }

    private void OnDisable()
    {
        GridTileBase.OnTileRevealed -= HandleTileRevealed;
    }

    private void HandleTileRevealed(GridTileBase tile)
    {
        if (tile == null)
        {
            return;
        }

        var tileRendererBound = tile.GetComponent<Renderer>().bounds;
        if (!boundsInitialized)
        {
            bounds = new Bounds(tileRendererBound.center, tileRendererBound.size);
            boundsInitialized = true;
        }
        else
        {
            bounds.Encapsulate(tileRendererBound);
        }

        _cameraController?.SetCamera(bounds);
    }

    private void Awake()
    {
        _grid = GetComponent<Grid>();
        _currentGap = _gap;

        _cameraController = FindObjectOfType<CameraController>();
    }

    private void OnValidate() => _requiresGeneration = true;

    private void LateUpdate()
    {
        if (Vector2.Distance(_currentGap, _gap) > 0.01f)
        {
            _currentGap = Vector2.SmoothDamp(_currentGap, _gap, ref _gapVel, 0.1f);
            _requiresGeneration = true;
        }

        if (_requiresGeneration) Generate();
    }

    private void Generate()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        if (_plainPrefab == null || _forestPrefab == null || _mountainPrefab == null)
        {
            Debug.LogError("One or more prefabs are not set.");
            return;
        }

        _grid.cellLayout = _layout;
        _grid.cellSize = _cellSize;
        if (_grid.cellLayout != GridLayout.CellLayout.Hexagon) _grid.cellGap = _currentGap;
        _grid.cellSwizzle = _gridSwizzle;

        var coordinates = new List<Vector3Int>();

        for (int x = 0; x < _size.x; x++)
        {
            for (int y = 0; y < _size.y; y++)
            {
                coordinates.Add(new Vector3Int(x, y));
            }
        }

        var mapBounds = new Bounds();
        var skipCount = Mathf.FloorToInt(coordinates.Count * _skipAmount);
        var forestCount = Mathf.FloorToInt(coordinates.Count * _forestAmount);
        var mountainCount = Mathf.FloorToInt(coordinates.Count * _mountainAmount);
        var forestIndex = 0;
        var mountainIndex = 0;
        var rand = new Random();

        foreach (var coordinate in coordinates.OrderBy(t => rand.Next()).Take(coordinates.Count - skipCount))
        {
            var isForest = forestIndex++ < forestCount;
            var isMountain = !isForest && mountainIndex++ < mountainCount;
            GridTileBase prefab;
            GridTileBase.TileType tileType;

            if (isForest)
            {
                prefab = _forestPrefab;
                tileType = GridTileBase.TileType.Forest;
            }
            else if (isMountain)
            {
                prefab = _mountainPrefab;
                tileType = GridTileBase.TileType.Mountain;
            }
            else
            {
                prefab = _plainPrefab;
                tileType = GridTileBase.TileType.Plain;
            }

            var position = _grid.GetCellCenterWorld(coordinate);
            var spawned = Instantiate(prefab, position, Quaternion.identity, transform);
            spawned.Init(position, tileType);

            var sortingGroup = spawned.GetComponent<SortingGroup>();
            if (sortingGroup == null)
            {
                sortingGroup = spawned.gameObject.AddComponent<SortingGroup>();
            }
            sortingGroup.sortingOrder = -(int)(position.y * 100);

            mapBounds.Encapsulate(spawned.GetComponent<Renderer>().bounds);
            _tileDictionary[position] = spawned;
        }
        _cameraController?.SetCamera(mapBounds);

        OnTilesInitialized?.Invoke();

        _requiresGeneration = false;
    }

    public int GetTotalTiles()
    {
        return _tileDictionary.Count;
    }

    public List<GridTileBase> GetAdjacentTiles(Vector2 coord, RevealType revealType)
    {
        List<GridTileBase> adjacentTiles = new List<GridTileBase>();

        Vector2[] crossOffsets = new Vector2[]
        {
            new Vector3(0.5f, -0.25f),  // bottom-right
            new Vector3(-0.5f, 0.25f), // top-left

            new Vector3(0f, 0f), // Center

            new Vector3(0.5f, 0.25f),  // top-right
            new Vector3(-0.5f, -0.25f)  // bottom-left
        };

        Vector2[] squareOffsets = new Vector2[]
        {
            new Vector3(0.5f, -0.25f),  // bottom-right
            new Vector3(-0.5f, 0.25f), // top-left
            new Vector3(0.5f, 0.25f),  // top-right
            new Vector3(-0.5f, -0.25f),  // bottom-left

            new Vector3(0f, 0f), // Center

            new Vector3(1f, 0f), // top
            new Vector3(0f, -0.5f), // Right
            new Vector3(0f, 0.5f), // Left
            new Vector3(-1f, 0f) // bottom
        };

        Vector2[] starOffsets = new Vector2[]
        {
            new Vector3(0.5f, -0.25f),  // bottom-right
            new Vector3(-0.5f, 0.25f), // top-left
            new Vector3(0.5f, 0.25f),  // top-right
            new Vector3(-0.5f, -0.25f),  // bottom-left

            new Vector3(0f, 0f), // Center

            new Vector3(1f, 0f), // bottom
            new Vector3(0f, -0.5f), // Left
            new Vector3(0f, 0.5f), // Right
            new Vector3(-1f, 0f), // top

            new Vector3(1f, -0.5f),  // bottom-right +1
            new Vector3(-1f, 0.5f), // top-left +1
            new Vector3(1f, 0.5f),  // top-right +1
            new Vector3(-1f, -0.5f),  // bottom-left +1
        };

        Vector2[] offsets;
        if (revealType == RevealType.Cross)
        {
            offsets = crossOffsets;
        }
        else if (revealType == RevealType.Square)
        {
            offsets = squareOffsets;
        }
        else if (revealType == RevealType.Star)
        {
            offsets = starOffsets;
        }
        else
            return adjacentTiles; // Type None

        foreach (var offset in offsets)
        {
            Vector3 neighborCoord = coord + offset;

            foreach (var tileEntry in _tileDictionary)
            {
                Vector2 tilePosition = new Vector2(tileEntry.Key.x, tileEntry.Key.y);
                if (Vector2.Distance(tilePosition, neighborCoord) < 0.1f)
                {
                    adjacentTiles.Add(tileEntry.Value);
                }
            }
        }

        return adjacentTiles;
    }

    public List<GridTileBase> GetAdjacentTilesWithoutCenter(Vector2 coord, RevealType revealType)
    {
        List<GridTileBase> adjacentTiles = new List<GridTileBase>();

        Vector2[] crossOffsets = new Vector2[]
        {
            new Vector3(0.5f, -0.25f),  // bottom-right
            new Vector3(-0.5f, 0.25f), // top-left

            new Vector3(0.5f, 0.25f),  // top-right
            new Vector3(-0.5f, -0.25f)  // bottom-left
        };

        Vector2[] squareOffsets = new Vector2[]
        {
            new Vector3(0.5f, -0.25f),  // bottom-right
            new Vector3(-0.5f, 0.25f), // top-left
            new Vector3(0.5f, 0.25f),  // top-right
            new Vector3(-0.5f, -0.25f),  // bottom-left

            new Vector3(1f, 0f), // top
            new Vector3(0f, -0.5f), // Right
            new Vector3(0f, 0.5f), // Left
            new Vector3(-1f, 0f) // bottom
        };

        Vector2[] starOffsets = new Vector2[]
        {
            new Vector3(0.5f, -0.25f),  // bottom-right
            new Vector3(-0.5f, 0.25f), // top-left
            new Vector3(0.5f, 0.25f),  // top-right
            new Vector3(-0.5f, -0.25f),  // bottom-left

            new Vector3(1f, 0f), // bottom
            new Vector3(0f, -0.5f), // Left
            new Vector3(0f, 0.5f), // Right
            new Vector3(-1f, 0f), // top

            new Vector3(1f, -0.5f),  // bottom-right +1
            new Vector3(-1f, 0.5f), // top-left +1
            new Vector3(1f, 0.5f),  // top-right +1
            new Vector3(-1f, -0.5f),  // bottom-left +1
        };

        Vector2[] offsets;
        if (revealType == RevealType.Cross)
        {
            offsets = crossOffsets;
        }
        else if (revealType == RevealType.Square)
        {
            offsets = squareOffsets;
        }
        else if (revealType == RevealType.Star)
        {
            offsets = starOffsets;
        }
        else
            return adjacentTiles; // Type None

        foreach (var offset in offsets)
        {
            Vector3 neighborCoord = coord + offset;

            foreach (var tileEntry in _tileDictionary)
            {
                Vector2 tilePosition = new Vector2(tileEntry.Key.x, tileEntry.Key.y);
                if (Vector2.Distance(tilePosition, neighborCoord) < 0.1f)
                {
                    adjacentTiles.Add(tileEntry.Value);
                }
            }
        }

        return adjacentTiles;
    }
}

[Serializable]
public enum GridType
{
    Rectangle,
    Isometric,
    HexagonPointy,
    HexagonFlat
}
