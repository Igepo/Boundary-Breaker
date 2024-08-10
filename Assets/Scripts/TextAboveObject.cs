using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class TextAboveObject : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textMeshPro;

    [TextArea(3, 10)]
    [SerializeField] private string _textInput;

    private Vector3 currentVelocity;
    private bool isMouseOver = false;
    void Start()
    {
        _textMeshPro.enabled = false;
        _textMeshPro.text = _textInput;
        _textMeshPro.transform.localScale = Vector3.zero;
    }

    void Update()
    {
        if (isMouseOver)
        {
            _textMeshPro.transform.localScale = Vector3.SmoothDamp(_textMeshPro.transform.localScale, Vector3.one, ref currentVelocity, 0.1f);
        }
        else
        {
            _textMeshPro.transform.localScale = Vector3.SmoothDamp(_textMeshPro.transform.localScale, Vector3.zero, ref currentVelocity, 0.1f);

            if (_textMeshPro.transform.localScale.x <= 0.01f)
            {
                _textMeshPro.enabled = false;
            }
        }
    }

    private void OnMouseEnter()
    {
        isMouseOver = true;
        _textMeshPro.enabled = true;
    }

    private void OnMouseExit()
    {
        isMouseOver = false;
    }
}
