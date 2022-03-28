using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Circuit : MonoBehaviour
{
	Material _mat;

	public delegate void CircuitEvent();
	public event CircuitEvent _powerChange;

	public UnityEvent _onPowered;
	public UnityEvent _onPoweredOff;

	public string _fillVar;
	float _length;
	public float _fillDur;
	public float _fallDur;
	public Circuit [] _next;
	public float _fill;
	[HideInInspector]
	public Circuit _prev;
	public float _fillRateOverride;
	[HideInInspector]
	public bool _isCable;

	AudioSource _source;
	[Header("Audio")]
	public float _toneFrequency;
	public float _toneNoise;
	public float _maxVolume;
	public float _volLerp;
	public bool _playAudio;
	public Vector2 _pitchRange;
	[HideInInspector]
	public float _targetVolume;
	int _chainLength;
	int _indexInChain;

	void Awake(){
		_mat=GetComponent<Renderer>().material;
		if(GetComponent<Cable>()!=null)
		{
			_length=GetComponent<Cable>().GetLength();
			_isCable=true;
		}
		else if(GetComponent<LineRenderer>()!=null)
		{
			_length=GetLengthFromLineRenderer();
		}
		foreach(Circuit n in _next)
			n._prev=this;
		if(_playAudio){
			_source=gameObject.AddComponent<AudioSource>();
			//_source.clip=Synthesizer.GenerateSineWave(880,1f,0.05f);
			_source.clip=Synthesizer.GenerateSquareWave(_toneFrequency,1f,_toneNoise);
			_source.spatialBlend=0f;
			_source.loop=true;
			_source.volume=0;
		}

		_mat.SetFloat(_fillVar,_fill);
	}

    // Start is called before the first frame update
    void Start()
    {

		//get pitch range when circuit is chained with others
		int prevCircuits=0;
		int nextCircuits=0;
		float totalLength=_length;//start with my length, and add others
		Circuit c = this;
		while(c._prev!=null){
			prevCircuits++;
			c=c._prev;
			if(c._isCable){
				totalLength+=c.GetComponent<Cable>().GetLength();
			}
			else{
				totalLength+=c.GetLengthFromLineRenderer();
			}
		}
		c=this;
		while(c._next.Length==1){
			nextCircuits++;
			c=c._next[0];
			if(c._isCable){
				totalLength+=c.GetComponent<Cable>().GetLength();
			}
			else{
				totalLength+=c.GetLengthFromLineRenderer();
			}
		}
		_indexInChain=prevCircuits;
		_chainLength=prevCircuits+nextCircuits+1;

		float minPitch01 = _indexInChain/(float)_chainLength;
		float maxPitch01 = (_indexInChain+1)/(float)_chainLength;
		float minPitch = Mathf.Lerp(_pitchRange.x,_pitchRange.y,minPitch01);
		float maxPitch = Mathf.Lerp(_pitchRange.x,_pitchRange.y,maxPitch01);
		_pitchRange = new Vector2(minPitch,maxPitch);

		//scale back fill / fall dur to fit the percentage of overall chain
		float lengthFrac=_length/totalLength;
		_fillDur*=lengthFrac;
		_fallDur*=lengthFrac;
    }

    // Update is called once per frame
    void Update()
    {
		if(_source==null)
			return;
		if(Mathf.Abs(_targetVolume-_source.volume)>0.001f){
			_source.volume=Mathf.Lerp(_source.volume,_targetVolume,_volLerp*Time.deltaTime);
			if(Mathf.Abs(_targetVolume-_source.volume)<=0.001f)
				_source.volume=_targetVolume;
			if(_source.volume==0)
				_source.Stop();
			else if(!_source.isPlaying)
				_source.Play();
		}
    }

	public void Power(bool on){
		if(!gameObject.activeSelf)
			return;
		if(!on)
		{
			if(!IsLastInChainWithPower()){
				foreach(Circuit n in _next){
					if(n._fill>0)//circuits in the middle of routines have some volume
						n.Power(false);
				}
				return;
			}
		}


		StopAllCoroutines();
		if(on){
			StartCoroutine(PowerR());
		}
		else{
			StartCoroutine(PowerDownR());
		}
	}

	IEnumerator PowerR(){
		float timer=0;
		float length = _length;
		float fullDur =_fillDur;
		float rate = length/fullDur;
		length=(1-_fill)*length;
		float dur=length/rate;
		timer=fullDur-dur;
		_targetVolume=_maxVolume;
		while(timer<fullDur){
			timer+=Time.deltaTime;
			_fill=timer/dur;
			_mat.SetFloat(_fillVar,_fill);
			if(_source!=null)
				_source.pitch=Mathf.Lerp(_pitchRange.x,_pitchRange.y,_fill);
			yield return null;
		}
		_fill=1f;
		_targetVolume=0;
		_mat.SetFloat(_fillVar,_fill);
		//continue power
		foreach(Circuit n in _next){
			n.Power(true);
		}
		if(_powerChange!=null)
		{
			_powerChange.Invoke();
		}
		_onPowered.Invoke();
	}

	IEnumerator PowerDownR(){
		_targetVolume=_maxVolume;
		if(_powerChange!=null)
		{
			//this initial fill is taken out, so the line registers as unpowered
			_fill-=0.01f;
			_powerChange.Invoke();
		}
		_onPoweredOff.Invoke();
		float timer=0;
		float length = _length;
		float fullDur =_fallDur;
		float rate = length/fullDur;
		length*=_fill;
		float dur = length/rate;
		timer=dur;
		while(timer>0){
			timer-=Time.deltaTime;
			_fill=timer/fullDur;
			_mat.SetFloat(_fillVar,_fill);
			if(_source!=null)
				_source.pitch=Mathf.Lerp(_pitchRange.x,_pitchRange.y,_fill);
			yield return null;
		}
		_targetVolume=0;
		_fill=0;
		_mat.SetFloat(_fillVar,_fill);
		if(_prev!=null)
			_prev.Power(false);
	}

	public bool Powered(){
		return _fill>=1f;
	}

	public float GetLengthFromLineRenderer(){
		LineRenderer lr = GetComponent<LineRenderer>();
		Vector3 [] points = new Vector3[lr.positionCount];
		lr.GetPositions(points);
		float dist=0;
		Vector3 prevPos=Vector3.zero;
		bool first=true;
		foreach(Vector3 p in points){
			if(first)
			{
				prevPos=p;
				first=false;
			}
			else
				dist+=(p-prevPos).magnitude;
			prevPos=p;
		}
		return dist;
	}

	public bool IsLastInChainWithPower(){
		foreach(Circuit n in _next){
			if(n._fill>0)
				return false;
		}
		return true;
	}

	public void Activate(bool active){
		_mat=GetComponent<Renderer>().material;
		_mat.SetFloat("_Outline",active? 0.2f : 0.5f);
	}
}
