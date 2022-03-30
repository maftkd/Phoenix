using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSwitch : MonoBehaviour
{
	public float _radius;
	Bird _player;
	bool _inZone;
	bool _prevInZone;
	public int _tutorialIndex;
	TutorialZone _tz;
	Transform _handle;
	Quaternion _offRot;
	Quaternion _onRot;
	bool _isOn;
	public float _animDur;
	public AudioClip _switchClip;
	Color _emissionColor;
	Color _defaultColor;
	Material _mat;
	bool _init;
	bool _active;
	public Circuit [] _next;
	Transform _cover;
	Quaternion _openRot;
	Quaternion _closeRot;
	public float _coverAnimDur;
	public AudioClip _openClip;
	public AudioClip _closeClip;
	public AnimationCurve _clipCurve;

	void Awake(){
		if(!_init)
			Init();
	}

	void Init(){
		_player=GameManager._player;
		_tz=GetComponent<TutorialZone>();
		_handle=transform.GetChild(0);
		_offRot=_handle.rotation;
		_handle.Rotate(Vector3.right*45f);
		_onRot=_handle.rotation;
		_handle.rotation=_offRot;

		_mat=_handle.GetComponent<MeshRenderer>().materials[1];
		_defaultColor=_mat.GetColor("_Color");
		float h=0;float s=0;float v=0;
		Color.RGBToHSV(_defaultColor,out h,out s,out v);
		_emissionColor=Color.HSVToRGB(h,1,1);

		_cover=transform.GetChild(1);
		_closeRot=_cover.rotation;
		_cover.Rotate(Vector3.right*135f);
		_openRot=_cover.rotation;
		_cover.rotation=_closeRot;

		_init=true;
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if(!_active)
			return;
		_inZone=(_player.transform.position-(transform.position+transform.forward*_cover.localPosition.z*2)).sqrMagnitude<_radius*_radius;
		if(_inZone!=_prevInZone){
			if(_inZone)
			{
				if(_tutorialIndex>0)
					_tz.ShowTutorial(_tutorialIndex);
				_player.NearLightSwitch(this);
				StartCoroutine(OpenCover());
			}
			else
			{
				if(_tutorialIndex>0)
					_tz.HideTutorial();
				_player.NearLightSwitch(null);
				StartCoroutine(CloseCover());
			}
		}
		_prevInZone=_inZone;
    }

	public void Toggle(){
		if(!_active)
			return;
		StartCoroutine(ToggleR());
	}

	IEnumerator ToggleR(){
		_isOn=!_isOn;

		//_player._anim.SetFloat("peckDir", _isOn? -1f : 1f);
		_player._anim.SetTrigger("peck");

		_mat.SetColor("_EmissionColor", _isOn ? _emissionColor : Color.black);
		Quaternion startRot=_isOn?_offRot : _onRot;
		foreach(Circuit n in _next)
			n.Power(_isOn);

		Quaternion endRot=_isOn?_onRot : _offRot;
		float pitch=_isOn?Random.Range(0.9f,1.1f):Random.Range(0.7f,0.9f);
		Sfx.PlayOneShot3D(_switchClip,transform.position,pitch);
		yield return null;

		//rotating is a little tricky because need to account for waddlecam and mcam
		_player.transform.LookAt(transform);
		Vector3 eulerAngles=_player.transform.eulerAngles;
		eulerAngles.x=0;
		_player.transform.eulerAngles=eulerAngles;
		yield return null;
		GameManager._mCam.Snap();

		float timer=0;
		float dur=_animDur;
		while(timer<dur){
			timer+=Time.deltaTime;
			float frac=timer/dur;
			_handle.rotation=Quaternion.Slerp(startRot,endRot,frac);
			//_player.transform.rotation=Quaternion.Slerp(playerStart,playerEnd,frac);
			yield return null;
		}
		//_player.transform.rotation=playerEnd;
		_handle.rotation=endRot;
		_player.Ground();
	}

	public void Activate(bool active){
		if(!_init)
			Init();
		_mat.SetColor("_EmissionColor", Color.black);
		_mat.SetColor("_Color", active? _defaultColor : Color.black);
		_active=active;
	}

	IEnumerator OpenCover(){
		float timer=0;
		float dur = _coverAnimDur;
		float frac=0;
		Sfx.PlayOneShot3D(_openClip,transform.position,Random.Range(0.8f,1.2f));
		yield return null;
		while(timer<dur){
			timer+=Time.deltaTime;
			frac=timer/dur;
			_cover.rotation=Quaternion.Slerp(_closeRot,_openRot,_clipCurve.Evaluate(frac));
			yield return null;
		}
		_cover.rotation=_openRot;
	}

	IEnumerator CloseCover(){
		float timer=0;
		float dur = _coverAnimDur;
		float frac=0;
		Sfx.PlayOneShot3D(_closeClip,transform.position,Random.Range(0.8f,1.2f));
		yield return null;
		while(timer<dur){
			timer+=Time.deltaTime;
			frac=timer/dur;
			_cover.rotation=Quaternion.Slerp(_closeRot,_openRot,_clipCurve.Evaluate(1-frac));
			yield return null;
		}
		_cover.rotation=_closeRot;

	}

	void OnDrawGizmos(){
		Gizmos.color=Color.red;
		Gizmos.DrawWireSphere(transform.position,_radius);
	}
}
