using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
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
    [SerializeField] private ScriptableGridConfig[] _configs;

    private bool _requiresGeneration = true;
    private Camera _cam;
    private Grid _grid;

    private Vector3 _cameraPositionTarget;
    private float _cameraSizeTarget;
    private Vector3 _moveVel;
    private float _cameraSizeVel;

    private Vector2 _currentGap;
    private Vector2 _gapVel;

    private Bounds bounds = new();
    private bool boundsInitialized = false;

    private Dictionary<Vector3, GridTileBase> _tileDictionary = new Dictionary<Vector3, GridTileBase>();

    public List<GridTileBase> TilesList { get; private set; } = new List<GridTileBase>();
    private void OnEnable()
    {
        // S'abonner à l'événement de révélation des tuiles
        GridTileBase.OnTileRevealed += HandleTileRevealed;
    }

    private void OnDisable()
    {
        // Se désabonner de l'événement lorsqu'il n'est plus nécessaire
        GridTileBase.OnTileRevealed -= HandleTileRevealed;
    }

    private void HandleTileRevealed(GridTileBase tile)
    {
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

        SetCamera(bounds);
    }

    private void Awake()
    {
        _grid = GetComponent<Grid>();
        _cam = Camera.main;
        _currentGap = _gap;
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

        _cam.transform.position = Vector3.SmoothDamp(_cam.transform.position, _cameraPositionTarget, ref _moveVel, 0.5f);
        _cam.orthographicSize = Mathf.SmoothDamp(_cam.orthographicSize, _cameraSizeTarget, ref _cameraSizeVel, 0.5f);
    }

    private void Generate()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        var config = _configs.First(c => c.Type == _gridType);

        _grid.cellLayout = config.Layout;
        _grid.cellSize = config.CellSize;
        //Debug.Log($"cellLayout : {_grid.cellLayout}, cellSize : {_grid.cellSize}");
        if (_grid.cellLayout != GridLayout.CellLayout.Hexagon) _grid.cellGap = _currentGap;
        _grid.cellSwizzle = config.GridSwizzle;

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
        var rand = new Random(420);

        foreach (var coordinate in coordinates.OrderBy(t => rand.Next()).Take(coordinates.Count - skipCount))
        {
            var isForest = forestIndex++ < forestCount;
            var isMountain = !isForest && mountainIndex++ < mountainCount;
            GridTileBase prefab;
            GridTileBase.TileType tileType;

            if (isForest)
            {
                prefab = config.ForestPrefab;
                tileType = GridTileBase.TileType.Forest;
            }
            else if (isMountain)
            {
                prefab = config.MountainPrefab;
                tileType = GridTileBase.TileType.Mountain;
            }
            else
            {
                prefab = config.PlainPrefab;
                tileType = GridTileBase.TileType.Plain;
            }

            var position = _grid.GetCellCenterWorld(coordinate);
            var spawned = Instantiate(prefab, position, Quaternion.identity, transform);
            spawned.Init(position, tileType);
            //spawned.name = $"{position.x}:{position.y}";
            mapBounds.Encapsulate(spawned.GetComponent<Renderer>().bounds);
            _tileDictionary[position] = spawned;
            FogOfWarManager.Instance.RegisterTile(spawned);
        }

        SetCamera(mapBounds);

        _requiresGeneration = false;
    }

    private void SetCamera(Bounds bounds)
    {
        bounds.Expand(2);

        var vertical = bounds.size.y;
        var horizontal = bounds.size.x * _cam.pixelHeight / _cam.pixelWidth;

        _cameraPositionTarget = bounds.center + Vector3.back;
        _cameraSizeTarget = Mathf.Max(horizontal, vertical) * 0.5f;
    }

    public List<GridTileBase> GetAdjacentTiles(Vector2 coord, RevealType revealType)
    {
        List<GridTileBase> adjacentTiles = new List<GridTileBase>();

        Vector2[] crossOffsets = new Vector2[]
        {
            new Vector3(0.5f, -0.25f),  // Right (might correspond to bottom-right in isometric)
            new Vector3(-0.5f, 0.25f), // Left (might correspond to top-left in isometric)
            new Vector3(0.5f, 0.25f),  // Up (might correspond to top-right in isometric)
            new Vector3(-0.5f, -0.25f)  // Down (might correspond to bottom-left in isometric)
        };

        Vector2[] squareOffsets = new Vector2[]
        {
            new Vector3(0.5f, -0.25f),  // Right (might correspond to bottom-right in isometric)
            new Vector3(-0.5f, 0.25f), // Left (might correspond to top-left in isometric)
            new Vector3(0.5f, 0.25f),  // Up (might correspond to top-right in isometric)
            new Vector3(-0.5f, -0.25f),  // Down (might correspond to bottom-left in isometric)

            new Vector3(1f, 0f), // Up-Right
            new Vector3(0f, -0.5f), // Down-Right
            new Vector3(0f, 0.5f), // Up-Left
            new Vector3(-1f, 0f) // Down-Left
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


[CreateAssetMenu]
public class ScriptableGridConfig : ScriptableObject
{
    public GridType Type;
    [Space(10)]
    public GridLayout.CellLayout Layout;
    public GridTileBase PlainPrefab, ForestPrefab, MountainPrefab;
    public Vector3 CellSize;
    public GridLayout.CellSwizzle GridSwizzle;
}

[Serializable]
public enum GridType
{
    Rectangle,
    Isometric,
    HexagonPointy,
    HexagonFlat
}