using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class GridTileBase : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    [SerializeField] private List<Sprite> spriteList = new List<Sprite>();

    [SerializeField] private GameObject _highlight;
    [SerializeField] private Sprite _hiddenSprite;

    [SerializeField] private int _woodProduction = 0;
    [SerializeField] private int _stoneProduction = 0;
    [SerializeField] private int _villagerProduction = 0;

    [SerializeField] private int _woodCost = 0;
    [SerializeField] private int _stoneCost = 0;
    [SerializeField] private int _villagerCost = 0;
    
    [SerializeField] private BuildingType _buildingType;
    [SerializeField] private float _pressedTileDistance = 0.03f;
    [SerializeField] private Vector3 _initialTilePosition;

    public Vector3 InitialTilePosition
    {
        get => _initialTilePosition; set => _initialTilePosition = value;
    }
    public BuildingType BuildingTypeGetter => _buildingType;
    public enum BuildingType
    {
        none,
        House,
        WatchTower,
        Sawmill,
        Mine
    }

    [SerializeField] private RevealType _tileRevealType;
    public RevealType TileRevealType
    {
        get => _tileRevealType;
        private set => _tileRevealType = value;
    }
    public enum RevealType
    {
        None,
        Cross,
        Square,
        Star
    }

    public enum TileType
    {
        Forest,
        Mountain,
        Plain,
        HexagonFlat
    }
    private TileType _tileType;
    public TileType TileTypeGetter => _tileType;

    public void SetTileType(TileType newTileType, GameObject newPrefab)
    {
        _tileType = newTileType;
        //GetComponent<SpriteRenderer>().sprite = newPrefab.GetComponent<SpriteRenderer>().sprite;

        Transform childTransform = newPrefab.transform.Find("Sprite");
        if (childTransform != null)
        {
            var component = childTransform.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = component.sprite;
        }
    }

    private Color _revealedColor = Color.white;
    private Color _hiddenColor = Color.black;

    [SerializeField] Animator animator;

    private bool _isReveal;
    public bool IsReveal => _isReveal;

    [SerializeField] private Sprite _initialSprite;

    public static event Action<GridTileBase> OnTileRevealed;
    public static event Action<GridTileBase> OnTileClicked;
    public static event Action<GridTileBase> OnTileHover;
    private void Start()
    {
        //if (spriteList?.Count > 0)
        //    spriteRenderer.sprite = spriteList[Random.Range(0, spriteList.Count)];
    }

    public void Init(Vector3 coordinate, TileType tileType)
    {
        //_spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteList?.Count > 0)
        {
            _initialSprite = spriteList[Random.Range(0, spriteList.Count)];
        }
        else
            _initialSprite = spriteRenderer.sprite;

        _initialTilePosition = transform.position;

        transform.position = coordinate;
        _tileType = tileType;
        _isReveal = false;
        UpdateAppearance();
    }

    public void SetReveal(bool isReveal)
    {
        if (_isReveal != isReveal)
        {
            _isReveal = isReveal;
            UpdateAppearance();

            if (_isReveal)
            {
                OnTileRevealed?.Invoke(this);
            }

            if (animator != null)
            {
                animator.Play("IsometricDiamondAnim", -1, 0f);
            }
        }
    }

    private void UpdateAppearance()
    {
        if (spriteRenderer == null) return;

        if (!_isReveal)
        {
            spriteRenderer.color = _hiddenColor;
            spriteRenderer.sprite = _hiddenSprite;
        }
        else
        {
            spriteRenderer.color = _revealedColor;
            spriteRenderer.sprite = _initialSprite;
        }
        spriteRenderer.color = _isReveal ? _revealedColor : _hiddenColor;
    }
    public (int woodCost, int stoneCost, int villagerCost) GetCosts()
    {
        return (_woodCost, _stoneCost, _villagerCost);
    }
    public (int woodProduction, int stoneProduction, int villagerProduction) GetProduction()
    {
        return (_woodProduction, _stoneProduction, _villagerProduction);
    }
    public int GetVillagerProduction()
    {
        return _villagerProduction;
    }

    public int GetWoodProduction()
    {
        return _woodProduction;
    }

    public int GetStoneProduction()
    {
        return _stoneProduction;
    }

    void OnMouseEnter()
    {
        _highlight.SetActive(true);
        OnTileHover?.Invoke(this);
    }

    void OnMouseExit()
    {
        _highlight.SetActive(false);
        //spriteRenderer.color = _initialSpriteColor;
    }

    void OnMouseUpAsButton()
    {
        //SetReveal(true);
        transform.position = _initialTilePosition;
        OnTileClicked?.Invoke(this);
    }

    private void OnMouseDown()
    {
        transform.position = new Vector3(_initialTilePosition.x, _initialTilePosition.y - _pressedTileDistance, _initialTilePosition.z);
    }

    private void OnMouseUp()
    {
        transform.position = _initialTilePosition;
    }
}
