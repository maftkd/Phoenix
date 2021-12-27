using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hop : MonoBehaviour
{
	Vector3 _camTarget;
	Footstep _footstep;
	Collider [] _cols;
	Animator _anim;
	MCamera _mCam;
	public Transform _stepParts;
	//bool _disableAfterHop;
	Bird _bird;
	bool _hopping;
	float _velocity;
	public float _hopAccel;
	public float _gravity;
	float _hopStartY;
	public float _airControl;
	public float _airTurnLerp;
	public float _hopVolumeMult;
	public float _hopCancelWindow;
	float _hopTimer;
	Vector3 _hopStartPos;
	Quaternion _hopStartRot;

	//ai hopping
	Terrain _terrain;
	Vector3 _destination;
	public bool _npc;
	Vector3 _npcInput;

	void Awake(){
		_cols = new Collider[2];
		_camTarget=transform.position;
		_anim=GetComponent<Animator>();
		float hopTime=2f*_hopAccel/_gravity;
		_anim.SetFloat("hopTime",1f/hopTime);
		_bird=GetComponent<Bird>();
		_terrain=FindObjectOfType<Terrain>();
	}

	void OnEnable(){
		_camTarget=transform.position;
	}

	void OnDisable(){
	}

    // Start is called before the first frame update
    void Start()
    {
		_mCam=FindObjectOfType<MCamera>();
    }

    // Update is called once per frame
    void Update()
    {
		Vector3 input=_npc ? _npcInput : _mCam.GetInputDir();
		if(!_hopping){
			if(input.sqrMagnitude>0){
				_hopping=true;
				_anim.SetBool("hop",true);
				_hopStartY=transform.position.y;
				_velocity=_hopAccel;
				_hopTimer=0;
				_hopStartPos=transform.position;
				_hopStartRot=transform.rotation;
			}
		}
		if(_hopping)
		{
			Vector3 startPos=transform.position;

			//rotation
			if(input.sqrMagnitude>0){
				Quaternion curRot=transform.rotation;
				transform.forward=input;
				Quaternion targetRot=transform.rotation;
				transform.rotation=Quaternion.Slerp(curRot,targetRot,_airTurnLerp*Time.deltaTime);
			}
			else if(_hopTimer<_hopCancelWindow){
				//hop cancel
				transform.position=_hopStartPos;
				transform.rotation=_hopStartRot;
				_hopping=false;
				_anim.SetBool("hop",false);
				_camTarget=transform.position;
				return;
			}
		
			//air control
			transform.position+=transform.forward*input.magnitude*Time.deltaTime*_airControl;

			//apply physics
			transform.position+=_velocity*Vector3.up*Time.deltaTime;
			_velocity-=_gravity*Time.deltaTime;
			_camTarget=transform.position;
			if(_camTarget.y>_hopStartPos.y)
				_camTarget.y=_hopStartY;

			//hit detection
			Vector3 posDelta=transform.position-startPos;
			Vector3 rayStart=startPos-posDelta;//start back?
			RaycastHit hit;
			if(Physics.Raycast(rayStart,posDelta, out hit,posDelta.magnitude*2f+0.001f,1)){
				transform.position=hit.point;
				_hopping=false;
				_camTarget=transform.position;

				//sfx + vfx
				if(_hopTimer>_hopCancelWindow){
					_footstep=hit.transform.GetComponent<Footstep>();
					if(_footstep!=null)
						_footstep.Sound(transform.position,_hopVolumeMult);
					PlayStepParticles();
				}
				//anim stuff
				_anim.SetBool("hop",false);
				
				//re-calibrate if npc
				if(_npc)
					HopTo(_destination);
				
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
		//#replace - gotta use a ray-cast because the map may consist of non-terrain elements
		//_destination.y=_terrain.SampleHeight(target);
		RaycastHit hit;
		if(Physics.Raycast(_destination+Vector3.up*1f,Vector3.down, out hit,1f,1)){
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
		transform.position=_destination;
		/*
		Vector3 pos = transform.position;
		pos.y=_destination.y;
		transform.position=pos;
		*/
		_npcInput=Vector3.zero;
		_anim.SetBool("hop",false);
	}

	public bool IsHopping(){
		//return _hopTimer>0;
		return _hopping;
	}

	public void FinishCurrentHop(){
		//_disableAfterHop=true;
	}

	public void PlayStepParticles(){
		Instantiate(_stepParts,transform.position,Quaternion.identity);
	}

	void OnDrawGizmos(){
		/*
		Gizmos.color=Color.green;
		Gizmos.DrawSphere(_hopStart,0.05f);
		Gizmos.color=Color.red;
		Gizmos.DrawSphere(_hopTarget,0.05f);
		*/
	}
}
