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
    
    #endregion

    #region LINE SCALE

    public float sampledPercentage;
    public float lineMultiplier;
    public float smoothingSpeed;
    
    #endregion
    
    #region RING
    
    public float minimumRadius;
    
    #endregion
}

public enum VisualizationMode
{
    RING
}