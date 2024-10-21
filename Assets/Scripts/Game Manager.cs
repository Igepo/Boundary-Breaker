using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using UnityEngine.VFX;
using static GridTileBase;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject _castlePrefab;
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private GameObject _endGameUI;
    [SerializeField] private TextMeshProUGUI _endText;

    [SerializeField] private GameObject forestPrefab;
    [SerializeField] private GameObject mountainPrefab;

    private GameObject _instantiatedFloatingObject;
    private GameObject _prefabToInstantiate;
    private Camera _mainCamera;
    private bool _isCastlePlace = false;
    private bool _isPlacable;

    [SerializeField] private int _playerWood = 100;
    [SerializeField] private int _playerStone = 50;
    [SerializeField] private int _playerVillager = 0;


    [SerializeField] private Color _isPlacableColor;
    [SerializeField] private Color _isNotPlacableColor;

    [SerializeField] public VisualEffect vfxPrefab;

    private BuildingClick _selectedBuilding;
    private (int woodCost, int stoneCost, int villagerCost) _buildingCosts;

    private List<GridTileBase> _placedBuildings = new List<GridTileBase>();

    private int _totalTiles;
    private int _revealedTiles = 0;
    private bool _shouldMirrorNextBuilding = false;
    private TutorialManager tutorialManager;

    public static event Action<GameManager> OnTurnEnded;

    private void Awake()
    {
        tutorialManager = FindObjectOfType<TutorialManager>();
    }

    private void OnEnable()
    {
        GridTileBase.OnTileClicked += HandleTileClicked;
        BuildingClick.OnBuildingClicked += HandleBuildingClick;
        GridTileBase.OnTileHover += HandleTileHover;
        TurnManager.OnGameOver += OnGameOver;
        GridManager.OnTilesInitialized += InitializeTotalTiles;
    }

    private void OnDisable()
    {
        GridTileBase.OnTileClicked -= HandleTileClicked;
        BuildingClick.OnBuildingClicked -= HandleBuildingClick;
        GridTileBase.OnTileHover -= HandleTileHover;
        TurnManager.OnGameOver -= OnGameOver;
        GridManager.OnTilesInitialized -= InitializeTotalTiles;
    }

    public (int playerWood, int playerStone, int playerVillager) GetCurrentRessources()
    {
        return (_playerWood, _playerStone, _playerVillager);
    }

    private void UpdateResourceUI()
    {
        var resourceUIManager = FindObjectOfType<ResourceUIManager>();
        if (resourceUIManager != null)
        {
            resourceUIManager.SetWood(_playerWood);
            resourceUIManager.SetStone(_playerStone);
            resourceUIManager.SetVillager(_playerVillager);
        }
    }
    private void InitializeTotalTiles()
    {
        _totalTiles = _gridManager.GetTotalTiles();
        UpdateResourceUI();
    }

    private List<GridTileBase> adjacentTilesPreview = new List<GridTileBase>();
    private Dictionary<GridTileBase, Coroutine> activeCoroutines = new Dictionary<GridTileBase, Coroutine>();
    private void StartColorAnimation(GridTileBase tile, Color initialColor, Color targetColor, float speed)
    {
        if (activeCoroutines.ContainsKey(tile))
        {
            StopCoroutine(activeCoroutines[tile]);
            activeCoroutines.Remove(tile);
        }

        Coroutine colorCoroutine = StartCoroutine(AnimateTileColorPingPong(tile, initialColor, targetColor, speed));
        activeCoroutines.Add(tile, colorCoroutine);
    }
    private void StopAllColorAnimations()
    {
        foreach (var kvp in activeCoroutines)
        {
            StopCoroutine(kvp.Value);
        }

        activeCoroutines.Clear();
    }

    private void RestoreInitialTileColors()
    {
        StopAllColorAnimations();
        foreach (var adjacentTile in adjacentTilesPreview)
        {
            if (adjacentTile != null)
            {
                var adjacentTileGridTileBase = adjacentTile.GetComponent<GridTileBase>();
                if (!adjacentTile.IsReveal)
                {
                    adjacentTileGridTileBase.spriteRenderer.color = Color.black;
                }
            }
        }
    }

    private void HandleTileHover(GridTileBase tile)
    {
        if (_instantiatedFloatingObject != null)
        {
            var newTileSpriteRenderer = _instantiatedFloatingObject.GetComponentInChildren<SpriteRenderer>();

            var floatingObjectGridTileBase = _instantiatedFloatingObject.GetComponentInChildren<GridTileBase>();

            var newTileGridTileBaseRevealType = floatingObjectGridTileBase.TileRevealType;
            if (floatingObjectGridTileBase != null)
            {
                RestoreInitialTileColors();

                var coord = new Vector2(tile.transform.position.x, tile.transform.position.y);
                var gridManager = FindObjectOfType<GridManager>();
                adjacentTilesPreview = gridManager.GetAdjacentTiles(coord, newTileGridTileBaseRevealType);

                foreach (var adjacentTile in adjacentTilesPreview)
                {
                    if (adjacentTile != null)
                    {
                        var adjacentTileGridTileBase = adjacentTile.GetComponent<GridTileBase>();
                        if (!adjacentTile.IsReveal)
                        {
                            StartColorAnimation(adjacentTileGridTileBase, adjacentTileGridTileBase.spriteRenderer.color, Color.gray, 1f);
                        }
                    }
                }
            }

            if (_isCastlePlace)
            {
                _isPlacable = tile.IsReveal && IsBuildingAllowedOnTile(tile.TileTypeGetter, floatingObjectGridTileBase.BuildingTypeGetter);

                Color colorPlacement = _isPlacable ? _isPlacableColor : _isNotPlacableColor;
                newTileSpriteRenderer.color = colorPlacement;
            }
        }
    }

    private IEnumerator AnimateTileColorPingPong(GridTileBase gridTileBase, Color baseColor, Color targetColor, float speed)
    {
        while (true)
        {
            float lerpFactor = Mathf.PingPong(Time.time * speed, 1f);

            if (gridTileBase.spriteRenderer != null)
                gridTileBase.spriteRenderer.color = Color.Lerp(baseColor, targetColor, lerpFactor);

            yield return null;
        }
    }
    private bool IsBuildingAllowedOnTile(TileType tileType, BuildingType buildingType)
    {
        switch (buildingType)
        {
            case BuildingType.House:
                return tileType == TileType.Plain;
            case BuildingType.Sawmill:
                return tileType == TileType.Forest;
            case BuildingType.Mine:
                return tileType == TileType.Mountain;
            default:
                return true;
        }
    }
    private void HandleBuildingClick(BuildingClick building)
    {
        if (_isCastlePlace)
        {
            PrepareBuildingConstruction(building);
        }
    }

    private void PrepareBuildingConstruction(BuildingClick building)
    {
        if (_instantiatedFloatingObject != null)
        {
            Destroy(_instantiatedFloatingObject);
            _instantiatedFloatingObject = null;
        }

        var buildingGridTileBase = building.BuildingPrefab.GetComponentInChildren<GridTileBase>();
        if (buildingGridTileBase != null)
        {
            _buildingCosts = buildingGridTileBase.GetCosts();
            int woodCost = _buildingCosts.woodCost;
            int stoneCost = _buildingCosts.stoneCost;
            int villagerCost = _buildingCosts.villagerCost;

            if (HasEnoughResources(woodCost, stoneCost, villagerCost))
            {
                InstantiateBuildingPrefab(building);
            }
            else
            {
                SoundManager.Instance.PlaySound("PickBuildingImpossible");
            }
        }
        else
        {
            Debug.LogError("Le prefab du bâtiment ne contient pas de GridTileBase.");
        }
    }
    private bool HasEnoughResources(int woodCost, int stoneCost, int villagerCost)
    {
        return _playerWood >= woodCost && _playerStone >= stoneCost && _playerVillager >= villagerCost;
    }
    private void InstantiateBuildingPrefab(BuildingClick building)
    {
        _selectedBuilding = building;
        PolygonCollider2D prefabCollider = building.BuildingPrefab.GetComponentInChildren<PolygonCollider2D>();
        if (prefabCollider != null)
        {
            prefabCollider.enabled = false;
        }

        if (building.BuildingPrefab != null)
        {
            _instantiatedFloatingObject = Instantiate(building.BuildingPrefab);
            _prefabToInstantiate = _instantiatedFloatingObject;
        }
        SoundManager.Instance.PlaySound("PickBuilding");
    }

    // Quand on clique sur une tuile
    private void HandleTileClicked(GridTileBase tile)
    {
        StopAllColorAnimations();

        var canPlay = _gridManager.canPlay;
        if (!canPlay)
            return;

        if (_instantiatedFloatingObject == null)
            return;

        if (_isCastlePlace && !_isPlacable)
            return;

        PrepareFloatingObjectForPlacement();
        GameObject newTile = PlaceBuildingOnTile(tile);

        DeductPlayerResources();
        GenerateResourcesOnPlacement(newTile);
        ReplaceTile(tile, newTile);
        PlayVFX(tile.transform.position);

        if (!_isCastlePlace)
        {
            tutorialManager.StepUp();
            PlaceCastleBeginHelp(tile);
            _isCastlePlace = true;
        }
        //TutorialManager(newTile);

        _shouldMirrorNextBuilding = !_shouldMirrorNextBuilding;
    }

    private bool isTurnSkip;
    // Gestion du tutoriel
    private void TutorialManager(GameObject newTile = null)
    {
        var tile = newTile.GetComponentInChildren<GridTileBase>();
        Debug.Log("tutorialManager.step : " + tutorialManager.step);
        Debug.Log("tile.BuildingTypeGetter : " + tile.BuildingTypeGetter);

        if (tile.BuildingTypeGetter == BuildingType.Mine && tutorialManager.step == 1)
        {
            tutorialManager.StepUp();
        }
        if (tile.BuildingTypeGetter == BuildingType.Sawmill && tutorialManager.step == 2)
        {
            tutorialManager.StepUp();
        }
        if (tutorialManager.step == 3 && isTurnSkip) // Quand on passe au jour suivant
        {
            tutorialManager.StepUp();
        }
        if (tile.BuildingTypeGetter == BuildingType.House && tutorialManager.step == 4)
        {
            tutorialManager.StepUp();
        }
        if (tile.BuildingTypeGetter == BuildingType.WatchTower && tutorialManager.step == 5)
        {
            tutorialManager.StepUp(); // Tuto fini
        }
    }
    private void PlaceCastleBeginHelp(GridTileBase tile)
    {
        _gridManager.IsCastlePlaced = true;

        var adjacentTiles = _gridManager.GetAdjacentTilesWithoutCenter(tile.transform.position, RevealType.Square);

        int plainTileCount = 0;
        foreach (var adjacentTile in adjacentTiles)
        {
            if (adjacentTile.TileTypeGetter == TileType.Plain)
            {
                plainTileCount++;
            }
        }

        if (plainTileCount >= 2)
        {
            var plainTiles = adjacentTiles.Where(t => t.TileTypeGetter == TileType.Plain).ToList();
            if (plainTiles.Count >= 2)
            {
                int randomIndex1 = Random.Range(0, plainTiles.Count);
                int randomIndex2 = (randomIndex1 + 1) % plainTiles.Count;

                plainTiles[randomIndex1].SetTileType(TileType.Mountain, mountainPrefab);
                plainTiles[randomIndex2].SetTileType(TileType.Forest, forestPrefab);
            }
        }
    }
    private void PrepareFloatingObjectForPlacement()
    {
        var spriteRenderer = _instantiatedFloatingObject.GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }
    private void GenerateResourcesOnPlacement(GameObject newTile)
    {
        var tile = newTile.GetComponentInChildren<GridTileBase>();
        if (tile != null)
        {
            if (tile.BuildingTypeGetter == BuildingType.House)
            {
                _playerVillager += tile.GetVillagerProduction();
                UpdateResourceUI();
            }
        }
    }
    private GameObject PlaceBuildingOnTile(GridTileBase tile)
    {
        GameObject newTile = Instantiate(_prefabToInstantiate, tile.transform.position, Quaternion.identity, tile.transform.parent);

        // On reattribue la position initial de la nouvelle tuile en récupérant l'ancienne
        GridTileBase newTileGridTileBase = newTile.GetComponentInChildren<GridTileBase>();
        newTileGridTileBase.InitialTilePosition = tile.InitialTilePosition;

        if (newTileGridTileBase != null)
        {
            _placedBuildings.Add(newTileGridTileBase);
        }

        if (_shouldMirrorNextBuilding) // 50% de chance
        {
            Vector3 scale = newTile.transform.localScale;
            scale.x *= -1;
            newTile.transform.localScale = scale;
        }
        return newTile;
    }

    private void DeductPlayerResources()
    {
        if (_selectedBuilding == null)
            return;

        var buildingGridTileBase = _selectedBuilding.BuildingPrefab.GetComponentInChildren<GridTileBase>();
        if (buildingGridTileBase != null)
        {
            var costs = buildingGridTileBase.GetCosts();
            _playerWood -= costs.woodCost;
            _playerStone -= costs.stoneCost;
            _playerVillager -= costs.villagerCost;
            UpdateResourceUI();
        }
    }
    private void ReplaceTile(GridTileBase oldTile, GameObject newTile)
    {
        var oldTileSortingGroup = oldTile.GetComponent<SortingGroup>();
        if (oldTileSortingGroup != null)
        {
            var oldTileSortingOrder = oldTileSortingGroup.sortingOrder;
            var sortingGroup = newTile.GetComponent<SortingGroup>();
            if (sortingGroup == null)
            {
                sortingGroup = newTile.gameObject.AddComponent<SortingGroup>();
            }
            sortingGroup.sortingOrder = oldTileSortingOrder;
        }

        Destroy(oldTile.gameObject);
        Destroy(_instantiatedFloatingObject);
        _instantiatedFloatingObject = null;

        PolygonCollider2D newTileCollider = newTile.GetComponentInChildren<PolygonCollider2D>();
        if (newTileCollider != null)
        {
            newTileCollider.enabled = true;
        }

        SoundManager.Instance.PlaySound("BuildingPlacement");

        TileRevealed(oldTile, newTile);
    }

    void TileRevealed(GridTileBase tile, GameObject newTile)
    {
        GridTileBase newTileGridTileBase = newTile.GetComponentInChildren<GridTileBase>();
        var newTileGridTileBaseRevealType = newTileGridTileBase.TileRevealType;
        if (newTileGridTileBase != null)
        {
            var coord = new Vector2(tile.transform.position.x, tile.transform.position.y);
            var gridManager = FindObjectOfType<GridManager>();
            var adjacentTiles = gridManager.GetAdjacentTiles(coord, newTileGridTileBaseRevealType);

            foreach (var adjacentTile in adjacentTiles)
            {
                if (!adjacentTile.IsReveal)
                {
                    adjacentTile.SetReveal(true);
                    _revealedTiles++;
                }
            }
        }
        CheckForGameEnd();
    }
    private void CheckForGameEnd()
    {
        if (_revealedTiles >= _totalTiles)
        {
            EndGame(true);
        }
    }
    private void EndGame(bool isVictory)
    {
        if (isVictory)
        {
            SoundManager.Instance.PlaySound("Victory");
            _endText.text = "Congratulations !";
        }
        else
        {
            SoundManager.Instance.PlaySound("Defeat");
            _endText.text = "There are no days left...";
        }

        _endGameUI.SetActive(true);

        MusicManager.Instance.StopMusic();
    }
    void Start()
    {
        _mainCamera = Camera.main;
        _endGameUI.SetActive(false);
        OnBegin();
        UpdateResourceUI();
    }

    void OnBegin()
    {
        PolygonCollider2D prefabCollider = _castlePrefab.GetComponentInChildren<PolygonCollider2D>();
        _prefabToInstantiate = _castlePrefab;

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
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    EndTurn();
        //}

        if (Input.GetMouseButtonDown(1) && _isCastlePlace)
        {
            CancelBuildingSelection();
        }

        Vector3 mousePosition = Input.mousePosition;
        Vector3 worldPosition = _mainCamera.ScreenToWorldPoint(mousePosition);
        worldPosition.z = 0;
        if (_instantiatedFloatingObject != null)
            _instantiatedFloatingObject.transform.position = worldPosition;
    }

    private void CancelBuildingSelection()
    {
        RestoreInitialTileColors();
        if (_instantiatedFloatingObject != null)
        {
            Destroy(_instantiatedFloatingObject);
            _instantiatedFloatingObject = null;
            _selectedBuilding = null;
            _prefabToInstantiate = null;
        }
    }

    private void OnGameOver(TurnManager turn)
    {
        EndGame(false);
    }
    public void EndTurn()
    {
        if (_isCastlePlace) // On vérifie si le chateau a été placé
        {
            CalculateResourceProduction();
            OnTurnEnded?.Invoke(this);
            SoundManager.Instance.PlaySound("EndTurn");
            //TutorialManager();
            isTurnSkip = true;
            //TurnManager.Instance.EndTurn();
        }
    }
    private void CalculateResourceProduction()
    {
        int totalWoodProduction = 0;
        int totalStoneProduction = 0;

        foreach (var building in _placedBuildings)
        {
            totalWoodProduction += building.GetWoodProduction();
            totalStoneProduction += building.GetStoneProduction();
        }

        _playerWood += totalWoodProduction;
        _playerStone += totalStoneProduction;

        UpdateResourceUI();
    }

    public void PlayVFX(Vector3 position)
    {
        VisualEffect vfxInstance = Instantiate(vfxPrefab, position, Quaternion.identity);

        Destroy(vfxInstance.gameObject, 5f);
    }
}
