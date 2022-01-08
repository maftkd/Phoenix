﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waddle : MonoBehaviour
{
	Animator _anim;
	MCamera _mCam;
	public float _walkSpeed;
	Bird _bird;
	public float _maxWalkSlope;
	public float _animSpeedMult;
	public float _stepVolume;
	float _stepTimer;
	Vector3 _input;
	public float _inputSmoothLerp;
	public float _minInput;
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

	void Awake(){
		_anim=GetComponent<Animator>();
		_mCam=FindObjectOfType<MCamera>();
		_bird = GetComponent<Bird>();
		_cols = new Collider[2];
	}

	void OnEnable(){
		_anim.SetFloat("walkSpeed",0.1f);

		//ground at start
		RaycastHit hit;
		if(Physics.Raycast(transform.position+Vector3.up*0.01f,Vector3.down,out hit, 0.02f,_bird._collisionLayer)){
			transform.position=hit.point;
			if(hit.transform.GetComponent<Footstep>()!=null)
				TakeStep(hit.transform.GetComponent<Footstep>());
		}
		_stepTimer=0;
		_input=_mCam.GetInputDir();
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
			Vector3 rawInput = _npc? _npcInput : _mCam.GetInputDir();
			_input=Vector3.Lerp(_input,rawInput,_inputSmoothLerp*Time.deltaTime);
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
			transform.forward=_input;
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
		if(Physics.Raycast(targetPos+Vector3.up*_bird._size.y,Vector3.down,out hit, _bird._size.y*1.5f,_bird._collisionLayer)){
			//raycast to ground
			targetPos.y=hit.point.y;
			float dy = (targetPos.y-transform.position.y);
			float dx = move.magnitude;
			float slope = dy/dx;
			if(slope<_maxWalkSlope)
				transform.position=hit.point;
			if(_stepTimer>=0.5f/animSpeed){
				TakeStep(hit.transform.GetComponent<Footstep>());
				_stepTimer=0;
			}
			_anim.SetFloat("walkSpeed",0.1f);
		}
		else{
			_anim.SetFloat("walkSpeed",0f);
		}
    }

	void TakeStep(Footstep f){
		if(f!=null)
			f.Sound(transform.position,_stepVolume);
		_bird.MakeFootprint();
		if(_npc){
			//recalibrate
			float mag = _npcInput.magnitude;
			Vector3 diff = _destination-transform.position;
			diff.y=0;
			diff.Normalize();
			_npcInput=diff*mag;
			Debug.Log("recalibrating");
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
		if(Physics.Raycast(_destination+Vector3.up*1f,Vector3.down, out hit,1f,_bird._collisionLayer)){
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
		bool closeEnough=(transform.position-_destination).sqrMagnitude<threshold*threshold;
		//bool timeOut=_walkTimer>_timeEstimate+0.5f;
		return  closeEnough;// || timeOut ;
	}
	public void StopWaddling(){
		//transform.position=_destination;
		_npcInput=Vector3.zero;
	}

}
