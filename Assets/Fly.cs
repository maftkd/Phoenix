using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fly : MonoBehaviour
{
	bool _init;
	Collider[] _cols;
	AudioSource[] _flapSounds;
	[HideInInspector]
	public Vector3 _velocity;
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
	public float _minTurnRadius;
	float _turnRadius;
	[Tooltip("Scales down the roll angle before computing turn radius")]//allows larger turn radii
	public float _rollMult;
	public Vector3 _airControl;
	Animator _anim;
	[HideInInspector]
	public float _forwardness;
	bool _diving;
	public Vector2 _flapPitchRange;
	public float _flapVolume;
	ParticleSystem _soarParticles;
	AudioSource _soarAudio;
	public float _soarVolume;
	public float _diveVolume;
	public float _divePitch;
	public ParticleSystem _flapParts;

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

		_forwardness=0;
		_diving=false;
		_anim.ResetTrigger("soar");

		Flap();
	}

	void Init(){
		_cols=new Collider[4];
		_flapSounds=transform.Find("FlapSounds").GetComponentsInChildren<AudioSource>();
		_bird=GetComponent<Bird>();
		_anim=GetComponent<Animator>();
		_mCam=FindObjectOfType<MCamera>();
		_soarParticles=transform.Find("SoarParticles").GetComponent<ParticleSystem>();
		_soarAudio=_soarParticles.GetComponent<AudioSource>();
		_init=true;
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		//flap
		if(Input.GetButtonDown("Jump")){
			if(_flapCounter<_numFlaps){
				_velocity+=_curFlapAccel;
				_flapTimer=0;
				_anim.SetTrigger("fly");
				_anim.ResetTrigger("soar");
				Soar(false);
				Flap();
			}
		}
		if(Input.GetButton("Jump")){
			if(_flapTimer<_flapDur){
				_velocity.y+=_curFlapAccel.y*Time.deltaTime*_flapHoldBoost;
				_flapTimer+=Time.deltaTime;
				if(_flapTimer>=_flapDur){
					//soar
					_anim.SetTrigger("soar");
					Soar(true);
				}
			}
		}
		if(Input.GetButtonUp("Jump")){
			_flapTimer=_flapDur;
			//soar
			_anim.SetTrigger("soar");
			Soar(true);
		}

		//dive
		if(Input.GetButtonDown("Dive")){
			Debug.Log("Dive time");
			_flapTimer=_flapDur;
			_anim.ResetTrigger("soar");
			_anim.SetTrigger("dive");
			Soar(false);
			_diving=true;
			_soarAudio.pitch=_divePitch;
			_soarAudio.volume=_diveVolume;
			_soarAudio.Play();
		}
		if(Input.GetButton("Dive")){
			_velocity.y-=_airControl.y*Time.deltaTime;
		}
		if(Input.GetButtonUp("Dive")){
			//back to soaring
			_anim.SetTrigger("soar");
			Soar(true);
			_diving=false;
		}

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
		//forward/backward air control
		_velocity+=transform.forward*vertIn*_airControl.z*Time.deltaTime;

		//turning mid-air
		Vector3 flatVel=_velocity;
		flatVel.y=0;
		_forwardness = Vector3.Dot(flatVel.normalized,transform.forward);
		float rollAngle=-_maxRoll*horIn;
		if(_forwardness>0){
			//if flying forward
			float tan = Mathf.Tan(rollAngle*_rollMult*Mathf.Deg2Rad);
			if(tan!=0&&!float.IsNaN(tan)&&!float.IsInfinity(tan)&&!float.IsNegativeInfinity(tan)){
				_turnRadius = -flatVel.sqrMagnitude/(11.26f*tan);
				if(Mathf.Abs(_turnRadius)>_minTurnRadius){
					//make sure turn radius isn't too small or else we get the dizzys
					float rawMag=flatVel.magnitude;
					float frameDistance = rawMag*Time.deltaTime;
					//calculate rotation arc
					float circumference = _turnRadius*2f*Mathf.PI;
					float arc = frameDistance/(circumference);
					//rotate velocity
					flatVel = Quaternion.Euler(0f,arc*360f,0)*flatVel;
					_velocity.x=flatVel.x;
					_velocity.z=flatVel.z;
					//rotate transform
					transform.forward = flatVel.normalized;
				}
			}
		}
		else{
			rollAngle=0;
		}
		Vector3 eulerAngles=transform.eulerAngles;
		eulerAngles.z=rollAngle;
		float targetPitch=0;
		if(_diving)
			targetPitch=-Mathf.Atan2(_velocity.y,flatVel.magnitude)*Mathf.Rad2Deg;
		/*
		float x=eulerAngles.x;
		if(x>180)
			x=-(360-x);
		else if(x<-180)
			x=(360+x);
		eulerAngles.x=Mathf.Lerp(x,targetPitch,Time.deltaTime*_pitchLerp);
			*/
		eulerAngles.x=targetPitch;
		transform.eulerAngles=eulerAngles;

		//cap velocity
		if(_velocity.x>_maxVel.x)
			_velocity.x=_maxVel.x;
		else if(_velocity.x<-_maxVel.x)
			_velocity.x=-_maxVel.x;

		//apply physics
		Vector3 prevPos=transform.position;
		transform.position+=_velocity*Time.deltaTime;
		_velocity+=Vector3.down*_gravity*Time.deltaTime;

		Vector3 ray = transform.position-prevPos;
		RaycastHit hit;
		if(Physics.Raycast(prevPos,ray,out hit, ray.magnitude+0.01f,_bird._collisionLayer)){
			transform.position=prevPos;
			Vector3 eulers = transform.eulerAngles;
			eulers.z=0;
			eulers.x=0;
			transform.eulerAngles=eulers;
			Soar(false);
			Footstep footstep=hit.transform.GetComponent<Footstep>();
			float vel = -_velocity.y/_maxVel.y;
			float vol = _diving? 1f : vel;
			if(footstep!=null)
				footstep.Sound(_groundPoint,vol);
			if(!_diving)
				_bird.Land();
			else
				_bird.Dive(vel);
		}
    }

	void Flap(){
		Instantiate(_flapParts,transform.position+_bird._size.y*Vector3.up,Quaternion.identity);
		_flapCounter++;
		foreach(AudioSource a in _flapSounds){
			if(!a.isPlaying){
				a.transform.position=transform.position;
				a.Play();
				a.pitch=Random.Range(_flapPitchRange.x,_flapPitchRange.y);
				a.volume=_flapVolume;
				return;
			}
		}
	}

	void Soar(bool soaring){
		if(soaring)
		{
			_soarParticles.Play();
			_soarAudio.pitch=1.0f;
			_soarAudio.Play();
			_soarAudio.volume=_soarVolume;
		}
		else{
			_soarParticles.Stop();
			_soarAudio.Stop();
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
