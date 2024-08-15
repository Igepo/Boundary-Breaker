using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera _cam;
    private Vector3 _cameraPositionTarget;
    private float _cameraSizeTarget;
    private Vector3 _moveVel;
    private float _cameraSizeVel;

    [Header("UI Settings")]
    [Tooltip("Height of the bottom UI in pixels.")]
    [SerializeField] private float uiHeightInPixels = 150f; // Variable modifiable pour la hauteur de l'UI en pixels

    private void Awake()
    {
        _cam = Camera.main;
    }

    private void OnEnable()
    {
        _moveVel = Vector3.zero;
        _cameraSizeVel = 0f;
    }

    private void OnDisable()
    {
    }

    private void Start()
    {
        //_cam.transform.position = _cameraPositionTarget;
        //_cam.orthographicSize = _cameraSizeTarget;
    }

    private void LateUpdate()
    {
        if (_cam != null)
        {
            //Debug.Log($"_cam.transform.position: {_cam.transform.position}, _cameraPositionTarget: {_cameraPositionTarget}, _cameraSizeTarget: {_cameraSizeTarget}");
            _cam.transform.position = Vector3.SmoothDamp(_cam.transform.position, _cameraPositionTarget, ref _moveVel, 0.5f);
            _cam.orthographicSize = Mathf.SmoothDamp(_cam.orthographicSize, _cameraSizeTarget, ref _cameraSizeVel, 0.5f);
        }
    }

    public void SetCamera(Bounds bounds)
    {
        if (_cam == null)
        {
            return;
        }
        bounds.Expand(2);

        var vertical = bounds.size.y;

        float uiHeightInWorldUnits = _cam.orthographicSize * (uiHeightInPixels / _cam.pixelHeight);
        var horizontal = bounds.size.x * _cam.pixelHeight / _cam.pixelWidth ;

        _cameraPositionTarget = bounds.center + Vector3.back;

        _cameraSizeTarget = Mathf.Max(horizontal, vertical) * 0.5f;
    }
}
