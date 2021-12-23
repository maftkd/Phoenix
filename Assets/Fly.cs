using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fly : MonoBehaviour
{
	bool _init;
	Collider[] _cols;
	AudioSource[] _flapSounds;
	Vector3 _velocity;
	public float _gravity;
	public Vector3 _maxVel;
	public Vector3 _flapAccel;//z=forward,y=up
	Vector3 _curFlapAccel;
	float _flapTimer;
	public float _flapDur;
	int _flapCounter;
	public int _numFlaps;
	[Tooltip("The vertical boost gained if jump button is held down during a flap")]
	public float _flapHoldBoost;
	Bird _bird;
	Vector3 _groundPoint;
	MCamera _mCam;
	public float _maxRoll;
	[Tooltip("magnitude of velocity must be greater than this to turn")]//prevents turn radii from being too small
	public float _minVelocityToTurn;
	[Tooltip("Min Vertical axis to turn. Allows backward movement without initiating turns")]
	public float _minVertInputToTurn;
	float _turnRadius;
	[Tooltip("Scales down the roll angle before computing turn radius")]//allows larger turn radii
	public float _rollMult;
	public Vector3 _airControl;
	Animator _anim;

	void OnEnable(){
		if(!_init)
			Init();

		_velocity=Vector3.zero;

		//_curFlapAccel=_flapAccel;
		_curFlapAccel=Vector3.zero;
		Vector3 input=_mCam.GetControllerInput();
		_curFlapAccel+=transform.forward*_flapAccel.z*input.magnitude+Vector3.up*_flapAccel.y;

		//initial velocity
		_velocity=_curFlapAccel;

		//zero out horizontal component of flap after initial flap
		_curFlapAccel=Vector3.up*_flapAccel.y;

		_flapTimer=0;
		_flapCounter=0;

		Flap();
	}

	void Init(){
		_cols=new Collider[4];
		_flapSounds=transform.Find("FlapSounds").GetComponentsInChildren<AudioSource>();
		_bird=GetComponent<Bird>();
		_anim=GetComponent<Animator>();
		_mCam=FindObjectOfType<MCamera>();
		_init=true;
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		//flaps
		if(Input.GetButtonDown("Jump")){
			//_lastJumpPress=Time.time;
			if(_flapCounter<_numFlaps){
				_velocity+=_curFlapAccel;
				_flapTimer=0;
				_anim.SetTrigger("fly");
				Flap();
			}
		}
		if(_flapTimer<_flapDur){
			if(Input.GetButton("Jump")){
				_velocity.y+=_curFlapAccel.y*Time.deltaTime*_flapHoldBoost;
				_flapTimer+=Time.deltaTime;
				if(_flapTimer>=_flapDur){
					//soar
					_anim.SetTrigger("soar");
				}
			}
			else if(Input.GetButtonUp("Jump")){
				_flapTimer=_flapDur;
				//soar
				_anim.SetTrigger("soar");
			}
		}
		/*
		if(_flapTimer<_flapDur&&Input.GetButton("Jump")){
			_velocity.y+=_curFlapAccel.y*Time.deltaTime*_flapHoldBoost;
			_flapTimer+=Time.deltaTime;
		}
		*/

		/*
		//check perch
		if(Input.GetButtonDown(GameManager._perchButton)){
			if(Physics.OverlapSphereNonAlloc(transform.position,0.01f,_cols,_perchMask)>0){
				_rustle.Play();
				GetComponent<Hop>().enabled=true;
				enabled=false;
			}
		}
		*/
		
		//add air control
		float horIn = Input.GetAxis("Horizontal");
		float vertIn = Input.GetAxis("Vertical");
		_velocity+=transform.forward*vertIn*_airControl.z*Time.deltaTime;
		//_velocity.x+=horIn*_airControl.x*Time.deltaTime;
			/*
		if(vertIn<0)
			_velocity.y+=vertIn*_airControl.y*Time.deltaTime;
			*/

		//calc turn radius
		Vector3 flatVel=_velocity;
		flatVel.y=0;
		float rollAngle=-_maxRoll*horIn;
		//scale roll angle - with rollMult=1, shits cray
		float tan = Mathf.Tan(rollAngle*_rollMult*Mathf.Deg2Rad);
		if(vertIn>_minVertInputToTurn&&tan!=0&&!float.IsNaN(tan)&&!float.IsInfinity(tan)&&!float.IsNegativeInfinity(tan)){
			_turnRadius = -flatVel.sqrMagnitude/(11.26f*tan);
			float rawMag=flatVel.magnitude;
			float mag = rawMag*Time.deltaTime;
			if(rawMag>_minVelocityToTurn){
				//calculate rotation arc
				float circumference = _turnRadius*2f*Mathf.PI;
				float arc = mag/(circumference);
				//rotate velocity
				flatVel = Quaternion.Euler(0f,arc*360f,0)*flatVel;
				_velocity.x=flatVel.x;
				_velocity.z=flatVel.z;
				//rotate transform
				transform.forward = flatVel.normalized;
			}
		}
		Vector3 eulerAngles=transform.eulerAngles;
		eulerAngles.z=rollAngle;
		transform.eulerAngles=eulerAngles;

		//cap velocity
		if(_velocity.x>_maxVel.x)
			_velocity.x=_maxVel.x;
		else if(_velocity.x<-_maxVel.x)
			_velocity.x=-_maxVel.x;

		//apply physics
		transform.position+=_velocity*Time.deltaTime;
		_velocity+=Vector3.down*_gravity*Time.deltaTime;
		//transform.eulerAngles=Vector3.back*45f*horIn;

		//collision detection
		RaycastHit hit;
		if(_velocity.y<=0&&Physics.Raycast(transform.position+Vector3.up*_bird._size.y,Vector3.down,out hit, _bird._size.y, 1)){
			//ground check
			if(!hit.transform.GetComponent<Collider>().isTrigger)
			{
				_groundPoint=hit.point;
				//GetComponent<Hop>().enabled=true;
				//enabled=false;
				transform.position=_groundPoint;
				Vector3 eulers = transform.eulerAngles;
				eulers.z=0;
				transform.eulerAngles=eulers;
				//Debug.Log("setting pos: "+_groundPoint);
				Footstep footstep=hit.transform.GetComponent<Footstep>();
				if(footstep!=null)
					footstep.Sound(_groundPoint);
				_bird.Land();
			}
		}
		else{
			if(Physics.Raycast(transform.position,Vector3.up,out hit, _bird._size.y, 1)){
				//ceiling check
				if(!hit.transform.GetComponent<Collider>().isTrigger)
				{
					if(_velocity.y>0)
					{
						_velocity.y=0;
						//_thonk.Play();
						Debug.Log("thonk");
					}
					_flapTimer=_flapDur;
				}
			}
			else{
				if(Physics.Raycast(transform.position+Vector3.up*_bird._size.y,Vector3.right*_velocity.x,out hit, _bird._size.y, 1)){
					//side wall check
					if(!hit.transform.GetComponent<Collider>().isTrigger)
					{
						_velocity.x=0;
						/*
						if(!_thonk.isPlaying)
							_thonk.Play();
							*/
						Debug.Log("thonk");
						_flapTimer=_flapDur;
						//undo last velocity
						transform.position-=Vector3.right*_velocity.x*Time.deltaTime;
					}
				}
			}
		}
        
    }

	void Flap(){
		_flapCounter++;
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
			Gizmos.DrawSphere(_groundPoint,0.02f);
		Gizmos.color=Color.blue;
		Vector3 right=transform.right;
		right.y=0;
		right.Normalize();
		Gizmos.DrawSphere(transform.position+right*_turnRadius,0.02f);
	}
}
