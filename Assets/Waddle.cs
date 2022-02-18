using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waddle : MonoBehaviour
{
	Animator _anim;
	MInput _mIn;
	public float _walkSpeed;
	Bird _bird;
	public float _maxWalkSlope;
	public float _animSpeedMult;
	public float _stepVolume;
	float _stepTimer;
	Vector3 _input;
	public float _inputSmoothLerp;
	public float _slerp;
	public float _minInput;
	public float _minSlopeToCheckSpeed;
	float _knockBackTimer;
	[Header("Knock back")]
	public float _knockBackTime;
	Vector3 _knockBackDir;
	public float _knockBackSpeedMult;
	Collider [] _cols;

	//npc
	Terrain _terrain;
	Vector3 _destination;
	public bool _npc;
	Vector3 _npcInput;
	float _timeEstimate;
	float _walkTimer;

	WaddleCam _cam;

	void Awake(){
		_anim=GetComponent<Animator>();
		_mIn = GameManager._mIn;
		_bird = GetComponent<Bird>();
		_cols = new Collider[2];
		_cam=transform.GetComponentInChildren<WaddleCam>();
	}

	void OnEnable(){
		_anim.SetFloat("walkSpeed",0.1f);

		//ground at start
		RaycastHit hit;
		if(Physics.Raycast(transform.position+Vector3.up*0.01f,Vector3.down,out hit, 0.02f,_bird._collisionLayer)){
			transform.position=hit.point;
			if(hit.transform.GetComponent<Footstep>()!=null)
				TakeStep(hit.transform);
		}
		_stepTimer=0;
		_input=_mIn.GetInputDir();
		if(!_npc)
		{
			//mCam -> transition(bird's waddle cam, transitions.lerp, 0, null, 1)
			//GameManager._mCam.Transition(_bird._waddleCam,MCamera.Transitions.LERP,0,null,1f);
			GameManager._mCam.Transition(_bird._waddleCam,MCamera.Transitions.CUT_BACK);
			_cam.enabled=true;
			//GameManager._mCam.SnapToCamera(_bird._waddleCam);
		}
	}

	void OnDisable(){
		if(!_npc)
		{
			_cam.enabled=false;
			GameManager._mCam.Transition(_bird._idleCam,MCamera.Transitions.CUT_BACK);
			_bird.Idle();
		}
	}
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
		_stepTimer+=Time.deltaTime;
		_walkTimer+=Time.deltaTime;
		if(_knockBackTimer<=0){
			Vector3 rawInput = _npc? _npcInput : _mIn.GetInputDir();
			//if(_input.sqrMagnitude<=rawInput.sqrMagnitude)
			_input=Vector3.Lerp(_input,rawInput,_inputSmoothLerp*Time.deltaTime);
			//_input=rawInput;
			if(_input.sqrMagnitude<_minInput*_minInput)
				return;
		}
		else{
			_knockBackTimer-=Time.deltaTime;
			if(_knockBackTimer<=0){
				_bird.ShakeItOff();
				return;
			}
		}
		float animSpeed=_input.magnitude*_animSpeedMult;
		Vector3 move=Vector3.zero;
		if(_knockBackTimer<=0)
		{
			_anim.SetFloat("hopTime",animSpeed);
			Quaternion curRot=transform.rotation;
			transform.forward=_input;
			Quaternion endRot=transform.rotation;
			transform.rotation=Quaternion.Slerp(curRot,endRot,_slerp*Time.deltaTime);
			move=transform.forward*_input.magnitude*Time.deltaTime*_walkSpeed*Mathf.Max(0,Vector3.Dot(transform.forward,_input));
		}
		else
		{
			_anim.SetFloat("hopTime",-animSpeed);
			//move=-transform.forward*_input.magnitude*Time.deltaTime*_walkSpeed*2;
			move=_input*Time.deltaTime*_walkSpeed;
		}

		Vector3 targetPos = transform.position+move;

		RaycastHit hit;
		if(Physics.Raycast(targetPos+Vector3.up*_bird._size.y,Vector3.down,out hit, _bird._size.y*1.1f,_bird._collisionLayer)){
			//raycast to ground
			targetPos.y=hit.point.y;
			float dy = (targetPos.y-transform.position.y);
			float dx = move.magnitude;
			float slope = dy/dx;
			if(slope<-_maxWalkSlope*0.5f){
				if(dx>0){
					/*
					_anim.SetFloat("walkSpeed",0f);
					Debug.Log("Should hop "+dy +"/"+dx);
					_bird.StartHopping();
					*/
					_bird.Ground();
				}
			}
			else if(slope<_maxWalkSlope)
			{
				if(slope>_minSlopeToCheckSpeed){
					Vector3 dir=hit.point-transform.position;
					targetPos=transform.position+dir.normalized*_walkSpeed*Time.deltaTime;
					if(Physics.Raycast(targetPos+Vector3.up*_bird._size.y,Vector3.down,out hit, _bird._size.y*1.5f,_bird._collisionLayer)){
						transform.position=hit.point;
					}
				}
				else
					transform.position=hit.point;
			}
			if(_stepTimer>=0.5f/animSpeed){
				TakeStep(hit.transform);
				_stepTimer=0;
			}
			_anim.SetFloat("walkSpeed",0.1f);

			if(_npc && Arrived(_bird._arriveRadius))
				StopWaddling();
		}
		else{
			_anim.SetFloat("walkSpeed",0f);
			Debug.Log("Should hop");
			_bird.StartHopping();
		}
    }

	void TakeStep(Transform t){
		Footstep f = t.GetComponent<Footstep>();
		if(f!=null)
			f.Sound(transform.position,_stepVolume);
		_bird.MakeFootprint(t);
		if(_npc){
			//recalibrate
			float mag = _npcInput.magnitude;
			Vector3 diff = _destination-transform.position;
			diff.y=0;
			diff.Normalize();
			_npcInput=diff*mag;
		}
	}

	public bool IsWaddling(){
		return _input.sqrMagnitude>=_minInput*_minInput;
	}
	
	public bool IsKnockBack(){
		return _knockBackTimer>0;
	}

	public void KnockBack(Vector3 dir){
		//_knockBackDir=dir;
		_knockBackTimer=_knockBackTime;
		float mag = _input.magnitude;
		_input=dir*mag*_knockBackSpeedMult;
		//transform.forward=-dir;
	}

	public void WaddleTo(Vector3 target,float speed){
		_destination=target;
		RaycastHit hit;
		if(Physics.Raycast(_destination+Vector3.up*_bird._size.y*0.5f,Vector3.down, out hit,1f,_bird._collisionLayer)){
			_destination.y=hit.point.y;
		}
		Vector3 diff = _destination-transform.position;
		diff.y=0;
		diff.Normalize();
		_npcInput=diff*speed;
		_timeEstimate=(_destination-transform.position).magnitude/(_npcInput.magnitude*_walkSpeed);
		//Debug.Log("te: "+_timeEstimate);
		_walkTimer=0;
	}

	public bool Arrived(float threshold){
		float sqrDst=(transform.position-_destination).sqrMagnitude;
		//Debug.Log("sqrDst: "+sqrDst);
		bool closeEnough=sqrDst<threshold*threshold;
		//bool timeOut=_walkTimer>_timeEstimate+0.5f;
		return  closeEnough;// || timeOut ;
	}

	public void StopWaddling(){
		//transform.position=_destination;
		_npcInput=Vector3.zero;
		_anim.SetFloat("walkSpeed",0f);
		enabled=false;
	}

	public void ToggleCamLines(){
		_cam.ToggleCamLines();
	}

	void OnDrawGizmos(){
		if(_destination!=null)
		{
			Gizmos.color=Color.red;
			Gizmos.DrawWireSphere(_destination,0.1f);
		}
	}
}
