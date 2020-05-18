using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Visualizator : MonoBehaviour
{
    #region INIT

    private protected float bufferAreaSize;
    private float _maximumScale;
    private protected int segments;
    private protected GameObject lineRendererPrefab;
    private protected Material material;

    private protected AudioVisualizationController audioVisualizationController;
    #endregion
    
    private protected float[] lineScales;
    private protected LineRenderer[] lines;

    public virtual void Initialize(VisualizationSettings settings)
    {
        audioVisualizationController = FindObjectOfType<AudioVisualizationController>();
        bufferAreaSize = settings.bufferAreaSize;
        _maximumScale = settings.maximumScale;
        segments = settings.segments;
        lineRendererPrefab = settings.lineRendererPrefab;
        material = settings.material;
    }

    public virtual void UpdateVisualization()
    {
        lineScales = audioVisualizationController.LineScales;
    }

    private protected void UpdateColors(LineRenderer[] l)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            l[i].startColor = VisualizationDisplay.GetColor(0);
            l[i].endColor =  VisualizationDisplay.GetColor((lineScales[i] - 1) / (_maximumScale - 1f));
        }
    }

    private protected void UpdateCameraPos(Vector2 offset)
    {
        Camera cam = Camera.main;
        Vector3 newCameraPos = cam.transform.position;
        newCameraPos.x += segments/2;
        newCameraPos.y += offset.y;
        cam.transform.position = newCameraPos;
    }
}