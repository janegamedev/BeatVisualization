using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleLineVisualization : Visualizator
{
    private LineRenderer[] _doubledLines;
    
    public override void Initialize(VisualizationSettings settings)
    {
        base.Initialize(settings);
        UpdateCameraPos(settings.offset);
        
        lineScales = new float[segments];
        lines = new LineRenderer[lineScales.Length];
        _doubledLines = new LineRenderer[lineScales.Length];
        
        for (int i = 0; i < lines.Length; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                lines[i] = LineSpawner.Initialize(lineRendererPrefab, material);
                _doubledLines[i] = LineSpawner.Initialize(lineRendererPrefab, material);
            }
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

            _doubledLines[i].SetPosition(0, Vector3.right * i);
            _doubledLines[i].SetPosition(1, (Vector3.right * i) + Vector3.down * (bufferAreaSize + lineScales[i]));

            lines[i].startWidth = _doubledLines[i].startWidth = 3f;
            lines[i].endWidth = _doubledLines[i].endWidth = 3f;
        }

        UpdateColors(lines);
        UpdateColors(_doubledLines);
    }
}
