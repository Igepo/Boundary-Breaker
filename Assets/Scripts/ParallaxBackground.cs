using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    public Transform[] backgroundLayers; // Les 5 images (ex: nuages, collines, arbres, etc.)
    public float[] parallaxFactors;      // Facteur de parallax pour chaque image
    public float smoothing = 1f;         // Le lissage du mouvement pour un effet plus fluide

    private Vector3 previousCamPos;      // Position de la cam�ra au dernier frame
    private Transform camTransform;      // R�f�rence � la cam�ra

    private float[] layerWidths;

    void Start()
    {
        camTransform = Camera.main.transform;
        previousCamPos = camTransform.position;

        // Calculer la largeur de chaque couche pour savoir quand elle doit �tre repositionn�e
        layerWidths = new float[backgroundLayers.Length];
        for (int i = 0; i < backgroundLayers.Length; i++)
        {
            SpriteRenderer sr = backgroundLayers[i].GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                layerWidths[i] = sr.bounds.size.x; // R�cup�re la largeur de l'image
            }
        }
    }

    void Update()
    {
        for (int i = 0; i < backgroundLayers.Length; i++)
        {
            float parallax = (previousCamPos.x - camTransform.position.x) * parallaxFactors[i];
            float backgroundTargetPosX = backgroundLayers[i].position.x + parallax;
            Vector3 backgroundTargetPos = new Vector3(backgroundTargetPosX, backgroundLayers[i].position.y, backgroundLayers[i].position.z);
            backgroundLayers[i].position = Vector3.Lerp(backgroundLayers[i].position, backgroundTargetPos, smoothing * Time.deltaTime);

            // R�p�ter l'image si elle sort de l'�cran
            if (camTransform.position.x - backgroundLayers[i].position.x > layerWidths[i] / 2)
            {
                backgroundLayers[i].position = new Vector3(backgroundLayers[i].position.x + layerWidths[i], backgroundLayers[i].position.y, backgroundLayers[i].position.z);
            }
            else if (camTransform.position.x - backgroundLayers[i].position.x < -layerWidths[i] / 2)
            {
                backgroundLayers[i].position = new Vector3(backgroundLayers[i].position.x - layerWidths[i], backgroundLayers[i].position.y, backgroundLayers[i].position.z);
            }
        }

        previousCamPos = camTransform.position;
    }
}
