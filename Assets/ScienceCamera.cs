﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ScienceCamera : MonoBehaviour
{
	public float _focusDelay;
	public float _minAngleToRotate;
	public float _maxRotationAngle;
	public float _anglesPerSec;
	float _focusTimer;
	Vector3 _targetPos;
	public Transform _ex;
	public AudioClip _zoom;
	public Vector2 _zoomPitchRange;
	public AudioClip _shutter;
	public AudioClip _flashCharge;
	float _flashDur;
	float _flashTimer;
	Transform _camera;
	public Transform _bird;
	AudioSource _audio;
	CanvasGroup _flash;
	public AnimationCurve _flashCurve;
	public float _minDistToTarget;
	public float _flashEffectDur;
	public MeshRenderer _progressBar;
	Material _progressMat;
	public UnityEvent _onFlash;
	Quaternion _startRot;
	public GameObject [] _ledLit;
	public GameObject [] _ledUnlit;
	public ScienceCamera _otherCam;
	bool _lit;
	public bool _noFlash;
	public Circuit [] _outputs;
	bool _birdPrevOnEx;

	void Awake(){
		_camera=transform.GetChild(1);
		_audio=GetComponent<AudioSource>();
		_flash=transform.GetChild(2).GetComponent<CanvasGroup>();
		_flashDur=_flashCharge.length;
		if(_progressBar!=null)
			_progressMat=_progressBar.material;
		_startRot=transform.rotation;
		_targetPos=_ex.position;
		_lit=true;
		LightLed(false);
	}

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
		_focusTimer+=Time.deltaTime;
		if(_focusTimer>=_focusDelay){
			if(_bird!=null)
				Refocus();
		}
		if(_bird!=null){
			bool birdOnEx=(_bird.position-_targetPos).sqrMagnitude<=_minDistToTarget*_minDistToTarget;
			if(birdOnEx!=_birdPrevOnEx){
				foreach(Circuit c in _outputs){
					c.Power(birdOnEx);
				}
				if(!_audio.isPlaying){
					_audio.clip=_zoom;
					_audio.pitch=Random.Range(_zoomPitchRange.x,_zoomPitchRange.y);
					_audio.Play();
				}

			}
			_birdPrevOnEx=birdOnEx;
		}

		if(_progressMat!=null)
		{
			float prog=_progressMat.GetFloat("_FillAmount");
			if(prog>0)
			{
				prog-=Time.deltaTime;
				if(prog<0)
					prog=0;
				_progressMat.SetFloat("_FillAmount",prog);
			}
		}
    }

	void Refocus(){
		Quaternion curRot=_camera.rotation;
		_camera.LookAt(_bird);
		Quaternion targetRot=_camera.rotation;
		_camera.rotation=curRot;
		float angle=Quaternion.Angle(_startRot,targetRot);
		if(angle<_maxRotationAngle){
			angle=Quaternion.Angle(curRot,targetRot);
			if(angle>_minAngleToRotate){
				StopAllCoroutines();
				StartCoroutine(RefocusR(angle,curRot,targetRot));
			}
		}
		_focusTimer=0;
	}

	IEnumerator RefocusR(float angle, Quaternion start, Quaternion target){
		float dur = angle/_anglesPerSec;
		float timer=0;
		while(timer<dur){
			timer+=Time.deltaTime;
			_camera.rotation=Quaternion.Slerp(start,target,timer/dur);
			yield return null;
		}
		_camera.rotation=target;
	}

	void FindPlayer(){
	}

	IEnumerator Flash(){
		enabled=false;
		//zoom
		_camera.LookAt(_bird);
		_audio.clip=_zoom;
		_audio.pitch=Random.Range(_zoomPitchRange.x,_zoomPitchRange.y);
		_audio.Play();
		while(_audio.isPlaying)
			yield return null;
		//flash
		_audio.clip=_flashCharge;
		_audio.pitch=1f;
		_audio.Play();
		float timer=0;
		while(timer<_flashDur){
			timer+=Time.deltaTime;
			if(_progressMat!=null)
				_progressMat.SetFloat("_FillAmount",timer/_flashDur);
			_camera.LookAt(_bird);
			if((_bird.position-_targetPos).sqrMagnitude>_minDistToTarget*_minDistToTarget||(_otherCam!=null&&!_otherCam.LedOn())){
				enabled=true;
				LightLed(false);
				_audio.Stop();
				StopAllCoroutines();
			}
			yield return null;
		}
		
		if(!_noFlash){
			//start effects
			_onFlash.Invoke();
			LightLed(false);

			//shutter
			_audio.clip=_shutter;
			_audio.Play();
			timer=0;
			float dur = _flashEffectDur;
			while(timer<dur){
				timer+=Time.deltaTime;
				_flash.alpha=_flashCurve.Evaluate(timer/dur);
				yield return null;
			}
			_flash.alpha=0;
		}
		else{
			while(_otherCam.LedOn())
				yield return null;
			LightLed(false);
		}
	}

	void LightLed(bool lit){
		/*
		if(lit==_lit)
			return;
		foreach(GameObject go in _ledLit)
			go.SetActive(lit);
		foreach(GameObject go in _ledUnlit)
			go.SetActive(!lit);
		_lit=lit;
		*/
	}

	public bool LedOn(){
		return _lit;
	}

	bool OtherCamLightOn() {
		if(_otherCam==null)
			return true;//single camera
		else return _otherCam.LedOn();
	}

	void OnDrawGizmos(){
		if(_targetPos==null)
			return;
		Gizmos.color=Color.red;
		Gizmos.DrawWireSphere(_targetPos,0.15f);
	}
}
