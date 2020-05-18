using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineVisualization : Visualizator
{
    public override void Initialize(VisualizationSettings settings)
    {
        base.Initialize(settings);
        UpdateCameraPos(settings.offset);
        
        lineScales = new float[segments];
        lines = new LineRenderer[lineScales.Length];

        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = LineSpawner.Initialize(lineRendererPrefab, material);
        }
        
        audioVisualizationController.LineScales = lineScales;
    }

    public override void UpdateVisualization()
    {
        base.UpdateVisualization();
        
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i].SetPosition(0, Vector3.right * i);
            lines[i].SetPosition(1, (Vector3.right * i) + Vector3.up * (bufferAreaSize + lineScales[i]));

            lines[i].startWidth = 1f;
            lines[i].endWidth = 1f;
        }
        
        UpdateColors(lines);
    }


}
