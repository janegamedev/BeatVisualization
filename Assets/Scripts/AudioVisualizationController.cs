using System;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(AudioSource), typeof(VisualizationDisplay), typeof(BeatDetector))]
public class AudioVisualizationController : MonoBehaviour
{
    public VisualizationSettings settings;
    
    #region CALCULATED
    
    private float _averagePower;
    private float _db;
    private float _pitch;

    private float _sampleRate;
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
        _sampleRate = AudioSettings.outputSampleRate;
        
        _samples = new float[1024];
        _spectrum = new float[1024];

        switch (settings.visualizationMode)
        {
            case VisualizationMode.RING:
                _currentVisualization = gameObject.AddComponent<RingVisualization>();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        _currentVisualization.Initialize(settings);
    }
    
    private void Update()
    {
        AnalyzeAudio();
        CalculateLineScales();
        _currentVisualization.UpdateVisualization();
    }

    private void CalculateLineScales()
    {
        int index = 0;
        int spectralIndex = 0;
     
        int averageSize =(int) Mathf.Abs(_samples.Length * settings.sampledPercentage);
        averageSize /= settings.segments;
        if(averageSize < 1)
        {
            averageSize = 1;
        }

        while(index < settings.segments)
        {
            int i = 0;
            float sum = 0;
            while(i < averageSize)
            {
                sum += _spectrum[spectralIndex];
                spectralIndex++;
                i++;
            }
        
            float yScale = sum / averageSize * settings.lineMultiplier;
            _lineScales[index] -= Time.deltaTime * settings.smoothingSpeed;
        
            if(_lineScales[index] < yScale)
            {
                _lineScales[index] = yScale;
            }
        
            if(_lineScales[index] > settings.maximumScale)
            {
                _lineScales[index] = settings.maximumScale;
            }
            index++;
        }
                
    }

    private void AnalyzeAudio()
    {
        _audioSource.GetOutputData(_samples, 0);
        
        float sum = _samples.Sum(t => t * t);

        _averagePower = Mathf.Sqrt(sum / _samples.Length);
        _db = 20 * Mathf.Log10(_averagePower * 0.1f);

        _audioSource.GetSpectrumData(_spectrum, 0, FFTWindow.BlackmanHarris);
        
        float maxV = 0;
        int maxN = 0;

        for (int i = 0; i < _samples.Length; i++)
        {
            if (!(_spectrum[i] > maxV) || !(_spectrum[i] > .0f))
            {
                continue;
            }

            maxV = _spectrum[i];
            maxN = i;
        }

        float frequenceN = maxN;
        if (maxN > 0 && maxN < _samples.Length - 1)
        {
            float dl = _spectrum[maxN - 1] / _spectrum[maxN];
            float dr = _spectrum[maxN + 1] / _spectrum[maxN];
            frequenceN += .5f * (dr * dr - dl * dl);
        }

        _pitch = frequenceN * (_sampleRate / 2) / _samples.Length;
    }
    
    //this event will be called every time a beat is detected.
    public void AddRadiusWidth()
    {
        _visualizationDisplay.SwitchColors();
    }
    
}