using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;

public class VisualizationDisplay : MonoBehaviour
{
    public Gradient gradientA = new Gradient();
    public Gradient gradientB = new Gradient();
    private static Gradient _currentColor = new Gradient();

    private void Start()
    {
        _currentColor = gradientA;
    }

    public void SwitchColors()
    {
        _currentColor = _currentColor.Equals(gradientA) ? gradientB : gradientA;
    }
    

    public static Color GetColor(float value)
    {
        return _currentColor.Evaluate(value);
    }
}