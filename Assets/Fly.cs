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
	public Vector3 _maxVel;
	Vector3 _maxVelOverride;
	public Vector3 _flapAccel;//z=forward,y=up
	public float _airResistance;
	Vector3 _curFlapAccel;
	float _flapTimer;
	public float _flapDur;
	int _flapCounter;
	public int _numFlaps;
	[Tooltip("The vertical boost gained if jump button is held down during a flap")]
	public float _flapHoldBoost;
	Bird _bird;
	MInput _mIn;
	[Header("Turning")]
	[Tooltip("Set via script")]
	public float _maxRoll;
	public float _soarMaxRoll;
	public float _defaultMaxRoll;
	public float _minTurnRadius;
	float _turnRadius;
	[Tooltip("Scales down the roll angle before computing turn radius")]//allows larger turn radii
	public float _rollMult;
	public float _rollChangeMult;
	float _rollAngle;
	public float _soarRollMult;
	public float _defaultRollMult;
	public float _turnSpeed;
	[Header("Pitch")]
	public float _maxAoa;
	[Tooltip("Multiply by maxAoa to get max pitch when inclined downward")]
	public float _downPitchMult;
	[Tooltip("How quickly pitch responds to vertical input")]
	public float _angleChangeMult;
	[Tooltip("How quickly pitch resets to zero when vertical input is low")]
	public float _angleFallMult;
	float _aoa;
	[Header("Lift")]
	public float _maxLift;
	public float _maxWeight;
	[Header("Thrust")]
	public float _maxThrust;
	public float _maxDrag;
	float _lift;
	float _thrust;
	Animator _anim;
	[HideInInspector]
	public float _forwardness;
	bool _diving;
	[Header("Audio")]
	public Vector2 _flapPitchRange;
	public float _flapVolume;
	ParticleSystem _soarParticles;
	public float _maxSoarPartsRate;
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
	//Conservation
	float _prevSqrMag;

	//shadow
	Transform _flyShadow;
	Transform _sun;
	bool _prevShowShadow;

	//land target
	Transform _landTarget;
	Material _landMat;
	float _maxDist = 0.75f;

	//stamina outline
	public Color _fullColor;
	public Color _emptyColor;
	Material _mat;

	void Awake(){
		_mIn=GameManager._mIn;
		_cols=new Collider[4];
		_bird=GetComponent<Bird>();
		_anim=GetComponent<Animator>();
		_soarParticles=transform.Find("SoarParticles").GetComponent<ParticleSystem>();
		_soarAudio=_soarParticles.GetComponent<AudioSource>();
		_landTarget=transform.Find("LandTarget");
		if(_landTarget!=null)
		{
			_landTarget.gameObject.SetActive(false);
			_landMat=_landTarget.GetComponent<Renderer>().material;
		}
	}

	void OnEnable(){
		_velocity=Vector3.zero;

		_curFlapAccel=Vector3.up*_flapAccel.y;

		//initial velocity
		_velocity=_curFlapAccel;
		_velocity+=transform.forward*_bird.GetVel();

		_speedFrac=0;

		_knockBackTimer=0;
		_prevSqrMag=0;

		_aoa=0;
		_lift=0;

		_flapTimer=0;
		_flapCounter=0;

		_forwardness=0;
		//_diving=false;
		_anim.ResetTrigger("soar");
		_rollAngle=0f;

		Flap(false);

		_mat = _bird.GetMaterial();
		_mat.SetColor("_RimColor",_fullColor);
		_landTarget.gameObject.SetActive(true);
	}

	void OnDisable(){
		//Soar(false);
		_mat.SetColor("_RimColor",Color.black);
		Soar(false);
		_anim.SetTrigger("land");
		_landTarget.gameObject.SetActive(false);
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		_flapTimer+=Time.deltaTime;
		Vector3 flatForward=transform.forward;
		flatForward.y=0;
		//flap
		if(_mIn.GetJumpDown()&&_flapTimer>_flapDur){
			if(_flapCounter<_numFlaps){
				_knockBackTimer=0f;//can reset knockback by flapping
				//forwardness depends on aoa
				float aoa01=((_aoa/_maxAoa)+1)*0.5f;
				//DebugScreen.Print("aoa01: ",aoa01,0);
				//_curFlapAccel=flatForward*_flapAccel.z*aoa01;
				_curFlapAccel=transform.forward*_flapAccel.z;
				//upness is uniform
				//_curFlapAccel+=transform.up*_flapAccel.y;
				//if going forward, add proportionate vel boost
				//_velocity.y+=_flapAccel.y*(1-aoa01);
				//add vertical when tilted back
				_curFlapAccel+=Vector3.up*_flapAccel.y;
				_flapTimer=0;
				_anim.SetTrigger("fly");
				Soar(false);
				Flap();
				//_diving=false;
			}
		}

		_soaring=_flapTimer>_flapDur;
		if(_soaring){
			Soar(_soaring);
		}
		else{
			_velocity+=_curFlapAccel*Time.deltaTime;
		}
		_anim.SetBool("soar",_soaring);

		//add air control
		Vector3 input=_mIn.GetControllerInput();
		Vector3 flatVel=_velocity;

		if(_knockBackTimer<=0)
		{
			//pitch control
			flatForward=transform.forward;
			flatForward.y=0;
			flatForward.Normalize();
			float prevAoa=_aoa;;
			_aoa+=input.y*Time.deltaTime*_angleChangeMult;
			if(input.y==0)
				_aoa=Mathf.Lerp(_aoa,0,_angleFallMult*Time.deltaTime);
			_aoa = Mathf.Clamp(_aoa,-_maxAoa,_maxAoa);
			float aoaDiff=_aoa-prevAoa;
			DebugScreen.Print("Aoa diff: "+aoaDiff.ToString("0.000"));
			_velocity=Quaternion.Euler(aoaDiff*transform.right)*_velocity;

			if(_aoa>0)
			{
				_thrust=Mathf.InverseLerp(0,_maxAoa,_aoa)*_maxThrust;
				_lift=Mathf.InverseLerp(0,_maxAoa,_aoa)*_maxWeight;
			}
			else if(_aoa<0)
			{
				//_velocity+=flatForward*Time.deltaTime*_aoa*_dragMult;
				float mult=Mathf.InverseLerp(0,_maxAoa,-_aoa);
				_thrust=mult*_maxDrag;
				_lift=mult*_maxLift;
			}
			else{
				_thrust=0;
				_lift=0;
			}
			flatVel=_velocity;
			flatVel.y=0;
			
			//cap velocity
			if(flatVel.sqrMagnitude>_maxVel.z*_maxVel.z){
				flatVel=flatVel.normalized*_maxVel.z;
				_velocity.x=flatVel.x;
				_velocity.z=flatVel.z;
			}
			_forwardness = Vector3.Dot(flatVel.normalized,transform.forward);


			//turning mid-air
			_rollMult=_soaring ? _soarRollMult : _defaultRollMult;
			_maxRoll=_soaring ? _soarMaxRoll : _defaultMaxRoll;
			_rollAngle=Mathf.Lerp(_rollAngle,-_maxRoll*input.x,_rollChangeMult*Time.deltaTime);
			_speedFrac=flatVel.magnitude/_maxVel.z;

			//rotate velocity
			if(flatVel.sqrMagnitude>0.01f){
				flatVel = Quaternion.Euler(0f,input.x*Time.deltaTime*_turnSpeed,0)*flatVel;
				_velocity.x=flatVel.x;
				_velocity.z=flatVel.z;
				//rotate transform
				float dt = Vector3.Dot(flatVel.normalized,transform.forward);
				if(dt>0.5f)
					transform.forward = flatVel.normalized;
			}
			else{
				flatForward = Quaternion.Euler(0f,input.x*Time.deltaTime*_turnSpeed,0)*flatForward;
				//rotate transform
				transform.forward = flatForward;
			}

			//adjust pitch
			Vector3 eulerAngles=transform.eulerAngles;
			eulerAngles.z=_rollAngle;
			float targetPitch=_aoa;
			if(_aoa>0)
				targetPitch*=_downPitchMult;
			eulerAngles.x=targetPitch;
			transform.eulerAngles=eulerAngles;
		}


		//apply physics
		Vector3 prevPos=transform.position;
		transform.position+=_velocity*Time.deltaTime;
		float normVel=flatVel.magnitude/_maxVel.z;
		float gLerp=1-normVel;
		_gravity=Mathf.Lerp(_soarGravity,_defaultGravity,gLerp);
		_velocity-=flatForward*_airResistance*Time.deltaTime;
		_velocity-=Vector3.up*_gravity*Time.deltaTime;
		_velocity+=Vector3.up*_lift*Time.deltaTime;
		if(_thrust>0)
			_velocity+=transform.forward*_thrust*Time.deltaTime;

		flatVel=_velocity;
		flatVel.y=0;

		//cap vertical velocity
		if(_velocity.y>_maxVel.y)
		{
			_velocity.y=_maxVel.y;
		}

		//prevent backwards flight
		if(_forwardness<=0){
			_velocity.x*=-1;
			_velocity.z*=-1;
		}

		_prevSqrMag=_velocity.sqrMagnitude;
		
		//debugging
		DebugScreen.Print("Aoa: "+_aoa.ToString("0.000"));
		/*
		DebugScreen.Print("Thrust: "+_thrust.ToString("0.000"));
		DebugScreen.Print("Lift: "+_lift.ToString("0.000"));
		*/
		DebugScreen.Print("flat vel mag: "+flatVel.magnitude.ToString("0.000"));
		DebugScreen.Print("gravity: "+_gravity.ToString("0.000"));


		if(_soaring){
			var emission = _soarParticles.emission;
			emission.rateOverTime = normVel*_maxSoarPartsRate;
		}

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
			//float vol = _diving? 1f : 0.1f;
			float vol = 1f;
			if(footstep!=null)
			{
				footstep.Sound(hit.point,vol);
			}
			_bird.Land();
			_flyShadow.gameObject.SetActive(false);
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

			//check for land target
			if(Physics.Raycast(transform.position,Vector3.down, out hit, 50f, _bird._collisionLayer)){
				float sqrDst = (transform.position-hit.point).sqrMagnitude;
				_landTarget.position=hit.point;
				Vector3 eulers = _landTarget.eulerAngles;
				eulers.x=0;
				eulers.z=0;
				_landTarget.eulerAngles=eulers;
				float frac=sqrDst/(_maxDist*_maxDist);
				frac=Mathf.Clamp(frac,0.15f,1f);
				_landMat.SetFloat("_Radius",frac);
			}
			else{
				//_landTarget.gameObject.
			}
		}
    }

	void Flap(bool countFlap=true){
		FlapEffects();
		if(countFlap)
		{
			_flapCounter++;
			float hFull;
			float sFull;
			float vFull;
			float hEmpty;
			float sEmpty;
			float vEmpty;
			Color.RGBToHSV(_fullColor,out hFull, out sFull, out vFull);
			Color.RGBToHSV(_emptyColor,out hEmpty, out sEmpty, out vEmpty);
			float frac=_flapCounter/(float)_numFlaps;
			Color newCol = Color.HSVToRGB(Mathf.Lerp(hFull,hEmpty,frac),Mathf.Lerp(sFull,sEmpty,frac),Mathf.Lerp(vFull,vEmpty,frac));
			_mat.SetColor("_RimColor",newCol);
		}
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
		//Debug.Log("Fly getting knocked back");
		_knockBackTimer+=Time.deltaTime;
		float vely = _velocity.y;
		_velocity*=-_knockBackMult;
		_velocity.y=vely;
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

	public void AddForce(Vector3 v){
		_velocity+=v*Time.deltaTime;
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.red;
		Gizmos.color=Color.blue;
		Vector3 right=transform.right;
		right.y=0;
		right.Normalize();
		Gizmos.DrawSphere(transform.position+right*_turnRadius,0.02f);
	}
}
