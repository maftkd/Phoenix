using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hop : MonoBehaviour
{
	Vector3 _camTarget;
	Footstep _footstep;
	Collider [] _cols;
	Animator _anim;
	MInput _mIn;
	[Header("Vfx")]
	public Transform _stepParts;
	//bool _disableAfterHop;
	Bird _bird;
	Fly _fly;
	bool _hopping;
	float _velocity;
	[Header("Physics")]
	public float _hopAccel;
	public float _gravity;
	float _hopStartY;
	public float _airControl;
	public float _inputSmoothLerp;
	public float _airTurnLerp;
	public float _hopVolumeMult;
	public float _hopCancelWindow;
	float _hopTimer;
	float _hopTime;
	Vector3 _hopStartPos;
	Quaternion _hopStartRot;
	public float _minDotToHop;
	bool _firstHop;
	Vector3 _input;
	public float _footOffset;
	[Header("Hop boost")]
	public float _hopBoostWindow;
	public float _hopBoost;
	bool _diving;
	AudioSource _soarAudio;
	AudioSource _hopAudio;
	public float _divePitchMult;
	bool _knockBack;
	public float _knockBackMult;
	public float _dotToKillVert;

	//ai hopping
	Terrain _terrain;
	Vector3 _destination;
	public bool _npc;
	Vector3 _npcInput;

	//squash and stretch
	Vector3 _defaultScale;
	public float _squashMult;
	Vector3 _squashScale;
	public float _stretchMult;
	Vector3 _stretchScale;
	//and rotate
	public float _leanAngle;
	float _defaultAngle;
	Transform _mesh;

	void Awake(){
		_cols = new Collider[2];
		_camTarget=transform.position;
		_anim=GetComponent<Animator>();
		_bird=GetComponent<Bird>();
		_fly=GetComponent<Fly>();
		_mIn=GameManager._mIn;
		_terrain=FindObjectOfType<Terrain>();
		_input=Vector3.zero;
		_soarAudio=transform.Find("SoarParticles").GetComponent<AudioSource>();
		_hopAudio=transform.Find("JumpSound").GetComponent<AudioSource>();
		ResetScale();
		_mesh=transform.GetChild(0);
		_defaultAngle=_mesh.localEulerAngles.y;
	}

	void OnEnable(){
		_camTarget=transform.position;
		_firstHop=true;
		_anim.SetFloat("walkSpeed",1f);
		_diving=false;
		_knockBack=false;
		_input=_mIn.GetInputDir();

		//start da hop
		StartHop();

		if(!_npc)
		{
			//transition to hop cam
			//GameManager._mCam.Transition(_bird._waddleCam,MCamera.Transitions.CUT_BACK);
		}
	}

	void OnDisable(){
		transform.localScale=_defaultScale;
		if(!_npc)
		{
			//transition to idle cam
			//GameManager._mCam.Transition(_bird._idleCam,MCamera.Transitions.CUT_BACK);
		}
	}

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
		Vector3 rawInput = _npc ? _npcInput : _mIn.GetInputDir();
		if(!_knockBack)
			_input=Vector3.Lerp(_input,rawInput,_inputSmoothLerp*Time.deltaTime);
		if(!_hopping&& (_npc || _mIn.GetJump()))
		{
			StartHop();
		}
		if(_hopping)
		{
			//middle of hop
			Vector3 startPos=transform.position;

			//dive
			if(!_npc&&Input.GetButtonDown("Dive")){
				_diving=true;
				_anim.SetTrigger("dive");
				_soarAudio.pitch=_fly._divePitch;
				_soarAudio.volume=_fly._diveVolume;
				_soarAudio.Play();
			}

			//dive pitch
			Vector3 eulerAngles=transform.eulerAngles;
			float targetPitch=0;
			if(_diving)
				targetPitch=-Mathf.Atan2(_velocity,_input.magnitude)*Mathf.Rad2Deg*_divePitchMult;
			eulerAngles.x=targetPitch;
			transform.eulerAngles=eulerAngles;

			//rotation
			if(!_knockBack){
				if(rawInput.sqrMagnitude>0){
					Quaternion curRot=transform.rotation;
					transform.forward=rawInput;
					Quaternion targetRot=transform.rotation;
					transform.rotation=Quaternion.Slerp(curRot,targetRot,_airTurnLerp*Time.deltaTime);
				}
				else if(_hopTimer<_hopCancelWindow&&!_firstHop){
					//hop cancel
					transform.position=_hopStartPos;
					transform.rotation=_hopStartRot;
					_hopping=false;
					//_anim.SetBool("hop",false);
					_camTarget=transform.position;
					return;
				}
			}
		
			//air control
			Vector3 airControl=Vector3.zero;
			if(!_knockBack)
				airControl=transform.forward*_input.magnitude*Time.deltaTime*_airControl*Mathf.Max(0,Vector3.Dot(transform.forward,_input));
				//more air control when moving along forward... helps give time to rotate first
			else
				airControl=_input*Time.deltaTime*_airControl;
			transform.position+=airControl;

			//hop boost
			if(_hopTimer<_hopBoostWindow&&(_mIn.GetJump()||_npc)&&_velocity>0){
				_velocity+=Time.deltaTime*_hopBoost;
			}
			if(_mIn.GetJumpUp())
				_hopTimer=_hopBoostWindow;

			//apply physics
			transform.position+=_velocity*Vector3.up*Time.deltaTime;
			_velocity-=_gravity*Time.deltaTime;
			_camTarget=transform.position;
			/*
			if(_camTarget.y>_hopStartPos.y)
				_camTarget.y=_hopStartY;
				*/

			//squash and stretch
			float lerpy = Mathf.Abs(_velocity/(_hopAccel*2));
			lerpy = Mathf.Clamp01(lerpy);
			transform.localScale=Vector3.Lerp(_squashScale,_stretchScale,lerpy);
			//Vector3 eulers=_mesh.localEulerAngles;
			//eulers.y=Mathf.Lerp(_leanAngle,_defaultAngle,lerpy);
			//_mesh.localEulerAngles=eulers;

			//hit detection
			Vector3 posDelta=transform.position-startPos;
			RaycastHit hit;
			if(Physics.Raycast(startPos+Vector3.up*_footOffset,posDelta, out hit,posDelta.magnitude+0.001f,_bird._collisionLayer)){
				Vector3 castDir = _velocity>0 ? Vector3.up : Vector3.down;
				//check collision along velocity
				if(Physics.Raycast(startPos+Vector3.up*_footOffset,castDir, out hit,Mathf.Abs(posDelta.y)+0.001f,_bird._collisionLayer)){
					//check collision along vert
					transform.position=hit.point;
					CompleteHop(hit.transform);
				}
				else//vertical is free
					transform.position=startPos+Vector3.up*posDelta.y;
			}
			_hopTimer+=Time.deltaTime;
		}
    }

	public Vector3 GetCamTarget(){
		return _camTarget;
	}

	public float GetHopAngle(){
		return Mathf.Atan2(transform.forward.z,transform.forward.x);
	}

	public void HopTo(Vector3 target){
		_destination=target;
		RaycastHit hit;
		if(Physics.Raycast(_destination+Vector3.up*1f,Vector3.down, out hit,1f,_bird._collisionLayer)){
			_destination.y=hit.point.y;
		}
		Vector3 diff = _destination-transform.position;
		diff.y=0;
		diff.Normalize();
		_npcInput=diff;
	}

	public bool Arrived(float threshold){
		return (transform.position-_destination).sqrMagnitude<threshold*threshold;
	}

	public void StopHopping(){
		//transform.position=_destination;
		_npcInput=Vector3.zero;
	}

	public bool IsHopping(){
		return _hopping;
	}

	public void FinishCurrentHop(){
		//_disableAfterHop=true;
	}

	public void PlayStepParticles(){
		Instantiate(_stepParts,transform.position,Quaternion.identity);
	}

	public void KnockBack(Vector3 dir){
		_anim.SetFloat("hopTime",-1f/_hopTime);
		_knockBack=true;
		Vector3 reflected = Vector3.Reflect(_input,dir)*_knockBackMult;
		//make sure reflected is more than 90 degrees away from original
		float dot = Vector3.Dot(_input.normalized,reflected.normalized);
		if(dot>0)
			_input=-_input*_knockBackMult;
		else
			_input=reflected;
		dot = Vector3.Dot(dir,Vector3.down);
		if(dot>=_dotToKillVert){
			_velocity=0;
		}
		//Debug.Break();
	}

	void CompleteHop(Transform ground){
		_hopping=false;
		_firstHop=false;
		_camTarget=transform.position;

		//sfx + vfx
		if(_hopTimer>_hopCancelWindow){
			_footstep=ground.GetComponent<Footstep>();
			float vol=_diving? 1f : -1f;
			if(_footstep!=null)
			{
				_footstep.Sound(transform.position,vol);
			}
			if(!_diving){
				PlayStepParticles();
				_bird.MakeFootprint(ground);
				_bird.MakeFootprint(ground,0.01f);
				_bird.SaveLastSpot();
			}
			else{
				_bird.Dive(0.5f);
				_soarAudio.Stop();
			}
		}
		
		//re-calibrate if npc
		if(_npc)
			HopTo(_destination);

		if(_knockBack){
			_bird.ShakeItOff();
		}

		transform.localScale=_defaultScale;
		Vector3 eulers=_mesh.localEulerAngles;
		eulers.y=_defaultAngle;
		_mesh.localEulerAngles=eulers;
	}

	void StartHop(){
		_hopping=true;
		_velocity=_hopAccel;
		_hopTime=2f*_hopAccel/_gravity;
		_anim.SetTrigger("resetWalk");
		_anim.SetFloat("hopTime",1f/_hopTime);
			//_anim.SetBool("hop",true);

		_hopStartY=transform.position.y;
		_hopTimer=0;
		_hopStartPos=transform.position;
		_hopStartRot=transform.rotation;

		_hopAudio.pitch=Random.Range(0.8f,1.2f);
		_hopAudio.Play();
	}

	public void ResetScale(){
		_defaultScale=transform.localScale;
		_squashScale=_defaultScale;
		_squashScale.y*=_squashMult;
		_stretchScale=_defaultScale;
		_stretchScale.y*=_stretchMult;

	}

	void OnDrawGizmos(){
		Gizmos.color=Color.green;
		Gizmos.DrawLine(transform.position,transform.position+_input.normalized);
		/*
		Gizmos.color=Color.green;
		Gizmos.DrawSphere(_hopStart,0.05f);
		Gizmos.color=Color.red;
		Gizmos.DrawSphere(_hopTarget,0.05f);
		*/
	}
}
