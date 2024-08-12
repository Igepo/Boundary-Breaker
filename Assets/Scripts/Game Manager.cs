using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
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

    private BuildingClick _selectedBuilding;
    private (int woodCost, int stoneCost) _buildingCosts;

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
    private void InitializeTotalTiles()
    {
        _totalTiles = _gridManager.GetTotalTiles();
        Debug.Log("totalTiles " + _totalTiles);
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

                // Vérifier si le joueur a suffisamment de ressources
                if (playerWood >= woodCost && playerStone >= stoneCost)
                {
                    // Autoriser la construction
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
    }

    private void HandleTileClicked(GridTileBase tile)
    {
        if (_instantiatedFloatingObject != null)
        {
            if (!isCastlePlace || (isCastlePlace && isPlacable))
            {
                var spriteRenderer = _instantiatedFloatingObject.GetComponentInChildren<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.white;
                }

                GameObject newTile = Instantiate(_prefabToInstantiate, tile.transform.position, Quaternion.identity, tile.transform.parent);

                // Ajouter le nouveau bâtiment à la liste des bâtiments placés
                GridTileBase newTileGridTileBase = newTile.GetComponentInChildren<GridTileBase>();
                if (newTileGridTileBase != null)
                {
                    placedBuildings.Add(newTileGridTileBase);
                }

                // Réduire les ressources du joueur
                if (_selectedBuilding != null)
                {
                    var buildingGridTileBase = _selectedBuilding.BuildingPrefab.GetComponentInChildren<GridTileBase>();
                    if (buildingGridTileBase != null)
                    {
                        var costs = buildingGridTileBase.GetCosts();
                        playerWood -= costs.woodCost;
                        playerStone -= costs.stoneCost;
                        UpdateResourceUI();
                    }
                }

                // On remplace la tuile
                Destroy(tile.gameObject);
                Destroy(_instantiatedFloatingObject);
                _instantiatedFloatingObject = null;

                PolygonCollider2D newTileCollider = newTile.GetComponentInChildren<PolygonCollider2D>();
            
                if (newTileCollider != null)
                {
                    newTileCollider.enabled = true; // On reactive le collider
                }

                TileRevealed(tile, newTile);
            }

            if (!isCastlePlace)
            {
                isCastlePlace = true;
            }


        }
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

    private void UpdateResourceUI()
    {
        // Assurez-vous que ResourceUIManager est correctement assigné
        var resourceUIManager = FindObjectOfType<ResourceUIManager>();
        if (resourceUIManager != null)
        {
            resourceUIManager.SetWood(playerWood);
            resourceUIManager.SetStone(playerStone);
        }
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

        Debug.Log("_revealedTiles " + _revealedTiles);
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
        Debug.Log("Start");
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

        if (Input.GetMouseButtonDown(1)) // 1 correspond au clic droit de la souris
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

    // Méthodes pour obtenir les ressources
    public int GetPlayerWood()
    {
        return playerWood;
    }

    public int GetPlayerStone()
    {
        return playerStone;
    }
}
