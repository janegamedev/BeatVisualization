using UnityEngine;

public class Autocorrelator
{
    private int _index;
    private readonly int _delayLength;
    private readonly float _smoothDecay;
    private readonly float _octaveWidth;
    private readonly float _framePeriod;

    private readonly float[] _delays;
    private readonly float[] _outputs;
    private readonly float[] _bpms;
    private readonly float[] _weights;

    public Autocorrelator (int delayLength, float smoothDecay, float framePeriod, float bandwidth)
    {
        _index = 0;

        _octaveWidth = bandwidth;
        _smoothDecay = smoothDecay;
        _delayLength = delayLength;
        _framePeriod = framePeriod;

        _delays = new float[delayLength];
        _outputs = new float[delayLength];
        _bpms = new float[delayLength];
        _weights = new float[delayLength];

        ApplyWeights();
    }

    private void ApplyWeights()
    {
        for (int i = 0; i < _delayLength; i++) 
        {
            _bpms [i] = 60.0f / (_framePeriod * i);
            _weights [i] = Mathf.Exp (-0.5f * Mathf.Pow (Mathf.Log (_bpms [i] / 120f) / Mathf.Log (2.0f) / _octaveWidth, 2.0f));
        }
    }

    public void NewValue (float onset)
    {
        _delays [_index] = onset;

        // update running autocorrelator values
        for (int i = 0; i < _delayLength; i++) 
        {
            int delayIndex = (_index - i + _delayLength) % _delayLength;
            _outputs [i] += (1 - _smoothDecay) * (_delays [_index] * _delays [delayIndex] - _outputs [i]);
        }

        _index++;
        if (_index >= _delayLength)
            _index = 0;
    }

    // read back the current autocorrelator value at a particular lag
    public float DelayAtIndex (int delayIndex)
    {
        return _weights[delayIndex] * _outputs[delayIndex];
    }
    
    
}