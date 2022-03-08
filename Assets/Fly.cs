using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fly : MonoBehaviour
{
	bool _init;
	Collider[] _cols;
	public AudioClip _flapSound;
	[HideInInspector]
	public Vector3 _velocity;
	float _gravity;
	public float _defaultGravity;
	public float _soarGravity;
	public float _minSoarVely;
	public float _minDefaultVely;
	float _minVely;
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
	MInput _mIn;
	public float _maxRoll;
	public float _soarMaxRoll;
	public float _defaultMaxRoll;
	public float _minTurnRadius;
	float _turnRadius;
	[Tooltip("Scales down the roll angle before computing turn radius")]//allows larger turn radii
	public float _rollMult;
	public float _soarRollMult;
	public float _defaultRollMult;
	public float _pitchMult;
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
	public bool _soaring;
	public float _diveVolume;
	public float _divePitch;
	public ParticleSystem _flapParts;
	float _speedFrac;
	[Header("Knockback")]
	public float _knockBackDelay;
	public float _knockBackMult;
	public float _minKnockBackMag;
	float _knockBackTimer;

	//shadow
	Transform _flyShadow;
	Transform _sun;
	bool _prevShowShadow;

	void Awake(){
		_mIn=GameManager._mIn;
		_cols=new Collider[4];
		_bird=GetComponent<Bird>();
		_anim=GetComponent<Animator>();
		_soarParticles=transform.Find("SoarParticles").GetComponent<ParticleSystem>();
		_soarAudio=_soarParticles.GetComponent<AudioSource>();
	}

	void OnEnable(){
		_velocity=Vector3.zero;

		//_curFlapAccel=_flapAccel;
		_curFlapAccel=Vector3.zero;
		Vector3 input=_mIn.GetControllerInput();
		_curFlapAccel+=transform.forward*_flapAccel.z*input.magnitude+Vector3.up*_flapAccel.y;

		//initial velocity
		_velocity=_curFlapAccel;

		_speedFrac=0;

		_knockBackTimer=0;

		//zero out horizontal component of flap after initial flap
		_curFlapAccel=Vector3.up*_flapAccel.y;

		_flapTimer=0;
		_flapCounter=0;

		_forwardness=0;
		_diving=false;
		_anim.ResetTrigger("soar");

		Flap();
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		_flapTimer+=Time.deltaTime;
		//flap
		if(_mIn.GetJumpDown()){
			if(_flapCounter<_numFlaps){
				_knockBackTimer=0f;//can reset knockback by flapping
				_velocity+=_curFlapAccel;
				_flapTimer=0;
				_anim.SetTrigger("fly");
				Soar(false);
				Flap();
				_diving=false;
			}
		}

		_anim.SetBool("soar",_mIn.GetJump());
		if(_flapTimer>_flapDur){
			Soar(_mIn.GetJump());
			_diving=!_mIn.GetJump();
		}

		//add air control
		Vector3 input=_mIn.GetControllerInput();

		if(_knockBackTimer<=0)
		{
			//air control if not getting knocked back
			//forward/backward air control
			//_velocity+=transform.forward*input.y*_airControl.z*Time.deltaTime;

			Vector3 flatVel=_velocity;
			flatVel.y=0;

			//turning mid-air
			_rollMult=_mIn.GetJump() ? _soarRollMult : _defaultRollMult;
			_maxRoll=_mIn.GetJump() ? _soarMaxRoll : _defaultMaxRoll;
			_forwardness = Vector3.Dot(flatVel.normalized,transform.forward);
			float rollAngle=-_maxRoll*input.x;
			_speedFrac=flatVel.magnitude/_maxVel.z;
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


			//cap velocity
			if(flatVel.sqrMagnitude>_maxVel.z*_maxVel.z){
				flatVel=flatVel.normalized*_maxVel.z;
				_velocity.x=flatVel.x;
				_velocity.z=flatVel.z;
			}
			
			//adjust pitch
			Vector3 eulerAngles=transform.eulerAngles;
			eulerAngles.z=rollAngle;
			float targetPitch=-_velocity.y*_pitchMult;
			eulerAngles.x=targetPitch;
			transform.eulerAngles=eulerAngles;
		}


		//apply physics
		Vector3 prevPos=transform.position;
		transform.position+=_velocity*Time.deltaTime;
		_gravity=_mIn.GetJump()&&_velocity.y<0? _soarGravity : _defaultGravity;
		_velocity+=Vector3.down*_gravity*Time.deltaTime;

		//cap vertical velocity
		if(_velocity.y>_maxVel.y)
			_velocity.y=_maxVel.y;

		_minVely=_mIn.GetJump()?_minSoarVely : _minDefaultVely;
		if(_velocity.y<_minVely)
			_velocity.y=_minVely;
		

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
			float vol = _diving? 1f : 0.1f;
			if(footstep!=null)
				footstep.Sound(_groundPoint,vol);
			_bird.Land();
		}

		//update kb timer
		if(_knockBackTimer>0){
			_knockBackTimer+=Time.deltaTime;
			if(_knockBackTimer>_knockBackDelay)
				_knockBackTimer=0;
		}

		//checking enabled because bird script may have disabled already from Land or Dive
		if(enabled){
			bool showShadow=false;
			if(Physics.Raycast(transform.position,_sun.forward, out hit, 50f, _bird._oceanLayer)){
				if(hit.transform.name=="Ocean")
				{
					showShadow=true;
					_flyShadow.position=hit.point;
					_flyShadow.rotation=Quaternion.identity;
				}
			}
			if(showShadow!=_prevShowShadow){
				_flyShadow.gameObject.SetActive(showShadow);
			}
			_prevShowShadow=showShadow;
		}
    }

	void Flap(){
		FlapEffects();
		_flapCounter++;
	}

	public void FlapEffects(){
		Instantiate(_flapParts,transform.position+_bird._size.y*Vector3.up,Quaternion.identity);
		FlapSounds();
	}

	public void FlapSounds(){
		Sfx.PlayOneShot3D(_flapSound,transform.position,Random.Range(_flapPitchRange.x,_flapPitchRange.y),_flapVolume);
	}

	public void Soar(bool soaring){
		_soaring=soaring;
		if(soaring)
		{
			if(!_soarParticles.isPlaying)
				_soarParticles.Play();
			if(!_soarAudio.isPlaying){
				_soarAudio.pitch=1.0f;
				_soarAudio.Play();
				_soarAudio.volume=_soarVolume;
			}
		}
		else{
			_soarParticles.Stop();
			_soarAudio.Stop();
		}
		_anim.SetBool("soar",soaring);
	}

	public float GetSpeedFraction(){
		return _speedFrac;
	}

	public void KnockBack(Vector3 dir){
		Debug.Log("Fly getting knocked back");
		_knockBackTimer+=Time.deltaTime;
		_velocity.x*=-_knockBackMult;
		_velocity.z*=-_knockBackMult;
		Vector2 kb =new Vector2(_velocity.x,_velocity.z);
		float mag = kb.magnitude;
		if(mag<_minKnockBackMag){
			kb=kb.normalized*_minKnockBackMag;
		}
		_velocity.x=kb.x;
		_velocity.z=kb.y;
	}

	public void SetupFlyShadow(){
		_flyShadow=transform.Find("FlyShadow");
		if(_flyShadow==null)
			return;
		_flyShadow.gameObject.SetActive(false);
		_sun=GameObject.FindGameObjectWithTag("Sun").transform;
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
