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
	public float _flapAccel;//z=forward,y=up
	public float _flapDeccel;//z=forward,y=up
	public float _flapDeccelDown;//z=forward,y=up
	public float _initialVelBoost;
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
	float _prevSqrMag;

	//shadow
	Transform _flyShadow;
	Transform _sun;
	bool _prevShowShadow;

	//ground effects
	const float _maxDist=3f;
	const float _maxDistRocks=3f;
	public Terrain _terrain;
	float[,,] _alphaMaps;
	TerrainData _terrainData;
	//water
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
	Transform _rocksRight;
	Transform _rocksLeft;
	ParticleSystem _rockPartsRight;
	ParticleSystem _rockPartsLeft;
	ParticleSystem.EmissionModule _rockEmissionRight;
	ParticleSystem.EmissionModule _rockEmissionLeft;
	Fader _rockSoundRight;
	Fader _rockSoundLeft;
	TrailRenderer _rockTrailRight;
	TrailRenderer _rockTrailLeft;

	//stamina outline
	[Header("Outline")]
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
		_rocksRight=Instantiate(_rockSprayPrefab,transform);
		_rockPartsRight=_rocksRight.GetComponent<ParticleSystem>();
		_rockSoundRight=_rocksRight.GetComponent<Fader>();
		_rockEmissionRight=_rockPartsRight.emission;
		_rocksLeft=Instantiate(_rockSprayPrefab,transform);
		_rockPartsLeft=_rocksLeft.GetComponent<ParticleSystem>();
		_rockSoundLeft=_rocksLeft.GetComponent<Fader>();
		_rockEmissionLeft=_rockPartsLeft.emission;
		_rockTrailLeft=_rocksLeft.GetComponentInChildren<TrailRenderer>();
		_rockTrailRight=_rocksRight.GetComponentInChildren<TrailRenderer>();
		_rockTrailLeft.emitting=false;
		_rockTrailRight.emitting=false;

		_terrainData = _terrain.terrainData;
		_alphaMaps = _terrainData.GetAlphamaps(0,0,_terrainData.alphamapWidth,_terrainData.alphamapHeight);
	}

	void OnEnable(){
		_velocity=Vector3.zero;

		_curFlapAccel=Vector3.up*_flapAccel*_initialVelBoost;
		//_curFlapAccel+=transform.forward*_bird.GetVel();//*_flapAccel.z;

		//initial velocity
		//_velocity=_curFlapAccel;
		//_velocity+=transform.forward*_bird.GetVel()*_maxVel.z*_initialVelBoost;

		_speedFrac=0;

		_knockBackTimer=0;
		_prevSqrMag=0;

		_aoa=0;

		_flapTimer=0;
		_flapCounter=0;

		_forwardness=0;
		//_diving=false;
		_anim.ResetTrigger("soar");
		_rollAngle=0f;

		Flap(false);

		_mat = _bird.GetMaterial();
		_mat.SetColor("_RimColor",_fullColor);
		//_landTarget.gameObject.SetActive(true);
	}

	void OnDisable(){
		//Soar(false);
		_mat.SetColor("_RimColor",Color.black);
		Soar(false);
		_anim.SetTrigger("land");
		Reset();
		//_landTarget.gameObject.SetActive(false);
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
				_curFlapAccel=(transform.up+transform.forward)*_flapAccel;
				_flapTimer=0;
				_anim.SetTrigger("fly");
				Soar(false);
				Flap();
			}
		}
		else if(_mIn.GetLandDown()){
			if(_flapCounter<_numFlaps){
				_knockBackTimer=0f;//can reset knockback by flapping
				_curFlapAccel=-flatForward*_flapDeccel;
				_curFlapAccel+=Vector3.down*_flapDeccelDown;
				_flapTimer=0;
				_anim.SetTrigger("fly");
				Soar(false);
				Flap();
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
		flatVel.y=0;

		if(_knockBackTimer<=0)
		{
			//pitch control
			flatForward=transform.forward;
			flatForward.y=0;
			flatForward.Normalize();
			float prevAoa=_aoa;
			/*
			_aoa+=input.y*Time.deltaTime*_angleChangeMult;
			if(input.y==0)
				_aoa=Mathf.Lerp(_aoa,0,_angleFallMult*Time.deltaTime);
				*/
			float y01=(input.y+1)*0.5f;
			_aoa=Mathf.Lerp(-_maxAoa,_maxAoa,y01);
			//_aoa = Mathf.Clamp(_aoa,-_maxAoa,_maxAoa);

			//rotate velocity
			if(_soaring){
				Vector3 prevVel=_velocity;
				if(_aoa>0)
					_velocity=Quaternion.Euler(_aoa*transform.right*_pitchSpeed*Time.deltaTime)*_velocity;
				else if(_aoa<0)
					_velocity=Quaternion.Euler(_aoa*transform.right*_upPitchSpeed*Time.deltaTime)*_velocity;
				flatVel=_velocity;
				flatVel.y=0;
				float yVel=_velocity.y;
				float phi=Mathf.Atan2(yVel,flatVel.magnitude);
				if(phi<-_maxPhi&&_aoa>0||phi>_maxUpPhi&&_aoa<0)
				{
					_velocity=prevVel;
				}
				flatVel=_velocity;
				flatVel.y=0;
			}
			
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


		//apply physics
		Vector3 prevPos=transform.position;
		transform.position+=_velocity*Time.deltaTime;
		float normVel=flatVel.magnitude/_maxVel.z;
		float gLerp=1-normVel;
		_gravity=_defaultGravity;
		_velocity-=flatForward*_airResistance*Time.deltaTime;
		_velocity-=Vector3.up*_gravity*Time.deltaTime;

		flatVel=_velocity;
		flatVel.y=0;

		//cap vertical velocity
		if(_velocity.y>_maxVel.y)
		{
			_velocity.y=_maxVel.y;
		}

		//prevent backwards flight
		if(_forwardness<=0){
			_velocity.x=0f;
			_velocity.z=0f;
		}

		_prevSqrMag=_velocity.sqrMagnitude;
		
		//debugging
		DebugScreen.Print("Vel mag: "+_velocity.magnitude.ToString("0.000"));
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
				_flyShadow.gameObject.SetActive(false);
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

			//raycast down like 50
			//hit point = foobar
			//if hit
			//	if terrain!=null && hit==terrain
			//		proceed
			//	else if hasTerrain component
			//		terrain=hit.terrain
			//	hit point = hit
			//else
			//	terrain=null

			Vector3 hitPoint=Vector3.zero;
			if(Physics.Raycast(transform.position,Vector3.down, out hit, 5f, _bird._collisionLayer)){
				hitPoint=hit.point;
				if(_bird._terrain!=null && hit.transform==_bird._terrain.transform)
				{
					//ok
				}
				else if(hit.transform.GetComponent<Terrain>()!=null){
					_bird._terrain=hit.transform.GetComponent<Terrain>();
				}
			}
			else{
				_bird._terrain=null;
			}

			//water = false
			//if (terrain==null || hit point <5) && yPos<5+maxDist
			// 	water = true

			bool water=false;
			if ((_bird._terrain==null || hitPoint.y<=5f)&&transform.position.y<5f+_maxDist)
				water=true;
			DebugScreen.Print("Water: "+water);

			//grass = false
			//sand = false
			//gravel = false
			//if terrain!=null && water==false
			//	int terrainIndex=bird.GetTerrainIndex
			//	if tI==0
			//		sand = true
			//	else if tI==1
			//		gravel = true
			//	else if tI==2
			//		sand = true
			//		

			bool grass=false;
			bool sand=false;
			bool gravel=false;
			if(_bird._terrain!=null && water==false){
				int terrainIndex=_bird.GetTerrainTextureIndex();
				if(terrainIndex==0)
					sand=true;
				else if(terrainIndex==1)
					gravel=true;
				else if(terrainIndex==2)
					grass=true;
			}
			DebugScreen.Print("Sand: "+sand);
			DebugScreen.Print("Gravel: "+gravel);
			DebugScreen.Print("Grass: "+grass);


			//check for land target
			/*
			if(Physics.Raycast(transform.position,Vector3.down, out hit, 5f, _bird._collisionLayer)){

				bool sprayActive=hit.point.y<=5.1f;
				Vector3 point=hit.point;
				point.y=5f;
				float diff=transform.position.y-point.y;
				float frac=Mathf.Clamp01(diff/_maxDist);
				if(sprayActive&&!_spraySound.IsOn())
				{
					_sprayParts.Play();
					_spraySound.Play();
					_sprayTrail.emitting=true;
				}
				else if(!sprayActive&&_spraySound.IsOn())
				{
					_sprayParts.Stop();
					_spraySound.Stop();
					_sprayTrail.emitting=false;
				}
				if(_sprayParts.isPlaying){
					_sprayEmission.rateOverTime=50f*(1-frac);
					_spraySound.SetTarget(1-frac);
					_sprayTrail.startColor=Color.white*(1-frac);
					_sprayTrail.startWidth=0.5f*(1-frac);
					_waterSpray.position=point;
					_waterSpray.rotation=Quaternion.identity;
				}

				//sand parts
				bool sandActive=false;
				if(!sprayActive&&hit.transform==_terrain.transform){
					//check if point is sand
					int layer = GetTerrainTextureIndex(hit.point);
					DebugScreen.Print("Over layer: "+layer);
					sandActive=layer==0;
				}
				DebugScreen.Print("Sand active: "+sandActive);

				diff=transform.position.y-hit.point.y;
				frac=Mathf.Clamp01(diff/_maxDist);
				if(sandActive&&!_sandSound.IsOn())
				{
					_sandParts.Play();
					_sandSound.Play();
				}
				else if(!sandActive&&_sandSound.IsOn())
				{
					_sandParts.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
					//_sandParts.Stop();
					_sandSound.Stop();
				}
				if(_sandParts.isPlaying){
					_sandSpray.position=hit.point;
					_sandSpray.rotation=Quaternion.identity;
					_sandEmission.rateOverTime=200f*(1-frac);
					_sandSound.SetTarget((1-frac));
				}
				
				//grass parts
				bool grassActive=false;
				if(!sprayActive&&!sandActive&&hit.transform==_terrain.transform){
					//check if point is sand
					int layer = GetTerrainTextureIndex(hit.point);
					grassActive=layer==2;
				}
				if(grassActive&&!_grassSound.IsOn())
				{
					_grassParts.Play();
					_grassSound.Play();
				}
				else if(!grassActive&&_grassSound.IsOn())
				{
					_grassParts.Stop();
					_grassSound.Stop();
				}
				if(_grassParts.isPlaying){
					_grassSpray.position=hit.point;
					_grassSpray.rotation=Quaternion.identity;
					_grassEmission.rateOverTime=100f*(1-frac);
					_grassSound.SetTarget((1-frac));
				}
			}
			bool rocksActive=false;
			Vector3 right=transform.right;
			right.y=0f;
			//rocks on right
			if(Physics.Raycast(transform.position,right, out hit, _maxDistRocks, _bird._collisionLayer)){

				//sand parts
				if(hit.transform==_terrain.transform){
					//check if point is sand
					int layer = GetTerrainTextureIndex(hit.point);
					rocksActive=layer==1;
				}

				float dist=(transform.position-hit.point).magnitude;
				float frac=Mathf.Clamp01(dist/(_maxDistRocks));
				if(_rockPartsRight.isPlaying){
					_rocksRight.position=hit.point;
					_rocksRight.up=-right;
					_rockEmissionRight.rateOverTime=100f*(1-frac);
					_rockSoundRight.SetTarget((1-frac));
					_rockTrailRight.startWidth=0.25f*(1-frac);
				}
			}
			if(rocksActive&&!_rockSoundRight.IsOn())
			{
				_rockPartsRight.Play();
				_rockSoundRight.Play();
				_rockTrailRight.emitting=true;
			}
			else if(!rocksActive&&_rockSoundRight.IsOn())
			{
				_rockPartsRight.Stop();
				_rockSoundRight.Stop();
				_rockTrailRight.emitting=false;
			}

			//rocks on left
			rocksActive=false;
			Vector3 left=-transform.right;
			left.y=0f;
			if(Physics.Raycast(transform.position,left, out hit, _maxDistRocks, _bird._collisionLayer)){

				//sand parts
				if(hit.transform==_terrain.transform){
					//check if point is sand
					int layer = GetTerrainTextureIndex(hit.point);
					rocksActive=layer==1;
				}

				float dist=(transform.position-hit.point).magnitude;
				float frac=Mathf.Clamp01(dist/(_maxDistRocks));
				if(_rockPartsLeft.isPlaying){
					_rocksLeft.position=hit.point;
					_rocksLeft.up=-left;
					_rockEmissionLeft.rateOverTime=100f*(1-frac);
					_rockSoundLeft.SetTarget((1-frac));
					_rockTrailLeft.startWidth=0.25f*(1-frac);
				}
			}
			if(rocksActive&&!_rockSoundLeft.IsOn())
			{
				_rockPartsLeft.Play();
				_rockSoundLeft.Play();
				_rockTrailLeft.emitting=true;
			}
			else if(!rocksActive&&_rockSoundLeft.IsOn())
			{
				_rockPartsLeft.Stop();
				_rockSoundLeft.Stop();
				_rockTrailLeft.emitting=false;
			}
			*/
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

	public void Reset(){
		_sprayParts.Stop();
		_spraySound.Stop();
		//_sandParts.Stop();
		_sandParts.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
		_sandSound.Stop();
		_grassParts.Stop();
		_grassSound.Stop();
		_rockPartsLeft.Stop();
		_rockSoundLeft.Stop();
		_rockPartsRight.Stop();
		_rockSoundRight.Stop();
		_sprayTrail.emitting=false;
		_rockTrailLeft.emitting=false;
		_rockTrailRight.emitting=false;
	}

	public bool IsFlapping(){
		return _flapTimer<_flapDur*2f;
	}

	int GetTerrainTextureIndex(Vector3 pos){
		//convert world coord to terrain space
		float xWorld=pos.x;
		float zWorld=pos.z;
		Vector3 local=pos-_terrain.transform.position;
		float xFrac=local.x/_terrainData.size.x;
		float zFrac=local.z/_terrainData.size.z;
		if(xFrac<0||xFrac>=1)
			return 0;
		if(zFrac<0||zFrac>=1)
			return 0;
		int xCoord=Mathf.FloorToInt(zFrac*_terrainData.alphamapHeight);
		int yCoord=Mathf.FloorToInt(xFrac*_terrainData.alphamapWidth);
		float max=0;
		int layer=0;
		for(int i=0;i<_terrainData.alphamapLayers;i++){
			float v = _alphaMaps[xCoord,yCoord,i];
			if(v>max)
			{
				max=v;
				layer=i;
			}
		}
		return layer;
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
