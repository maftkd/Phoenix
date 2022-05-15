using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fader : MonoBehaviour
{
	AudioSource _audio;
	float _targetVolume;
	public float _maxVol;
	const float _fudge= 0.001f;
	public float _lerp;
	bool _on;
	public bool _dontModPitch;
	public bool _fancyEnvelope;
	float _attack;
	public float _attackDur;
	public AnimationCurve _attackCurve;

	void Awake(){
	}
    // Start is called before the first frame update
    void Start()
    {
		_audio=GetComponent<AudioSource>();
		_audio.volume=0;
		_audio.Stop();
		_audio.loop=true;
    }

    // Update is called once per frame
    void Update()
    {
		if(!_audio.isPlaying)
			return;
		//fade in
		if(_on && _audio.volume<_targetVolume-_fudge){
			_audio.volume=Mathf.Lerp(_audio.volume,_targetVolume,_lerp*Time.deltaTime);
			if(_audio.volume>=_targetVolume-_fudge)
				_audio.volume=_targetVolume;
		}
		//fade out - assuming targetVolume is 0
		else if(!_on&&_audio.volume>_targetVolume+_fudge){
			_audio.volume=Mathf.Lerp(_audio.volume,_targetVolume,_lerp*Time.deltaTime);
			if(_audio.volume<=_targetVolume+_fudge)
			{
				_audio.volume=_targetVolume;
				_audio.Stop();
			}
		}
    }

	public void Play(){
		_on=true;
		SetTarget(_maxVol);
		if(!_dontModPitch)
			_audio.pitch=Random.Range(0.8f,1.2f);
	}
	public void Stop(){
		SetTarget(0);
		_on=false;
	}

	public void SetTarget(float f){
		if(!_on)
			return;
		_targetVolume=f*_maxVol;
		if(f>0&&!_audio.isPlaying)
		{
			_audio.Play();
		}
	}

	public bool IsOn(){
		return _on;
	}
}
