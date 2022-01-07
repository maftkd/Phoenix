using System.Collections;
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
	Transform _player;
	AudioSource _audio;
	CanvasGroup _flash;
	public AnimationCurve _flashCurve;
	public float _minDistToTarget;
	public float _flashEffectDur;
	Image _ringFill;
	public UnityEvent _onFlash;
	Quaternion _startRot;

	void Awake(){
		_camera=transform.GetChild(1);
		_player=GameObject.FindGameObjectWithTag("Player").transform;
		_audio=GetComponent<AudioSource>();
		_flash=transform.GetChild(2).GetComponent<CanvasGroup>();
		_flashDur=_flashCharge.length;
		_ringFill=transform.GetChild(3).GetChild(1).GetComponent<Image>();
		_startRot=transform.rotation;
		_targetPos=_ex.position;
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
			Refocus();
		}
		if((_player.position-_targetPos).sqrMagnitude<=0.01f){
			StartCoroutine(Flash());
		}
		_ringFill.fillAmount=Mathf.Lerp(_ringFill.fillAmount,0,Time.deltaTime);
    }

	void Refocus(){
		Quaternion curRot=_camera.rotation;
		_camera.LookAt(_player);
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
		if(!_audio.isPlaying){
			_audio.clip=_zoom;
			_audio.pitch=Random.Range(_zoomPitchRange.x,_zoomPitchRange.y);
			_audio.Play();
		}
		float dur = angle/_anglesPerSec;
		float timer=0;
		while(timer<dur){
			timer+=Time.deltaTime;
			_camera.rotation=Quaternion.Slerp(start,target,timer/dur);
			yield return null;
		}
		_camera.rotation=target;
	}

	IEnumerator Flash(){
		enabled=false;
		//zoom
		_camera.LookAt(_player);
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
			_ringFill.fillAmount=timer/_flashDur;
			if((_player.position-_targetPos).sqrMagnitude>_minDistToTarget*_minDistToTarget){
				enabled=true;
				_audio.Stop();
				StopAllCoroutines();
			}
			yield return null;
		}

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

		//start effects
		_onFlash.Invoke();
	}

	void OnDrawGizmos(){
		if(_targetPos==null)
			return;
		Gizmos.color=Color.red;
		Gizmos.DrawWireSphere(_targetPos,0.15f);
	}
}
