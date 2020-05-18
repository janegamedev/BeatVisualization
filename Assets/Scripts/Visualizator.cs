using UnityEngine;

public class Visualizator : MonoBehaviour
{
    #region INIT

    private protected float bufferAreaSize;
    private protected  float maximumScale;
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
        maximumScale = settings.maximumScale;
        segments = settings.segments;
        lineRendererPrefab = settings.lineRendererPrefab;
        material = settings.material;
    }

    public virtual void UpdateVisualization()
    {
        lineScales = audioVisualizationController.LineScales;
    }
}