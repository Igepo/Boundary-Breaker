using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTileBase : MonoBehaviour
{
    [SerializeField] private GameObject _highlight;
    [SerializeField] private Sprite _HiddenSprite;

    [SerializeField] private BuildingType _buildingType;
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
        Square
    }

    public enum TileType
    {
        Forest,
        Mountain,
        Plain,
        HexagonFlat
    }
    private TileType _tileType;


    private Color _revealedColor = Color.white;
    private Color _hiddenColor = Color.black;

    private SpriteRenderer _spriteRenderer;
    private bool _isReveal;

    private Sprite _initialSprite;

    public static event Action<GridTileBase> OnTileRevealed;
    public static event Action<GridTileBase> OnTileClicked;
    public void Init(Vector3 coordinate, TileType tileType)
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _initialSprite = _spriteRenderer.sprite;

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

            // Déclenche l'événement lorsqu'une case est révélée
            if (_isReveal)
            {
                OnTileRevealed?.Invoke(this);
            }
        }
    }

    private void UpdateAppearance()
    {
        if (_spriteRenderer == null) return;

        if (!_isReveal)
        {
            _spriteRenderer.color = _hiddenColor;
            _spriteRenderer.sprite = _HiddenSprite;
        }
        else
        {
            _spriteRenderer.color = _revealedColor;
            _spriteRenderer.sprite = _initialSprite;
        }
        _spriteRenderer.color = _isReveal ? _revealedColor : _hiddenColor;
    }

    void OnMouseEnter()
    {
        _highlight.SetActive(true);
    }

    void OnMouseExit()
    {
        _highlight.SetActive(false);
    }

    /// <summary>
    /// Pour le debug
    /// </summary>
    private void OnMouseDown()
    {
        //SetReveal(true);
        OnTileClicked?.Invoke(this);
    }
}
