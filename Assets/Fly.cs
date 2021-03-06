using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fly : MonoBehaviour
{
	bool _init;
	Collider[] _cols;
	public AudioClip _flapSound;
	public AudioClip _flapBackSound;
	[HideInInspector]
	public Vector3 _velocity;
	public float _gravityBoost;
	public float _gravity;
	//public float _defaultGravity;
	public float _maxVel;
	public float _maxFlapVel;
	public float _flapAccel;
	public float _flapAccelUp;
	public float _flapDeccel;
	public float _flapDeccelDown;
	public float _initialVelBoost;
	public float _airResistance;
	bool _airResistEnabled=true;
	Vector3 _curFlapAccel;
	float _flapTimer;
	public float _flapDur;
	int _flapCounter;
	public int _numFlaps;
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
	public float _pitchSpeed;
	public float _upPitchSpeed;
	public float _maxPhi;
	public float _maxUpPhi;
	[Tooltip("Multiply by maxAoa to get max pitch when inclined downward")]
	public float _downPitchMult;
	[Tooltip("How quickly pitch responds to vertical input")]
	public float _angleChangeMult;
	[Tooltip("How quickly pitch resets to zero when vertical input is low")]
	public float _angleFallMult;
	float _aoa;
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
	float _prevMag;

	//ground effects
	public float _maxDist=5f;
	//water
	Material _oceanMat;
	Vector4 _playerPos;
	[Header ("Effects")]
	public Transform _waterSprayPrefab;
	Transform _waterSpray;
	ParticleSystem _sprayParts;
	ParticleSystem.EmissionModule _sprayEmission;
	Fader _spraySound;
	TrailRenderer _sprayTrail;
	//sand
	public Transform _sandSprayPrefab;
	Transform _sandSpray;
	ParticleSystem _sandParts;
	ParticleSystem.EmissionModule _sandEmission;
	Fader _sandSound;
	//grass
	public Transform _grassSprayPrefab;
	Transform _grassSpray;
	ParticleSystem _grassParts;
	ParticleSystem.EmissionModule _grassEmission;
	Fader _grassSound;
	//rocks right
	public Transform _rockSprayPrefab;
	Transform _rockSpray;
	ParticleSystem _rockParts;
	ParticleSystem.EmissionModule _rockEmission;
	Fader _rockSound;
	TrailRenderer _rockTrail;

	//stamina outline
	[Header("Outline")]
	public Color _fullColor;
	public Color _emptyColor;
	//Material _mat;

	MCamera _mCam;
	public Transform _windLinesPrefab;
	Transform _windLines;
	ParticleSystem _windParts;
	Fader _windSound;
	ParticleSystem.EmissionModule _windEmission;

	//boundary
	//GameObject _boundary;

	void Awake(){
		_mIn=GameManager._mIn;
		_mCam=GameManager._mCam;
		_cols=new Collider[4];
		_bird=GetComponent<Bird>();
		_anim=GetComponent<Animator>();
		_soarParticles=transform.Find("SoarParticles").GetComponent<ParticleSystem>();
		_soarAudio=_soarParticles.GetComponent<AudioSource>();

		//setup water spray
		_waterSpray=Instantiate(_waterSprayPrefab,transform);
		_sprayParts=_waterSpray.GetComponent<ParticleSystem>();
		_sprayParts.Stop();
		_sprayEmission=_sprayParts.emission;
		_spraySound=_waterSpray.GetComponent<Fader>();
		_sprayTrail=_waterSpray.GetComponentInChildren<TrailRenderer>();
		_sprayTrail.emitting=false;
		//setup sand spray
		_sandSpray=Instantiate(_sandSprayPrefab,transform);
		_sandParts=_sandSpray.GetComponent<ParticleSystem>();
		_sandEmission=_sandParts.emission;
		_sandParts.Stop();
		AudioSource sandAudio=_sandSpray.gameObject.AddComponent<AudioSource>();
		sandAudio.loop=true;
		sandAudio.spatialBlend=1f;
		sandAudio.clip=Synthesizer.GenerateSineWave(220f,10f,0.5f,0.2f,0.4f);
		sandAudio.Stop();
		_sandSound=_sandSpray.GetComponent<Fader>();
		//setup grass spray
		_grassSpray=Instantiate(_grassSprayPrefab,transform);
		_grassParts=_grassSpray.GetComponent<ParticleSystem>();
		_grassSound=_grassSpray.GetComponent<Fader>();
		_grassEmission=_grassParts.emission;
		//setup rock spray
		_rockSpray=Instantiate(_rockSprayPrefab,transform);
		_rockParts=_rockSpray.GetComponent<ParticleSystem>();
		_rockSound=_rockSpray.GetComponent<Fader>();
		_rockEmission=_rockParts.emission;
		_rockTrail=_rockSpray.GetComponentInChildren<TrailRenderer>();
		_rockTrail.emitting=false;

		//setup wind lines
		_windLines=Instantiate(_windLinesPrefab,transform);
		_windLines.SetParent(_mCam.transform);
		_windLines.localPosition=Vector3.zero;
		_windLines.localEulerAngles=Vector3.zero;
		_windParts=_windLines.GetComponent<ParticleSystem>();
		_windSound=_windLines.GetComponent<Fader>();
		_windEmission=_windParts.emission;

		//boundary
		//_boundary=transform.Find("Boundary").gameObject;
		//_boundary.SetActive(false);
		_oceanMat=FindObjectOfType<Ocean>().GetComponent<Renderer>().material;
		_playerPos=Vector4.zero;
		Shader.SetGlobalVector("_PlayerPos",_playerPos);
	}

	void OnEnable(){
		_velocity=Vector3.zero;

		_curFlapAccel=Vector3.up*_flapAccel*_initialVelBoost;
		_curFlapAccel+=transform.forward*_bird.GetVel();//*_flapAccel.z;
		_velocity=(transform.forward+transform.up).normalized;

		_speedFrac=0;
		_knockBackTimer=0;
		_prevMag=0;
		_aoa=0;
		_flapTimer=0;
		_flapCounter=0;
		_forwardness=0;
		_anim.SetBool("soar",false);
		_rollAngle=0f;

		Flap();

		//_mat = _bird.GetMaterial();
	}

	void OnDisable(){
		//Soar(false);
		//_mat.SetColor("_RimColor",Color.black);
		Soar(false);
		_anim.SetTrigger("land");
		//_boundary.SetActive(false);
		_mCam.SetFovFrac(0);
		Reset();
		//_landTarget.gameObject.SetActive(false);
		//_mCam.SetVignette(0);
		_playerPos=Vector4.zero;
		//_oceanMat.SetVector("_PlayerPos",_playerPos);
		Shader.SetGlobalVector("_PlayerPos",_playerPos);
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
		if(_mIn.GetJumpDown()){
			if(_flapCounter<_numFlaps){
				_knockBackTimer=0f;//can reset knockback by flapping
				//_curFlapAccel=transform.up*_flapAccel;
				//_curFlapAccel=(transform.up+transform.forward)*_flapAccel;
				_curFlapAccel=transform.forward*_flapAccel;
				_curFlapAccel+=transform.up*_flapAccelUp;
				//_curFlapAccel=(transform.up+transform.forward*0.5f)*_flapAccel;
				_flapTimer=0;
				_anim.SetTrigger("fly");
				Soar(false);
				Flap();
			}
		}
		else if(_mIn.GetLandDown()){
			if(_flapCounter<_numFlaps){
				_knockBackTimer=0f;//can reset knockback by flapping
				_curFlapAccel=-transform.forward*_flapDeccel;
				//_curFlapAccel+=transform.up*_flapDeccel*0.25f;
				_curFlapAccel+=Vector3.down*_flapDeccelDown;
				_flapTimer=0;
				_anim.SetTrigger("flyBack");
				Soar(false);
				Flap(false);
			}

		}

		_soaring=_flapTimer>_flapDur;
		if(_soaring){
			Soar(_soaring);
		}
		else{
			float curMag = _velocity.magnitude;
			_velocity+=_curFlapAccel*Time.deltaTime;
			if(_velocity.magnitude>_maxFlapVel)
			{
				if(curMag<_maxFlapVel)//if we've just accelerated past the max vel
					_velocity=_velocity.normalized*_maxFlapVel;
				else
				{
					//if we are over the max flap vel, and trying to increase
					if(_velocity.magnitude>=_prevMag)
						_velocity=_velocity.normalized*curMag;
				}
			}
		}
		_anim.SetBool("soar",_soaring);

		//add air control
		Vector3 input=_mIn.GetControllerInput();
		Vector3 flatVel=_velocity;
		flatVel.y=0;

		if(_knockBackTimer<=0)
		{

			//pitch control
			flatForward=transform.forward;
			flatForward.y=0;
			flatForward.Normalize();
			float prevAoa=_aoa;
			float y01=1-(input.y+1)*0.5f;//inverted
			_aoa=Mathf.Lerp(-_maxAoa,_maxAoa,y01);
			//_aoa = Mathf.Clamp(_aoa,-_maxAoa,_maxAoa);

			//rotate velocity
			if(_soaring){
				Vector3 prevVel=_velocity;
				if(_aoa>0)
				{
					_velocity=Quaternion.Euler(_aoa*transform.right*_pitchSpeed*Time.deltaTime)*_velocity;
					_velocity+=_velocity.normalized*_gravityBoost*Time.deltaTime;
				}
				else if(_aoa<0)
				{
					_velocity=Quaternion.Euler(_aoa*transform.right*_upPitchSpeed*Time.deltaTime)*_velocity;
					//_velocity-=_velocity.normalized*1*Time.deltaTime;
				}
				//level out
				else
				{
					Vector3 targetVel=flatForward*_prevMag;
					_velocity=Vector3.Lerp(_velocity,targetVel,Time.deltaTime);
					_velocity=_velocity.normalized*_prevMag;
					//Debug.Log("yoyoyo");
				}
				flatVel=_velocity;
				flatVel.y=0;
				float yVel=_velocity.y;
				float phi=Mathf.Atan2(yVel,flatVel.magnitude);
				//cap aoa
				if(phi<-_maxPhi&&_aoa>0||phi>_maxUpPhi&&_aoa<0)
				{
					_velocity=prevVel.normalized*_velocity.magnitude;
				}
				flatVel=_velocity;
				flatVel.y=0;
			}
			
			_forwardness = Vector3.Dot(flatVel.normalized,transform.forward);

			//turning mid-air
			_rollMult=_soaring ? _soarRollMult : _defaultRollMult;
			_maxRoll=_soaring ? _soarMaxRoll : _defaultMaxRoll;
			_rollAngle=Mathf.Lerp(_rollAngle,-_maxRoll*input.x,_rollChangeMult*Time.deltaTime);
			_speedFrac=_velocity.magnitude/_maxVel;

			//adjust pitch
			Vector3 eulerAngles=transform.eulerAngles;
			eulerAngles.z=_rollAngle;
			float targetPitch=_aoa;
			eulerAngles.x=targetPitch;
			transform.eulerAngles=eulerAngles;

			//turn
			float rotateAmount=_rollAngle*_turnSpeed*Time.deltaTime;
			transform.Rotate(-Vector3.up*rotateAmount,Space.World);
			float mag = flatVel.magnitude;
			float theta=Mathf.Atan2(transform.forward.z,transform.forward.x);

			flatVel.x=Mathf.Cos(theta)*mag;
			flatVel.z=Mathf.Sin(theta)*mag;
			_velocity.x=flatVel.x;
			_velocity.z=flatVel.z;
		}

		flatForward=transform.forward;
		flatForward.y=0;

		//cap velocity
		if(_velocity.magnitude>_maxVel){
			_velocity=_velocity.normalized*_maxVel;
		}

		//apply physics
		Vector3 prevPos=transform.position;
		transform.position+=_velocity*Time.deltaTime;
		float normVel=_velocity.magnitude/_maxVel;
		//float gLerp=1-normVel;
		//_gravity=_defaultGravity;
		if(Input.GetKeyDown(KeyCode.F3))
			_airResistEnabled=!_airResistEnabled;
		if(_airResistEnabled)
			_velocity-=flatForward.normalized*_airResistance*Time.deltaTime;
		else
			DebugScreen.Print("Air Resistance off");
		_velocity-=Vector3.up*_gravity*Time.deltaTime;
		_mCam.SetFovFrac(normVel);

		flatVel=_velocity;
		flatVel.y=0;


		//prevent backwards flight
		if(_forwardness<=0){
			_velocity.x=0f;
			_velocity.z=0f;
		}

		float velMag=_velocity.magnitude;
		//DebugScreen.Print(velMag.ToString("0.000"));
		float accel=(velMag-_prevMag)/Time.deltaTime;
		bool boosted=normVel>0.2f;
		if(boosted&&!_windSound.IsOn()){
			_windSound.Play();
			_windParts.Play();
		}
		else if(!boosted&&_windSound.IsOn()){
			_windSound.Stop();
			_windParts.Stop();
		}
		if(_windParts.isPlaying){
			//_windLines.rotation=Quaternion.identity;
			_windEmission.rateOverTime=normVel*25f;
			_windSound.SetTarget(normVel);
		}
		//_windSound.SetTarget(normVel);

		_prevMag=_velocity.magnitude;
		
		//debugging
		//DebugScreen.Print("Velocity: "+flatVel.magnitude.ToString("0.000"));
		//DebugScreen.Print("Accel: "+accel.ToString("0.000"));
		//DebugScreen.Print("flat vel mag: "+flatVel.magnitude.ToString("0.000"));
		//DebugScreen.Print("y vel: "+_velocity.y.ToString("0.000"));


		if(_soaring){
			var emission = _soarParticles.emission;
			emission.rateOverTime = normVel*_maxSoarPartsRate;
		}

		Vector3 ray = transform.position-prevPos;
		RaycastHit hit;
		if(Physics.Raycast(prevPos,ray,out hit, ray.magnitude+0.01f,_bird._collisionLayer)){
			//don't land when hitting trees - landing causes trees to switch to triggers which interferes with
			//trees knockback behaviour which requires them to remain colliders
			if(hit.transform.GetComponent<Tree>()==null){
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
			}
		}

		//update kb timer
		if(_knockBackTimer>0){
			_knockBackTimer+=Time.deltaTime;
			if(_knockBackTimer>_knockBackDelay)
				_knockBackTimer=0;
		}

		//checking enabled because bird script may have disabled already from Land or Dive
		if(enabled){

			_playerPos.x=transform.position.x;
			_playerPos.y=transform.position.y;
			_playerPos.z=transform.position.z;
			_playerPos.w=_prevMag/_maxVel;
			//_oceanMat.SetVector("_PlayerPos",_playerPos);
			Shader.SetGlobalVector("_PlayerPos",_playerPos);

			//raycast down 
			Vector3 hitPoint=Vector3.zero;
			if(Physics.Raycast(transform.position,Vector3.down, out hit, 50f, _bird._collisionLayer)){
				hitPoint=hit.point;
				Terrain t = hit.transform.GetComponent<Terrain>();
				//change of terrain
				if(hitPoint.y<5f)
					_bird.SetTerrain(null);
				else
				{
					_bird.SetTerrain(t);
				}
				//_bird.SetCardTerrain(t);

			}
			else{
				_bird.SetTerrain(null);
			}

			//determine which fx to play
			bool water=false;
			if (hitPoint.y<=5f&&transform.position.y<5f+_maxDist)
			{
				if(transform.position.y<5f){
					//under water
				}
				else
					water=true;
			}
			bool grass=false;
			bool sand=false;
			bool gravel=false;
			if(_bird._terrain!=null && water==false&&transform.position.y<hitPoint.y+_maxDist){
				int terrainIndex=_bird.GetTerrainTextureIndex();
				if(terrainIndex==0)
					sand=true;
				else if(terrainIndex==1||terrainIndex==3)
					gravel=true;
				else if(terrainIndex==2)
					grass=true;
			}

			if(water)
			{
				if(_bird._terrain==null)
					hitPoint=transform.position;
				hitPoint.y=5f;
			}
			float diff=transform.position.y-hitPoint.y;
			float frac=Mathf.Clamp01(diff/_maxDist);

			//water fx
			if(water&&!_spraySound.IsOn()){
				_sprayParts.Play();
				_spraySound.Play();
				//_sprayTrail.emitting=true;
			}
			else if(!water&&_spraySound.IsOn()){
				_sprayParts.Stop();
				_spraySound.Stop();
				//_sprayTrail.emitting=false;
			}
			if(_sprayParts.isPlaying){
				_sprayEmission.rateOverTime=500f*(1-frac);
				_spraySound.SetTarget(1-frac);
				//_sprayTrail.startColor=Color.white*(1-frac);
				//_sprayTrail.startWidth=0.25f*(1-frac);
				_waterSpray.position=hitPoint+Vector3.up*0.05f;
				_waterSpray.rotation=transform.rotation;//Quaternion.identity;
				Vector3 eulers = _waterSpray.eulerAngles;
				eulers.z=0;
				eulers.x=0;
				_waterSpray.eulerAngles=eulers;
			}

			//sand fx
			if(sand&&!_sandSound.IsOn())
			{
				_sandParts.Play();
				_sandSound.Play();
			}
			else if(!sand&&_sandSound.IsOn())
			{
				_sandParts.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
				_sandSound.Stop();
			}
			if(_sandParts.isPlaying){
				_sandSpray.position=hitPoint;
				_sandSpray.rotation=Quaternion.identity;
				_sandEmission.rateOverTime=200f*(1-frac);
				_sandSound.SetTarget((1-frac));
			}
			
			//gravel parts
			if(gravel&&!_rockSound.IsOn())
			{
				_rockParts.Play();
				_rockSound.Play();
				//_rockTrail.emitting=true;
			}
			else if(!gravel&&_rockSound.IsOn())
			{
				_rockParts.Stop();
				_rockSound.Stop();
				//_rockTrail.emitting=false;
			}
			if(_rockParts.isPlaying){
				_rockSpray.position=hit.point;
				_rockSpray.rotation=Quaternion.identity;
				_rockEmission.rateOverTime=100f*(1-frac);
				_rockSound.SetTarget((1-frac));
				//_rockTrail.startWidth=0.25f*(1-frac);
			}

			//grass fx
			if(grass&&!_grassSound.IsOn())
			{
				_grassParts.Play();
				_grassSound.Play();
			}
			else if(!grass&&_grassSound.IsOn())
			{
				_grassParts.Stop();
				_grassSound.Stop();
			}
			if(_grassParts.isPlaying){
				_grassSpray.position=hitPoint;
				_grassSpray.rotation=Quaternion.identity;
				_grassEmission.rateOverTime=200f*(1-frac);
				_grassSound.SetTarget((1-frac));
			}

			//_boundary.SetActive(!sand && !grass && !water);
			//_boundary.transform.rotation=Quaternion.identity;
		}
    }

	void Flap(bool forward=true){
		FlapEffects(forward);
		/*
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
			//_mat.SetColor("_RimColor",newCol);
			if(_flapCounter==_numFlaps)
				_mat.SetColor("_RimColor",_emptyColor);
		}
		*/
	}

	public void FlapEffects(bool forward){
		Instantiate(_flapParts,transform.position+_bird._size.y*Vector3.up,Quaternion.identity);
		FlapSounds(forward);
	}

	public void FlapSounds(bool forward){
		if(forward)
			Sfx.PlayOneShot3D(_flapSound,transform.position,Random.Range(_flapPitchRange.x,_flapPitchRange.y),_flapVolume);
		else
			Sfx.PlayOneShot3D(_flapBackSound,transform.position,Random.Range(_flapPitchRange.x,_flapPitchRange.y),_flapVolume);
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


	public void AddForce(Vector3 v){
		_velocity+=v*Time.deltaTime;
	}

	public void Reset(){
		_sprayParts.Stop();
		_spraySound.Stop();
		//_sandParts.Stop();
		_sandParts.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
		_sandSound.Stop();
		_grassParts.Stop();
		_grassSound.Stop();
		_rockParts.Stop();
		_rockSound.Stop();
		_sprayTrail.emitting=false;
		_rockTrail.emitting=false;
		_windParts.Stop();
		_windSound.Stop();
	}

	public bool IsFlapping(){
		return _flapTimer<_flapDur*2f;
	}

	public void BoostSpeed(float amount, float dur){
		//_boostAmount=amount;
		//_boostTimer=dur;
		_velocity+=transform.forward*amount;
	}

	public void KillVert(){
		_velocity.y=0;
	}

	void OnTriggerEnter(Collider other){
		if(other.tag=="Foliage"){
			Debug.Log("We can land in: "+other.name);
		}
	}

	void OnTriggerExit(Collider other){
		if(other.tag=="Foliage"){
			Debug.Log("We can no longer land in: "+other.name);
		}
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
