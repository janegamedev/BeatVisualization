using UnityEngine;
using UnityEngine.Events;

public partial class BeatDetector : MonoBehaviour
{
	#region EVENTS

	[System.Serializable] public class OnBeatEventHandler : UnityEvent{}
	[Header("Events: ")]
	public OnBeatEventHandler onBeat;

	#endregion

	#region SETTINGS

	[Header("Settings: ")]
	public int bufferSize = 1024;
	public bool limitBeats = false;
    public int limitAmount;
    public float threshold = 0.1f; 				// sensitivity

    #endregion

    #region CALCULATED VALUES
    
    private int _blipDelayLen = 16;
    private int[] _blipDelay;

	private int _samplingRate = 44100; 			// fft sampling frequency
	private const int BANDS = 12;				// number of bands
  
	private int _sinceLast = 0;					// counter to suppress double-beats
	private float _framePeriod;

	private readonly int _colMax = 120;

	private float[] _spectrum;
	private float[] _averages;
	private float[] _delayValues;
	private float[] _onSets;
	private float[] _scores;
	private float[] _dobeat;
	
	private int _now = 0;						// time index for circular buffer within above
	private float[] _spec; 						// the spectrum of the previous step
	
    #endregion

	#region AUTOCORRELATION STRUCTURE

	private const int MAXIMUM_LAG = 100;		// (in frames) largest lag to track
	private const float SMOOTH_DECAY = 0.997f;	// smoothing constant for running average
	private Autocorrelator _autocorrelator;
	private float _alph; 						// trade-off constant between tempo deviation penalty and onset strength
	
	#endregion

	#region REFERENCES

	private AudioSource _audioSource;
	
	#endregion
	
	private void Start ()
	{
		InitArrays();

		_audioSource = GetComponent<AudioSource> ();
		_samplingRate = _audioSource.clip.frequency;

		_framePeriod = (float)bufferSize / (float)_samplingRate;

		//initialize record of previous spectrum
		_spec = new float[BANDS];
		for (int i = 0; i < BANDS; i++)
		{
			_spec[i] = 100.0f;
		}

		_autocorrelator = new Autocorrelator(MAXIMUM_LAG, SMOOTH_DECAY, _framePeriod, GetHalfBandWidth () * 2);
	}
	private void InitArrays()
	{
		_blipDelay = new int[_blipDelayLen];
		_onSets = new float[_colMax];
		_scores = new float[_colMax];
		_dobeat = new float[_colMax];
		_spectrum = new float[bufferSize];
		_averages = new float[BANDS];
		_delayValues = new float[MAXIMUM_LAG];
		_alph = 100 * threshold;
	}
	
	
	void Update ()
	{
		//return if nothing is playing
		if (!_audioSource.isPlaying) return;
		
		_audioSource.GetSpectrumData(_spectrum, 0, FFTWindow.BlackmanHarris);
		ComputeAverages(_spectrum);

		/* calculate the value of the onset function in this frame */
		float onset = 0;
		for (int i = 0; i < BANDS; i++)
		{
			float specValue = Mathf.Max(-100.0f, 20.0f * Mathf.Log10(_averages[i]) + 160);
			specValue *= 0.025f;
			float dbIncrement = specValue - _spec[i]; 			// dB increment since last frame 
			_spec[i] = specValue; 								// record this frome to use next time around
			onset += dbIncrement; 								// onset function is the sum of dB increments
		}

		_onSets[_now] = onset;

		/* update autocorrelator and find peak lag = current tempo */
		_autocorrelator.NewValue(onset);

		// record largest value in (weighted) autocorrelation as it will be the tempo
		float maximumDelay = 0.0f;
		int tempo = 0;

		for (int i = 0; i < MAXIMUM_LAG; i++)
		{
			float delayValue = Mathf.Sqrt(_autocorrelator.DelayAtIndex(i));
			if (delayValue > maximumDelay)
			{
				maximumDelay = delayValue;
				tempo = i;
			}

			// store in array backwards, so it displays right-to-left, in line with traces
			_delayValues[MAXIMUM_LAG - 1 - i] = delayValue;
		}

		/* calculate DP-ish function to update the best-score function */
		float maximumScore = -999999;
		int maximumScoreIndex = 0;
		// weight can be varied dynamically with the mouse

		// consider all possible preceding beat times from 0.5 to 2.0 x current tempo period
		for (int i = tempo / 2; i < Mathf.Min(_colMax, 2 * tempo); i++)
		{
			// objective function - this beat's cost + score to last beat + transition penalty
			float score = onset + _scores[(_now - i + _colMax) % _colMax] - _alph * Mathf.Pow(Mathf.Log((float) i / (float) tempo), 2);

			// keep track of the best-scoring predecesor
			if (score > maximumScore)
			{
				maximumScore = score;
				maximumScoreIndex = i;
			}
		}

		_scores[_now] = maximumScore;
		// keep the smallest value in the score fn window as zero, by subtracing the min val

		float minimumScore = _scores[0];
		for (int i = 0; i < _colMax; i++)
		{
			if (_scores[i] < minimumScore)
			{
				minimumScore = _scores[i];
			}
		}
		for (int i = 0; i < _colMax; i++)
		{
			_scores[i] -= minimumScore;
		}
			
		/* find the largest value in the score fn window, to decide if we emit a blip */
		maximumScore = _scores[0];
		maximumScoreIndex = 0;
		for (int i = 0; i < _colMax; i++)
		{
			if (_scores[i] > maximumScore)
			{
				maximumScore = _scores[i];
				maximumScoreIndex = i;
			}
		}

		// dobeat array records where we actally place beats
		_dobeat[_now] = 0;
		_sinceLast++;
		// if current value is largest in the array, probably means we're on a beat
		if (maximumScoreIndex == _now)
		{
			if (limitBeats)
			{
				if (_sinceLast > tempo / limitAmount)
				{
					onBeat.Invoke();
					_blipDelay[0] = 1;
					// record that we did actually mark a beat this frame
					_dobeat[_now] = 1;
					// reset counter of frames since last beat
					_sinceLast = 0;
				}
			}
			else
			{
				onBeat.Invoke();
			}
		}

		// update column index
		_now++;
		if (_now >= _colMax)
		{
			_now = 0;
		}
	}

	private float GetHalfBandWidth()
	{
		return (2f / (float)bufferSize) * (_samplingRate / 2f) * .5f;
	}

	private void ComputeAverages(float[] data)
	{
		for (int i = 0; i < BANDS; i++) 
		{
			float avg = 0;

			var lowFreq = i == 0 ? 0 : (int) ((_samplingRate / 2) / (float) System.Math.Pow(2, BANDS - i));
			int hiFreq = (int)((_samplingRate / 2) / (float)System.Math.Pow (2, BANDS - 1 - i));
			
			int lowBound = FrequencyByIndex(lowFreq);
			int hiBound = FrequencyByIndex(hiFreq);
			for (int j = lowBound; j <= hiBound; j++) {
				//Debug.Log("lowbound: " + lowBound + ", highbound: " + hiBound);
				avg += data [j];
			}
			
			avg /= (hiBound - lowBound + 1);
			_averages[i] = avg;
		}
	}
	
	private int FrequencyByIndex (int frequencyIndex)
	{
		// special case: freq is lower than the bandwidth of spectrum[0]
		if (frequencyIndex < GetHalfBandWidth())
		{
			return 0;
		}
		
		// special case: freq is within the bandwidth of spectrum[512]
		if (frequencyIndex > _samplingRate / 2 - GetHalfBandWidth())
		{
			return (bufferSize / 2);
		}
	
		float fraction = (float)frequencyIndex / (float)_samplingRate;
		return Mathf.RoundToInt (bufferSize * fraction);
	}

}
