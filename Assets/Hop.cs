using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hop : MonoBehaviour
{
	[Range(0f,1f)]
	public float _inThresh;
	public float _hopDist;
	public float _hopSpeed;
	public float _hopHeight;
	public float _hopHeightRandom;
	public float _turnLerp;
	float _hopTarget;
	float _hopStart;
	float _midPos;
	Vector3 _camTarget;
	float _hopTimer;
	float _hopTime;
	Footstep _footstep;
	Collider [] _cols;
	Animator _anim;
	MCamera _mCam;
	public Transform _stepParts;
	bool _disableAfterHop;
	Bird _bird;

	//ai hopping
	Terrain _terrain;
	Vector3 _destination;
	public bool _npc;
	Vector3 _npcInput;

	void Awake(){
		_cols = new Collider[2];
		_hopTime=_hopDist/_hopSpeed;
		_camTarget=transform.position;
		_anim=GetComponent<Animator>();
		_anim.SetFloat("hopTime",1f/_hopTime);
		_mCam=FindObjectOfType<MCamera>();
		_bird=GetComponent<Bird>();
		_terrain=FindObjectOfType<Terrain>();
	}
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
		Vector3 input=_npc ? _npcInput : _mCam.GetInputDir();
		if(input.sqrMagnitude>0){
			Quaternion curRot=transform.rotation;
			transform.forward=input;
			Quaternion targetRot=transform.rotation;
			transform.rotation=Quaternion.Slerp(curRot,targetRot,_turnLerp*Time.deltaTime);
			transform.position+=transform.forward*_hopSpeed*Time.deltaTime;

			if(_hopTimer<=0){
				Vector3 rayStart=transform.position+Vector3.up*_bird._size.y*2;
				rayStart+=transform.forward*_hopDist;
				RaycastHit hit;
				if(Physics.Raycast(rayStart,Vector3.down, out hit, _bird._size.y*4f,1)){
					//check for good ground spot
					if(Physics.OverlapSphereNonAlloc(hit.point+Vector3.up*_bird._size.y,0.01f,_cols,1)>0){
						//make sure no walls are in the way
						Debug.Log("wobble");
					}
					else{
						_hopTarget=hit.point.y;
						_hopTimer=_hopTime;
						_hopStart=transform.position.y;
						//_midPos=Vector3.Lerp(_hopStart,_hopTarget,0.5f);
						//_midPos+=Vector3.up*(_hopHeight+_hopHeightRandom*(Random.value*2-1));
						_midPos=Mathf.Lerp(_hopStart,_hopTarget,0.5f)+(_hopHeight+_hopHeightRandom*(Random.value*2-1));
						_footstep=hit.transform.GetComponent<Footstep>();
						_anim.SetBool("hop",true);
					}
				}
				else{
				}
			}
		}

		if(_hopTimer>0)
		{
			float t = 1f-_hopTimer/_hopTime;
			float y=0;
			if(t<0.5f)//first half of hop
				y=Mathf.Lerp(_hopStart,_midPos,t*2f);
			else//second half of hop
				y=Mathf.Lerp(_midPos,_hopTarget,(t-0.5f)*2f);
			Vector3 pos = transform.position;
			pos.y=y;
			transform.position=pos;
			_camTarget=transform.position;
			_camTarget.y=Mathf.Lerp(_hopStart,_hopTarget,t);

			_hopTimer-=Time.deltaTime;
			if(_hopTimer<=0){
				//end of hop
				pos=transform.position;
				pos.y=_hopTarget;
				transform.position=pos;
				_camTarget=transform.position;

				//footstep audio
				if(_footstep!=null)
					_footstep.Sound(transform.position);
				//transform.eulerAngles=Vector3.zero;

				//fx
				_anim.SetBool("hop",false);
				Instantiate(_stepParts,transform.position,Quaternion.identity);
				if(_disableAfterHop){
					_disableAfterHop=false;
					enabled=false;
				}
			}
		}
		else{
			//not hopping
		}

		if(!_npc&&Input.GetButtonDown("Jump")){
			Debug.Log("Wants to jump");
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
		_destination.y=_terrain.SampleHeight(target);
		Vector3 diff = _destination-transform.position;
		diff.y=0;
		diff.Normalize();
		_npcInput=diff;
	}

	public bool Arrived(float threshold){
		return (transform.position-_destination).sqrMagnitude<threshold*threshold;
	}

	public void StopHopping(){
		_npcInput=Vector3.zero;
		_anim.SetBool("hop",false);
	}

	public bool IsHopping(){
		return _hopTimer>0;
	}

	public void FinishCurrentHop(){
		_disableAfterHop=true;
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
