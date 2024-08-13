using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using static GridTileBase;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject _castlePrefab;
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private GameObject endGameUI;

    private GameObject _instantiatedFloatingObject;
    private GameObject _prefabToInstantiate;
    private Camera _mainCamera;
    private bool isCastlePlace = false;
    private bool isPlacable;

    [SerializeField] private int playerWood = 100; // Exemples de ressources initiales
    [SerializeField] private int playerStone = 50;
    [SerializeField] private int playerVillager = 0;

    private BuildingClick _selectedBuilding;
    private (int woodCost, int stoneCost, int villagerCost) _buildingCosts;

    private List<GridTileBase> placedBuildings = new List<GridTileBase>();

    private int _totalTiles;
    private int _revealedTiles = 0;
    
    private void OnEnable()
    {
        // S'abonner à l'événement de révélation des tuiles
        GridTileBase.OnTileClicked += HandleTileClicked;
        BuildingClick.OnBuildingClicked += HandleBuildingClick;
        GridTileBase.OnTileHover += HandleTileHover;
        GridManager.OnTilesInitialized += InitializeTotalTiles;

        StartCoroutine(SubscribeToTurnManagerEvent());
    }

    private void OnDisable()
    {
        // Se désabonner de l'événement lorsqu'il n'est plus nécessaire
        GridTileBase.OnTileClicked -= HandleTileClicked;
        BuildingClick.OnBuildingClicked -= HandleBuildingClick;
        GridTileBase.OnTileHover -= HandleTileHover;
        TurnManager.Instance.OnTurnStarted -= OnTurnStarted;
        GridManager.OnTilesInitialized -= InitializeTotalTiles;
    }
    private IEnumerator SubscribeToTurnManagerEvent()
    {
        while (TurnManager.Instance == null)
        {
            yield return null; // Attendre un frame avant de réessayer
        }

        TurnManager.Instance.OnTurnStarted += OnTurnStarted;
    }
    private void UpdateResourceUI()
    {
        // Assurez-vous que ResourceUIManager est correctement assigné
        var resourceUIManager = FindObjectOfType<ResourceUIManager>();
        if (resourceUIManager != null)
        {
            resourceUIManager.SetWood(playerWood);
            resourceUIManager.SetStone(playerStone);
            resourceUIManager.SetVillager(playerVillager);
        }
    }
    private void InitializeTotalTiles()
    {
        _totalTiles = _gridManager.GetTotalTiles();
        UpdateResourceUI();
    }

    private void HandleTileHover(GridTileBase tile)
    {
        if (_instantiatedFloatingObject != null)
        {
            Vector3 mousePosition = Input.mousePosition;
            Vector3 worldPosition = _mainCamera.ScreenToWorldPoint(mousePosition);
            worldPosition.z = 0;
            _instantiatedFloatingObject.transform.position = worldPosition;

            var newTileSpriteRenderer = _instantiatedFloatingObject.GetComponentInChildren<SpriteRenderer>();

            var floatingObjectGridTileBase = _instantiatedFloatingObject.GetComponentInChildren<GridTileBase>();
            if (isCastlePlace) // Pas de gestion de couleur pour le château
            {
                // Vérifier si le bâtiment peut être placé sur cette tuile
                isPlacable = tile.IsReveal && IsBuildingAllowedOnTile(tile.TileTypeGetter, floatingObjectGridTileBase.BuildingTypeGetter);

                // Mettre à jour la couleur du bâtiment en fonction de la possibilité de placement
                Color colorPlacement = isPlacable ? Color.green : Color.red;
                newTileSpriteRenderer.color = colorPlacement;
            }
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
            // Ajoutez d'autres cas pour d'autres types de bâtiments si nécessaire
            default:
                return true;
        }
    }
    private void HandleBuildingClick(BuildingClick building)
    {
        if (isCastlePlace)
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
                Debug.Log("Pas assez de ressources pour construire ce bâtiment.");
            }
        }
        else
        {
            Debug.LogError("Le prefab du bâtiment ne contient pas de GridTileBase.");
        }
    }
    private bool HasEnoughResources(int woodCost, int stoneCost, int villagerCost)
    {
        return playerWood >= woodCost && playerStone >= stoneCost && playerVillager >= villagerCost;
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
    }

    private void HandleTileClicked(GridTileBase tile)
    {
        if (_instantiatedFloatingObject == null)
            return;

        if (isCastlePlace && !isPlacable)
            return;

        PrepareFloatingObjectForPlacement();
        GameObject newTile = PlaceBuildingOnTile(tile);

        DeductPlayerResources();
        GenerateResourcesOnPlacement(newTile);
        ReplaceTile(tile, newTile);

        if (!isCastlePlace)
        {
            isCastlePlace = true;
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
                playerVillager += tile.GetVillagerProduction();
                UpdateResourceUI();
            }
        }
    }
    private GameObject PlaceBuildingOnTile(GridTileBase tile)
    {
        GameObject newTile = Instantiate(_prefabToInstantiate, tile.transform.position, Quaternion.identity, tile.transform.parent);

        // Ajouter le nouveau bâtiment à la liste des bâtiments placés
        GridTileBase newTileGridTileBase = newTile.GetComponentInChildren<GridTileBase>();
        if (newTileGridTileBase != null)
        {
            placedBuildings.Add(newTileGridTileBase);
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
            playerWood -= costs.woodCost;
            playerStone -= costs.stoneCost;
            playerVillager -= costs.villagerCost;
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
            newTileCollider.enabled = true; // On réactive le collider
        }

        TileRevealed(oldTile, newTile);
    }

    void TileRevealed(GridTileBase tile, GameObject newTile)
    {
        GridTileBase newTileGridTileBase = newTile.GetComponentInChildren<GridTileBase>();
        var newTileGridTileBaseRevealType = newTileGridTileBase.TileRevealType;

        // On revele les tuiles
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
                    _revealedTiles++; // Incrémenter pour chaque tuile révélée
                }
            }
        }
        CheckForGameEnd();
    }
    private void CheckForGameEnd()
    {
        if (_revealedTiles >= _totalTiles)
        {
            EndGame();
        }
    }
    private void EndGame()
    {
        // Afficher un message de victoire
        Debug.Log("Félicitations ! Toutes les tuiles sont révélées. Le jeu est terminé.");

        // Vous pourriez désactiver les entrées du joueur, afficher un écran de victoire, etc.
        // Exemple: activer un écran de fin de jeu
        endGameUI.SetActive(true);

        // Optionnel: Arrêter le jeu
        Time.timeScale = 0;
    }
    void Start()
    {
        _mainCamera = Camera.main;
        endGameUI.SetActive(false);
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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EndTurn();
        }

        if (Input.GetMouseButtonDown(1) && isCastlePlace)
        {
            CancelBuildingSelection();
        }
    }

    private void CancelBuildingSelection()
    {
        // Si un objet est actuellement sélectionné
        if (_instantiatedFloatingObject != null)
        {
            Destroy(_instantiatedFloatingObject); // Détruire l'objet flottant
            _instantiatedFloatingObject = null; // Réinitialiser la variable
            _selectedBuilding = null; // Réinitialiser la sélection du bâtiment
            _prefabToInstantiate = null; // Réinitialiser le prefab à instancier

            Debug.Log("Sélection du bâtiment annulée.");
        }
    }

    private void OnTurnStarted()
    {
        // Réinitialiser l'état des objets, ou mettre à jour des éléments en début de tour
        Debug.Log("Un nouveau tour a commencé.");
    }

    public void EndTurn()
    {
        CalculateResourceProduction();
        TurnManager.Instance.EndTurn();
    }
    private void CalculateResourceProduction()
    {
        int totalWoodProduction = 0;
        int totalStoneProduction = 0;

        // Parcourir tous les bâtiments placés et calculer la production
        foreach (var building in placedBuildings)
        {
            totalWoodProduction += building.GetWoodProduction();
            totalStoneProduction += building.GetStoneProduction();
        }

        // Ajouter la production totale aux ressources du joueur
        playerWood += totalWoodProduction;
        playerStone += totalStoneProduction;

        // Mettre à jour l'interface utilisateur des ressources
        UpdateResourceUI();

        Debug.Log($"Production de fin de tour: +{totalWoodProduction} bois, +{totalStoneProduction} pierre");
    }

    public int GetPlayerWood()
    {
        return playerWood;
    }

    public int GetPlayerStone()
    {
        return playerStone;
    }
}
