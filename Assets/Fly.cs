using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fly : MonoBehaviour
{
	public Vector2 _flapAccel;
	Vector3 _curFlapAccel;
	public float _flapHoldBoost;
	Vector3 _velocity;
	public float _gravity;
	Collider [] _cols;
	bool _init;
	public Vector2 _airControl;
	public Vector2 _maxVel;
	public float _flapDur;
	float _flapTimer;
	
	void OnEnable(){
		_velocity=Vector3.zero;
		_curFlapAccel=_flapAccel;
		float z=transform.eulerAngles.z;
		if(z>180)
			z=-(360-z);
		else if(z<-180)
			z=(360+z);
		
		_curFlapAccel.x*=-z/45f;
		//initial velocity
		_velocity=_curFlapAccel;

		//zero out horizontal component of flap after initial flap
		_curFlapAccel.x=0;

		_flapTimer=0;

		if(!_init){
			Init();
		}
	}

	void Init(){
		_cols=new Collider[4];
		_init=true;
	}

    // Update is called once per frame
    void Update()
    {
		//flaps
		if(Input.GetKeyDown(KeyCode.Space)){
			_velocity+=_curFlapAccel;
			_flapTimer=0;
		}
		if(_flapTimer<_flapDur&&Input.GetKey(KeyCode.Space)){
			//_velocity+=_curFlapAccel*Time.deltaTime;
			_velocity.y+=_curFlapAccel.y*Time.deltaTime*_flapHoldBoost;
			_flapTimer+=Time.deltaTime;
		}
		float horIn = Input.GetAxis("Horizontal");
		float vertIn = Input.GetAxis("Vertical");
		//add air control
		_velocity.x+=horIn*_airControl.x*Time.deltaTime;
		if(vertIn<0)
			_velocity.y+=vertIn*_airControl.y*Time.deltaTime;

		//cap velocity
		if(_velocity.x>_maxVel.x)
			_velocity.x=_maxVel.x;
		else if(_velocity.x<-_maxVel.x)
			_velocity.x=-_maxVel.x;

		transform.position+=_velocity*Time.deltaTime;
		_velocity+=Vector3.down*_gravity*Time.deltaTime;
		if(Physics.OverlapSphereNonAlloc(transform.position,0.01f,_cols)>0){
			//hit something
			GetComponent<Hop>().enabled=true;
			enabled=false;
		}
    }
}
