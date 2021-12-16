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
	AudioSource [] _flapSounds;
	Vector3 _groundPoint;
	AudioSource _thonk;
	[Header("Perch")]
	public LayerMask _perchMask;
	[HideInInspector]
	public bool _killVert;
	AudioSource _rustle;
	
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

		if(_killVert)
			_velocity.y=0;
		_killVert=false;

		if(!_init){
			Init();
		}

		Flap();
	}

	void Init(){
		_cols=new Collider[4];
		_flapSounds=transform.Find("FlapSounds").GetComponentsInChildren<AudioSource>();
		_thonk=transform.Find("Thonk").GetComponent<AudioSource>();
		_rustle=transform.Find("Rustle").GetComponent<AudioSource>();
		_init=true;
	}

    // Update is called once per frame
    void Update()
    {
		//flaps
		if(Input.GetButtonDown(GameManager._jumpButton)){
			_velocity+=_curFlapAccel;
			_flapTimer=0;
			Flap();
		}
		if(_flapTimer<_flapDur&&Input.GetButton(GameManager._jumpButton)){
			_velocity.y+=_curFlapAccel.y*Time.deltaTime*_flapHoldBoost;
			_flapTimer+=Time.deltaTime;
		}

		//check perch
		if(Input.GetButtonDown(GameManager._perchButton)){
			if(Physics.OverlapSphereNonAlloc(transform.position,0.01f,_cols,_perchMask)>0){
				_rustle.Play();
				GetComponent<Hop>().enabled=true;
				enabled=false;
			}
		}
		
		//add air control
		float horIn = Input.GetAxis("Horizontal");
		float vertIn = Input.GetAxis("Vertical");
		_velocity.x+=horIn*_airControl.x*Time.deltaTime;
		if(vertIn<0)
			_velocity.y+=vertIn*_airControl.y*Time.deltaTime;

		//cap velocity
		if(_velocity.x>_maxVel.x)
			_velocity.x=_maxVel.x;
		else if(_velocity.x<-_maxVel.x)
			_velocity.x=-_maxVel.x;

		//apply physics
		transform.position+=_velocity*Time.deltaTime;
		_velocity+=Vector3.down*_gravity*Time.deltaTime;
		transform.eulerAngles=Vector3.back*45f*horIn;

		//collision detection
		RaycastHit hit;
		if(Physics.Raycast(transform.position+Vector3.up*Bird._height,Vector3.down,out hit, Bird._height, 1)){
			//ground check
			_groundPoint=hit.point;
			GetComponent<Hop>().enabled=true;
			enabled=false;
			transform.position=_groundPoint;
		}
		else{
			if(Physics.Raycast(transform.position,Vector3.up,out hit, Bird._height, 1)){
				//ceiling check
				if(_velocity.y>0)
				{
					_velocity.y=0;
					_thonk.Play();
				}
				_flapTimer=_flapDur;
			}
			else{
				if(Physics.Raycast(transform.position+Vector3.up*Bird._height,Vector3.right*_velocity.x,out hit, Bird._width, 1)){
					//side wall check
					_velocity.x=0;
					if(!_thonk.isPlaying)
						_thonk.Play();
					_flapTimer=_flapDur;
					//undo last velocity
					transform.position-=Vector3.right*_velocity.x*Time.deltaTime;
				}
			}
		}
    }

	void Flap(){
		foreach(AudioSource a in _flapSounds){
			if(!a.isPlaying){
				a.transform.position=transform.position;
				a.Play();
				return;
			}
		}
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.red;
		if(_groundPoint!=null)
			Gizmos.DrawSphere(_groundPoint,0.2f);
	}
}
