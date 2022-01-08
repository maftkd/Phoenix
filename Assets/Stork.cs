using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stork : MonoBehaviour
{
	int _state;
	Vector3 _targetPos;
	public float _flightDur;
	public float _flightApex;
	public float _startDist;
	public float _preDeliveryDelay;
	//apex form for quadratic y = a*(x-h)^2 + k
	float _a;
	float _h;
	float _k;
	Vector3 _flightDir;
	public AnimationCurve _flightCurve;
	MCamera _mCam;
	public Vector3 _trackOffset;
	Animator _anim;
	public float _landTime;
	public float _flapDur;
	public float _landFlapDur;
	public AudioClip _flapSound;
	public Transform _midFeet;
	public Transform _boxCarrier;
	Transform _carrier;
	Vector3 _startPos;
	Vector3 _flightStartPos;
	public float _pauseDur;
	public Vector3 _boxOffset;
	PuzzleBox _carryingBox;
	Quaternion _boxRot;
	Vector3 _boxPos;

	void Awake(){
		_mCam = Camera.main.GetComponent<MCamera>();
		_anim=GetComponent<Animator>();
		_startPos=transform.position;
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

	public void DeliverBox(PuzzleBox box){
		switch(_state){
			case 0://can start delivery from idle
				//flight plan 
				Transform t = box.transform;
				_boxRot=t.rotation;
				_boxPos=t.position;
				_targetPos=t.position-_boxOffset;
				Vector3 startPos=transform.position;
				startPos.y=_targetPos.y;
				Vector3 dir = _targetPos-startPos;
				float dist = dir.magnitude;
				_flightDir=dir/dist;
				Debug.Log("Travel dist: "+dist);
				_k = _flightApex;
				_h = _startDist;
				_a = -_k/(_h*_h);

				//payload
				EquipBox(box);

				//take-off
				StartCoroutine(ArcToTarget());
				break;
			default:
				break;
		}
	}

	IEnumerator ArcToTarget(){
		yield return new WaitForSeconds(_preDeliveryDelay);
		//camera
		_mCam.TrackTarget(transform,_trackOffset,true);
		//initial orientation
		_flightStartPos=_targetPos-_flightDir*_h+Vector3.up*_k;
		transform.position=_flightStartPos;
		transform.forward=_flightDir;
		//anim
		_anim.SetTrigger("fly");
		float timer=0;
		float t01;
		float x;
		float y;
		bool _landed=false;
		float flapTimer=0;
		while(timer<_flightDur){
			timer+=Time.deltaTime;
			t01=1-_flightCurve.Evaluate(timer/_flightDur);
			x=t01*_h;
			y = _a*(x-_h)*(x-_h)+_k;
			//flying
			Vector3 pos = _targetPos-_flightDir*x+Vector3.up*y;
			transform.position=pos;
			//landing
			if(t01<_landTime&&!_landed){
				_landed=true;
				_anim.SetTrigger("land");
			}

			flapTimer+=Time.deltaTime;
			if(!_landed){
				if(flapTimer>=_flapDur){
					Sfx.PlayOneShot3D(_flapSound,transform.position);
					flapTimer=0;
				}
			}
			else{
				if(flapTimer>=_landFlapDur){
					Sfx.PlayOneShot3D(_flapSound,transform.position);
					flapTimer=0;
				}
			}
			//ensure carrier is upright
			_carrier.rotation=Quaternion.identity;
			yield return null;
		}
		if(!_landed)
			_anim.SetTrigger("land");

		ReleaseBox();
		_mCam.TrackTarget(_carryingBox.transform,Vector3.zero,false);

		//lerp towards actual top of box before pausing
		timer=0;
		float dur=0.5f;
		Vector3 startPos=transform.position;
		Vector3 rullTarget=_carryingBox.transform.position+0.5f*Vector3.up;
		while(timer<dur){
			timer+=Time.deltaTime;
			transform.position=Vector3.Lerp(startPos,rullTarget,timer/dur);
			yield return null;
		}

		yield return new WaitForSeconds(_pauseDur);
		StartCoroutine(FlyAway());
	}

	IEnumerator FlyAway(){
		Vector3 startPos=transform.position;
		Vector3 endPos = _flightStartPos;
		transform.forward=-_flightDir;
		float timer=0;
		_anim.SetTrigger("fly");
		float flapTimer=_flapDur;
		while(timer<_flightDur){
			timer+=Time.deltaTime;
			transform.position = Vector3.Lerp(startPos,endPos,1-_flightCurve.Evaluate(1-timer/_flightDur));
			if(flapTimer>=_flapDur){
				Sfx.PlayOneShot3D(_flapSound,transform.position);
				flapTimer=0;
			}
			yield return null;
		}
		transform.position=_startPos;
		_anim.SetTrigger("land");
	}

	void EquipBox(PuzzleBox box){
		_carryingBox=box;
		_carrier = Instantiate(_boxCarrier,_midFeet);
		_carrier.localPosition=Vector3.zero;
		_carryingBox.transform.SetParent(_carrier);
		_carryingBox.transform.localPosition=_boxOffset;
		_carryingBox.gameObject.SetActive(true);
	}

	void ReleaseBox(){
		_carrier.SetParent(null);
		_carryingBox.transform.position=_boxPos;
		_carryingBox.transform.rotation=_boxRot;
		_carryingBox.Reveal();
	}
}
