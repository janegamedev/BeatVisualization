using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleVisualization : Visualizator
{
    private float _minimumRadius;
    private Vector3[] _circlePositions;
    private LineRenderer _circleRenderer;
    public override void Initialize(VisualizationSettings settings)
    {
        base.Initialize(settings);
        _minimumRadius = settings.minimumRadius;
        
        lineScales = new float[segments];

        _circleRenderer = LineSpawner.Initialize(lineRendererPrefab, material);
        _circleRenderer.positionCount = segments;
        _circleRenderer.startWidth = _circleRenderer.endWidth = 0.5f;

        const float z = 0f;
        _circlePositions = new Vector3[segments];
        float angle = 20f;

        for (int i = 0; i < (segments); i++)
        {
            var x = Mathf.Sin(Mathf.Deg2Rad * angle) * _minimumRadius;
            var y = Mathf.Cos(Mathf.Deg2Rad * angle) * _minimumRadius;

            _circleRenderer.SetPosition(i, new Vector3(x, y, z));
            _circlePositions[i] = new Vector3(x, y, z);
            angle += (360f / segments);
        }
        
        audioVisualizationController.LineScales = lineScales;
    }

    public override void UpdateVisualization()
    {
        base.UpdateVisualization();
        
        for (int i = 0; i < _circlePositions.Length; i++)
        {
            float t = i / (segments - 2f);
            float a = t * Mathf.PI * 2f;

            Vector2 direction = new Vector2(Mathf.Cos(a), Mathf.Sin(a));

            float maxRadius = (_minimumRadius + bufferAreaSize + lineScales[i]);

            Vector3 changedY = direction * maxRadius;

            _circleRenderer.SetPosition(i, i == _circlePositions.Length - 1 ? _circlePositions[0] : changedY);

            _circlePositions[i] = _circleRenderer.GetPosition(i);
        }
    }
}
