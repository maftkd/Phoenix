using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sing : MonoBehaviour
{
	MInput _mIn;
	Fader _low;
	Fader _mid;
	Fader _high;
	public float _lowFreq;
	public float _midFreq;
	public float _highFreq;
	Animator _anim;
	int _state;
	public float _animFallWindow;
	float _animFallTimer;

	void Awake(){
		_mIn=GameManager._mIn;
		GameObject lowGo=new GameObject("Low Whistle");
		lowGo.transform.SetParent(transform);
		AudioSource lowAs=lowGo.AddComponent<AudioSource>();
		lowAs.clip=Synthesizer.GenerateSomethingHarmonic(_lowFreq,1,0);
		_low=lowGo.AddComponent<Fader>();
		_low._maxVol=1f;
		_low._lerp=10f;
		_low._dontModPitch=true;

		GameObject midGo=new GameObject("Mid Whistle");
		midGo.transform.SetParent(transform);
		AudioSource midAs=midGo.AddComponent<AudioSource>();
		midAs.clip=Synthesizer.GenerateSomethingHarmonic(_midFreq,1,0);
		_mid=midGo.AddComponent<Fader>();
		_mid._maxVol=1f;
		_mid._lerp=10f;
		_mid._dontModPitch=true;

		GameObject highGo=new GameObject("High Whistle");
		highGo.transform.SetParent(transform);
		AudioSource highAs=highGo.AddComponent<AudioSource>();
		highAs.clip=Synthesizer.GenerateSomethingHarmonic(_highFreq,1,0);
		_high=highGo.AddComponent<Fader>();
		_high._maxVol=1f;
		_high._lerp=10f;
		_high._dontModPitch=true;

		_anim=transform.GetComponentInParent<Animator>();

	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		switch(_state){
			case 0://not singing
				if(_mIn.GetLowDown()){
					_low.Play();
					_anim.SetFloat("pitch",0.333f);
					_state=1;
				}
				else if(_mIn.GetMidDown()){
					_mid.Play();
					_anim.SetFloat("pitch",0.666f);
					_state=2;
				}
				break;
			case 1://sing low
				if(_mIn.GetMidDown()){
					_low.Stop();
					_mid.Play();
					_anim.SetFloat("pitch",0.666f);
					_state=2;
				}
				else if(_mIn.GetLowUp()){
					_low.Stop();
					_state=0;
					_anim.SetFloat("pitch",0f);
				}
				break;
			case 2:
				if(_mIn.GetLowDown()){
					_low.Play();
					_mid.Stop();
					_anim.SetFloat("pitch",0.333f);
					_state=1;
				}
				else if(_mIn.GetMidUp()){
					_mid.Stop();
					_state=0;
					_anim.SetFloat("pitch",0f);
				}
				break;
			case 3:
				break;
			default:
				break;
		}
		if(_mIn.GetHighDown()){
			//Debug.Log("High Down");
			_high.Play();
		}
		else if(_mIn.GetHighUp()){
			//Debug.Log("High Up");
			_high.Stop();
		}
        
    }
}
