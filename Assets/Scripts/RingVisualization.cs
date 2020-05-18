using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RingVisualization : Visualizator
{
    #region INIt
    private float _minimumRadius;
    #endregion

    #region CALCS
    private float _currentRadius;
    #endregion
    public override void Initialize(VisualizationSettings settings)
    {
        base.Initialize(settings);
        
        _minimumRadius = settings.minimumRadius;
        
        lineScales = new float[segments + 1];
        lines = new LineRenderer[lineScales.Length];
        
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = LineSpawner.Initialize(lineRendererPrefab, material);
        }

        _currentRadius = _minimumRadius;
        audioVisualizationController.LineScales = lineScales;
    }

    public override void UpdateVisualization()
    {
        base.UpdateVisualization();
        
        for (int i = 0; i < lines.Length; i++)
        {
            float t = i / (lines.Length - 2f);
            float a = t * Mathf.PI * 2f;

            Vector2 direction = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
            float maxRadius = (_currentRadius + bufferAreaSize + lineScales[i]);

            lines[i].SetPosition(0, direction * _currentRadius);
            lines[i].SetPosition(1, direction * maxRadius);
            
            lines[i].startWidth = Spacing(_currentRadius);
            lines[i].endWidth = Spacing(maxRadius);
        }
        
        UpdateColors(lines);
    }
    
    
    private float Spacing(float radius)
    {
        float c = 2f * Mathf.PI * radius;
        float nums = lines.Length;
        return c / nums;
    }

}
