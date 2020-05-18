using System;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(AudioSource), typeof(VisualizationDisplay), typeof(BeatDetector))]
public class AudioVisualizationController : MonoBehaviour
{
    public VisualizationSettings settings;
    
    #region CALCULATED
    
    private float[] _samples;
    private float[] _spectrum;
    private float[] _lineScales;

    public float[] LineScales
    {
        get => _lineScales;
        set => _lineScales = value;
    }

    #endregion

    private VisualizationDisplay _visualizationDisplay;
    private Visualizator _currentVisualization;
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _visualizationDisplay = GetComponent<VisualizationDisplay>();

        //setting arrays size to 1024
        _samples = new float[1024];
        _spectrum = new float[1024];

        // this will add mode component which will be responsible for init and update depends on visualizing mode
        switch (settings.visualizationMode)
        {
            case VisualizationMode.RING:
                _currentVisualization = gameObject.AddComponent<RingVisualization>();
                break;
            case VisualizationMode.LINE:
                _currentVisualization = gameObject.AddComponent<LineVisualization>();
                break;
            case VisualizationMode.DOUBLE_LINE:
                _currentVisualization = gameObject.AddComponent<DoubleLineVisualization>();
                break;
            case VisualizationMode.CIRCLE:
                _currentVisualization = gameObject.AddComponent<CircleVisualization>();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        // calling initialization for current mode
        _currentVisualization.Initialize(settings);
    }
    
    private void Update()
    {
        AnalyzeAudio();
        CalculateLineScales();
        _currentVisualization.UpdateVisualization();
    }
    
    private void AnalyzeAudio()
    {
        // fill array with samples
        _audioSource.GetOutputData(_samples, 0);
        
        // get sound spectrum
        _audioSource.GetSpectrumData(_spectrum, 0, settings.window);
    }

    private void CalculateLineScales()
    {
        //calculating average size
        int averageSize =(int) Mathf.Abs(_samples.Length * settings.sampledPercentage);
        averageSize /= settings.segments;
        
        //limit size
        if(averageSize < 1)
        {
            averageSize = 1;
        }
        
        int spectralIndex = 0;
        
        for (int i = 0; i < settings.segments; i++)
        {
            float sum = 0;

            for (int j = 0; j < averageSize; j++)
            {
                sum += _spectrum[spectralIndex];
                spectralIndex++;
            }

            //calculating scale
            float yScale = sum / averageSize * settings.lineMultiplier;
            
            //smooth returning
            _lineScales[i] -= Time.deltaTime * settings.smoothingSpeed;
        
            
            //this will limit scale with bottom and top range
            _lineScales[i] = Mathf.Clamp(_lineScales[i], yScale, settings.maximumScale);
        }
    }
    
    //this event will be called every time a beat is detected.
    public void AddRadiusWidth()
    {
        _visualizationDisplay.SwitchColors();
    }
    
}