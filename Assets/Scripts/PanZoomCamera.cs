using UnityEngine;
using UnityEngine.EventSystems;

public class PanZoomCamera : MonoBehaviour
{
    private Vector3 mouseWorldPosStart;
    private float zoomScale = 5.0f;
    private float zoomMin = 0.5f;
    private float zoomMax = 6.0f;

    void Start()
    {
    }

    void Update()
    {
        if ((Input.GetMouseButtonDown(2) || Input.GetMouseButtonDown(1)) && !Input.GetKey(KeyCode.LeftShift) && !EventSystem.current.IsPointerOverGameObject()) //Check for Pan
        {
            mouseWorldPosStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        if ((Input.GetMouseButton(2) || Input.GetMouseButton(1)) && !Input.GetKey(KeyCode.LeftShift) && !EventSystem.current.IsPointerOverGameObject())
        {
            Pan();
        }
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            Zoom(Input.GetAxis("Mouse ScrollWheel")); //Check for Zoom
        }
    }

    void Pan()
    {
        if (Input.GetAxis("Mouse Y") != 0 || Input.GetAxis("Mouse X") != 0)
        {
            Vector3 mouseWorldPosDiff = mouseWorldPosStart - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Camera.main.transform.position += mouseWorldPosDiff;
        }
    }

    void Zoom(float zoomDiff)
    {
        if (zoomDiff != 0)
        {
            mouseWorldPosStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - zoomDiff * zoomScale, zoomMin, zoomMax);
            Vector3 mouseWorldPosDiff = mouseWorldPosStart - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Camera.main.transform.position += mouseWorldPosDiff;
        }
    }

}
