using UnityEngine;

[System.Serializable]
public struct VisualizationSettings
{
    #region DEFUALT

    public float bufferAreaSize;
    public float maximumScale;
    public int segments;
    public GameObject lineRendererPrefab;
    public Material material;
    public VisualizationMode visualizationMode;
    public FFTWindow window;
    
    #endregion

    #region LINE SCALE

    public float sampledPercentage;
    public float lineMultiplier;
    public float smoothingSpeed;
    
    #endregion
    
    #region RING
    
    [Header("Ring settings: ")]
    public float minimumRadius;
    
    #endregion

    #region LINE

    [Header("Line settings: ")] 
    public Vector2 offset;

    #endregion
}

public enum VisualizationMode
{
    RING,
    LINE,
    DOUBLE_LINE,
    CIRCLE
}