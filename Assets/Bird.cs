﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Bird : MonoBehaviour
{
	public float _triggerRadius;
	public int _state;
	public bool _startSoaring;
	Hop _hop;
	Fly _fly;
	Waddle _waddle;
	Tool _tool;
	Follow _follow;
	Tutorial _tutorial;
	[HideInInspector]
	public Animator _anim;
	AudioSource _ruffleAudio;
	public bool _playerControlled;
	MInput _mIn;
	MCamera _mCam;
	Transform _camera;
	public Bird _mate;
	[HideInInspector]
	public Vector3 _size;
	[Header("Call")]
	public Transform _callEffects;
	public Vector2 _callPitchRange;
	public AudioClip _call;
	public LayerMask _collisionLayer;
	public LayerMask _birdLayer;
	public LayerMask _oceanLayer;
	public float _controllerZero;
	[Header("Footprints")]
	public Transform [] _footprints;
	public Vector3 _footprintOffset;
	int _leftRightPrint=1;
	public Terrain _terrain;
	float[,,] _alphaMaps;
	TerrainData _terrainData;

	[Header("Explosion")]
	public float _afterDiveDelay;
	float _afterDiveTimer;
	public Transform _explodeParts;
	public Transform _landParts;
	public float _divePartsDelay;
	[Header("Collisions")]
	public float _shakeDelay;
	float _shakeTimer;
	ParticleSystem _starParts;
	public float _waddleKnockVolume;
	public float _hopKnockVolume;
	Vector3 _prevPos;
	Vector3 _vel;
	public float _hitYOffset;
	public float _hitRadius;
	SphereCollider _sphereCol;
	public Transform _mandible;
	Transform _curKey;
	public Transform _ruffleEffects;
	public float _summonDist;
	SkinnedMeshRenderer _smr;
	Vector3 _lastSpot;
	Collider[] _cols;

	[Header("NPC - peck")]
	public float _peckCheckTime;
	public float _peckChance;
	float _peckTimer;
	public float _flySpeed;
	public float _maxSpeed;
	public float _flapDur;

	public Transform _band;
	public Color _bandColor;
	Bird _player;

	//cameras
	[Header("Cameras")]
	public Camera _waddleCam;
	public Camera _idleCam;
	public Camera _hopCam;
	public Camera _flyCam;
	public Camera _speciesCam;
	SpeciesCam _sCam;
	public Camera _singCam;

	[System.Serializable]
	public struct BirdData{
		public string _name;
		public float _scale;
		public Material _mat;
		public Mesh _mesh;
		public float _walkSpeed;
		//public float _colRad;
		//other stuff may include
		//walk speed
		//camera distance
	}

	//[Header("Interactions")]
	LightSwitch _nearSwitch;
	TouchPlate _nearPlate;
	//#todo - rework so it's near food or something
	//seed is for prototoype
	Seed _nearSeed;
	//checkpoint
	Vector3 _checkPoint;

	[Header("Bath time")]
	public bool _inWater;
	public AudioClip _waterShake;
	public Transform _splash;

	//ui stuff
	ButtonPrompts _butts;
	RawImage _reticle;
	NPB _npb;
	Sing _sing;
	public List<Sing.BirdSong> _songs;

	void Awake(){
		//calculations
		_smr = transform.GetComponentInChildren<SkinnedMeshRenderer>();
		_size=_smr.sharedMesh.bounds.size*_smr.transform.localScale.x*transform.localScale.x;

		//get references
		_hop=GetComponent<Hop>();
		_fly=GetComponent<Fly>();
		_waddle=GetComponent<Waddle>();
		_tool=GetComponent<Tool>();
		_follow=GetComponent<Follow>();
		_tutorial=GetComponentInChildren<Tutorial>();
		_anim=GetComponent<Animator>();
		//_ruffleAudio=transform.Find("Ruffle").GetComponent<AudioSource>();
		//_starParts=transform.Find("StarParts").GetComponent<ParticleSystem>();
		_mIn = GameManager._mIn;
		_mCam = GameManager._mCam;
		_camera=_mCam.transform;
		_cols = new Collider[3];
		if(_playerControlled)
		{
			_waddleCam = transform.Find("WaddleCam").GetComponent<Camera>();
			_flyCam = transform.Find("FlyCam").GetComponent<Camera>();
			_speciesCam = transform.Find("SpeciesCam").GetComponent<Camera>();
			_sCam=_speciesCam.GetComponent<SpeciesCam>();
			_singCam = transform.Find("SingCam").GetComponent<Camera>();
		}

		//disable things
		_hop.enabled=false;
		_fly.enabled=false;

		_lastSpot=transform.position;

		_player=GameManager._player;

		if(_state==0)
			Ground();
		else if(!_playerControlled){
			_anim.SetTrigger("flyLoop");
		}
		_prevPos=transform.position;

		_butts=FindObjectOfType<ButtonPrompts>();
		//_reticle=_butts.transform.parent.Find("Reticle").GetComponent<RawImage>();
		Transform hud=_mCam.transform.Find("Hud");
		_reticle=hud.Find("Reticle").GetComponent<RawImage>();
		_sing=transform.GetComponentInChildren<Sing>();
		_songs = new List<Sing.BirdSong>();

		_sphereCol = GetComponent<SphereCollider>();
		_hitRadius=_sphereCol.radius*transform.localScale.x;
		_hitYOffset=_sphereCol.center.y*transform.localScale.x;
		SetCheckPoint();
	}

    // Start is called before the first frame update
    void Start()
    {
		if(_startSoaring){
			Fly();
			_fly.BoostSpeed(3f,0);
			_fly.KillVert();
		}
    }

    // Update is called once per frame
    void Update()
    {
		RaycastHit hit;
		bool targetting=false;
		_vel=transform.position-_prevPos;
		if(_playerControlled){
			//player update
			switch(_state){
				case 0://chilling
					Ground();
					if(_mIn.GetJumpDown())
						StartHopping();
					else if(_mIn.GetControllerInput().sqrMagnitude>_controllerZero*_controllerZero)
						StartWaddling();
					//look for other birds
					targetting=ScanForNpb();
					/*
					if(_mIn.GetInteractDown()){
						//todo, for certain prompts like new song acquired
						//new photo, or new island unlocked, I'd like to be able to force the menu
						//to open to a particular pane, but for now let's just test the input
						Debug.Log("Opening menu");
						GameManager.Pause();
						_state=7;
						//state = 7 , so we can disable the input from interfering with the game
						//GameManager.Pause 
					}
					*/
					break;
				case 1://waddling
					if(!_waddle.IsWaddling()){
						StopWaddling();
					}
					else if(_waddle.IsKnockBack()){
						//just wait
					}
					else if(_mIn.GetJumpDown()){
						StartHopping();
					}
					//look for other birds
					targetting=ScanForNpb();
					break;
				case 2://hopping
					if(!_hop.IsHopping()){
						if(_mIn.GetControllerInput().sqrMagnitude<=_controllerZero*_controllerZero){
							StopHopping();
							//go to idle
							/*
							_state=0;
							_hop.enabled=false;
							_anim.SetFloat("walkSpeed",0f);
							*/
						}
						else{
							StartWaddling();
						}
					}
					else{
						if(_mIn.GetJumpDown()){
							Fly();
						}
					}
					//look for other birds
					targetting=ScanForNpb();
					break;
				case 3://flying
					break;
				case 4://after dive
					_afterDiveTimer+=Time.deltaTime;
					if(_afterDiveTimer>_afterDiveDelay)
						_state=0;
					break;
				case 5://shake it off
					_shakeTimer+=Time.deltaTime;
					if(_shakeTimer>=_shakeDelay){
						_state=0;
					}
					break;
				case 6://singing
					if(SpeciesScreen._canSkip&&_mIn.GetJumpDown()){
						EndScan();
					}
					break;
				case 7://menu state
					/*
					if(_mIn.GetInteractDown()){
						//todo, for certain prompts like new song acquired
						//new photo, or new island unlocked, I'd like to be able to force the menu
						//to open to a particular pane, but for now let's just test the input
						Debug.Log("Closing menu");
						GameManager.Play();
						_state=0;
						//state = 7 , so we can disable the input from interfering with the game
						//GameManager.Pause 
					}
					*/
					break;
				case 8://feeding
					break;
				case 9://interacting
					break;
				case 10://edit mode
					break;
				default:
					break;
			}
			if(_mIn.GetResetDown()){
				LoadCheckPoint();
				Ground();
			}
			/*
			if(Input.GetKeyDown(KeyCode.F1)){
				_waddle.ToggleCamLines();
			}
			*/
		}
		else{
			switch(_state){
				case 0:
				default://chilling
					Ground();
					_peckTimer+=Time.deltaTime;
					if(_peckTimer>_peckCheckTime){
						if(Random.value<_peckChance){
							_anim.SetTrigger("peck");
							if(Physics.Raycast(transform.position+Vector3.up*_size.y*0.5f, Vector3.down, out hit, _size.y,1)){
								Footstep f = hit.transform.GetComponent<Footstep>();
								if(f!=null)
									f.Sound(hit.point);
							}
						}
						_peckTimer=0;
					}
					break;
				case 3://flying
					break;
				case 2://following
					break;
			}
		}
		_prevPos=transform.position;
		
		//_butts.SetActive(targetting);
		_reticle.color=targetting?Color.white:Color.black;
		_reticle.enabled=_state<3;
		//if targetting and get sing down
		//	go to sing state
		//	tell npb to listen
    }

	public bool IsPlayerClose(Bird other){
		return (other.transform.position-transform.position).sqrMagnitude<_triggerRadius*_triggerRadius;
	}

	public bool IsPlayerInRange(Transform other,float rad){
		return (other.position-transform.position).sqrMagnitude<rad*rad;
	}

	public float Ruffle(){
		//_anim.SetTrigger("ruffle");
		//Instantiate(_ruffleEffects,transform.position+_size.y*0.5f*Vector3.up,Quaternion.identity,transform);
		//_state=2;
		//_ruffleAudio.Play();
		//return _ruffleAudio.clip.length;
		return 0;
	}

	public void Call(){
		//_anim.SetTrigger("sing");
		Transform call = Instantiate(_callEffects,transform.position+Vector3.up*_size.y,Quaternion.identity);
		AudioSource source = call.GetComponent<AudioSource>();
		source.clip=_call;
		source.pitch=Random.Range(_callPitchRange.x,_callPitchRange.y);
		source.Play();
		_anim.SetTrigger("sing");
		StartCoroutine(InvokeCall());
	}

	IEnumerator InvokeCall(){
		float dur = _call.length;
		yield return new WaitForSeconds(dur);
		if(_onCall!=null)
			_onCall.Invoke();
	}

	public void CopyCall(Bird other){
		_call=other._call;
	}

	public void ComeTo(Transform t){
		Vector3 diff=transform.position-t.position;
		//_mate.WaddleTo(transform.position-diff.normalized*_summonDist,1f);
		transform.position=t.position+diff.normalized*_summonDist;
	}

	public void HopTo(Vector3 loc){
		_hop.enabled=true;
		_hop.HopTo(loc);
		_state=2;
	}

	public void WaddleTo(Vector3 loc,float speed){
		Ground();
		_waddle.enabled=true;
		_waddle.WaddleTo(loc,speed);
		_state=1;
	}

	public void StartHopping(bool smallHop=false){
		_state=2;
		_hop.enabled=true;
		_waddle.enabled=false;
		if(smallHop)
			_hop.LimitHopTimer();
		//GameManager._mCam.Transition(_hopCam,MCamera.Transitions.CUT_BACK);
		GameManager._mCam.Transition(_waddleCam,MCamera.Transitions.CUT_BACK);
	}

	void StartWaddling(){
		_state=1;
		_waddle.enabled=true;
		_hop.enabled=false;
		GameManager._mCam.Transition(_waddleCam,MCamera.Transitions.CUT_BACK);
	}

	public bool IsHopping(){
		return _hop.enabled&&_hop.IsHopping();
	}

	public void StopHopping(){
		_hop.StopHopping();
		_hop.enabled=false;
		_anim.SetFloat("walkSpeed",0f);
		_state=0;
		GameManager._mCam.Transition(_waddleCam,MCamera.Transitions.CUT_BACK);
	}

	public void StopWaddling(){
		_waddle.StopWaddling();
		_state=0;
	}

	public bool IsWaddling(){
		return _waddle.IsWaddling();
	}
	public bool IsFlying(){
		return _fly.enabled;
	}

	void Fly(){
		//disable waddle
		_waddle.enabled=false;
		//disable hop
		_hop.enabled=false;
		//enable flight
		_fly.enabled=true;
		_flyCam.GetComponent<FlyCam>().SetPriority();
		_state=3;
		_anim.SetTrigger("fly");
		//GameManager._mCam.Transition(_flyCam,MCamera.Transitions.LERP,0f,null,0.5f);
		GameManager._mCam.Transition(_flyCam,MCamera.Transitions.CUT_BACK);
		if(_onFlight!=null)
			_onFlight.Invoke();
	}
	public void Land(){
		SaveLastSpot();
		_fly.enabled=false;
		_state=0;
		_anim.SetFloat("walkSpeed",0f);
		_anim.SetTrigger("land");
		_hop.PlayStepParticles();

		//reset flight priority to 0
		_flyCam.GetComponent<FlyCam>().ResetPriority();

		Instantiate(_landParts,transform.position,Quaternion.identity);

		//which allows us to switch back to the idle cam
		GameManager._mCam.Transition(_waddleCam,MCamera.Transitions.CUT_BACK);
		if(_onLand!=null)
			_onLand.Invoke();
	}

	public void Dive(float vel){
		SaveLastSpot();
		_fly.enabled=false;
		_hop.enabled=false;
		_state=4;
		//reset pitch
		Vector3 eulerAngles=transform.eulerAngles;
		float targetPitch=0;
		eulerAngles.x=targetPitch;
		transform.eulerAngles=eulerAngles;
		//polish
		StartCoroutine(PlayExplodeParticlesR(vel));
		_anim.SetFloat("walkSpeed",0f);
		_anim.SetTrigger("land");
		_afterDiveTimer=0;
	}

	public void MakeFootprint(Transform surface, float offset=0){
		Vector3 mOffset=_footprintOffset;
		if(_leftRightPrint<0)
			mOffset.x*=-1;
		mOffset+=transform.forward*offset;
		_leftRightPrint*=-1;
		if(_terrain!=null&&surface==_terrain.transform){
			Vector3 pos=transform.position+mOffset;
			int terrainIndex = GetTerrainTextureIndex();
			Instantiate(_footprints[terrainIndex],pos,Quaternion.identity);
		}
	}

	IEnumerator PlayExplodeParticlesR(float vel){
		Instantiate(_explodeParts,transform.position,Quaternion.identity);
		yield return new WaitForSeconds(_divePartsDelay);
	}

	public void KnockBack(CollisionHelper ch, Vector3 dir,bool supress=false,bool ignoreNpc=false){
		if(ignoreNpc&&!_playerControlled)
			return;
		if(!supress){
			/*
			if(_state>2)
				_starParts.Play();
				*/
		}

		switch(_state){
			case 0:
			case 1://waddling
				//ch.Sound(_waddleKnockVolume);
				_waddle.KnockBack(dir);
				break;
			case 2://hopping
				//ch.Sound(_hopKnockVolume);
				_hop.KnockBack(dir);
				break;
			case 3://flying
				ch.Sound(_hopKnockVolume);
				_fly.KnockBack(dir);
				break;
			default:
				break;
		}
	}

	public void ShakeItOff(){
		_waddle.enabled=false;
		_hop.enabled=false;
		_fly.enabled=false;
		_anim.SetFloat("walkSpeed",0f);
		_shakeTimer=0;
		_state=5;
	}

	public void RevertToPreviousPosition(){
		//move back to previous frame, to prevent collider from getting stuck overlapping
		if(_state==7)
			return;
		transform.position=_prevPos;
	}

	public void SnapToPos(Vector3 pos){
		transform.position=pos;
		Ground(false);
	}

	public bool GoingUp(){
		return _vel.y>0;
	}

	public Vector3 GetVelocity(){
		return _vel/Time.deltaTime;
	}

	public float GetVel(){
		if(_state==9)
			return 0f;
		return _vel.magnitude/Time.deltaTime;
	}

	public void CollectKey(Transform t){
		t.SetParent(_mandible);
		t.localPosition=Vector3.zero;
		t.localEulerAngles=Vector3.up*90f;
		t.localScale=Vector3.one*0.25f;
		_curKey=t;
	}

	public Transform GetKey(){
		return _curKey;
	}

	public void UseKey(Transform tool,ToolPath path){
		if(_curKey!=null){
			Vector3 handlePos=_curKey.Find("Handle").position;
			RaycastHit hit;
			if(Physics.Raycast(handlePos,Vector3.down,out hit, 1f, 1)){
				transform.position=hit.point;
			}
			_curKey=null;
		}
		StartUsingTool(path);
	}

	void StartUsingTool(ToolPath path){
		_waddle.enabled=false;
		_hop.enabled=false;
		_fly.enabled=false;
		_anim.SetFloat("walkSpeed",0f);
		//enter tool state
		_tool._path=path;
		_tool.enabled=true;
		_state=6;
	}

	public void DoneWithTool(){
		_state=0;
		_tool.enabled=false;
	}

	public void Ground(bool resetState=true){
		if(_waddle==null)
			return;
		if(resetState){
			_waddle.enabled=false;
			_hop.enabled=false;
			_fly.enabled=false;
			_anim.SetFloat("walkSpeed",0f);
			_state=0;
		}
		//remove roll
		Vector3 eulerAngles=transform.eulerAngles;
		eulerAngles.z=0;
		transform.eulerAngles=eulerAngles;

		RaycastHit hit;
		if(Physics.Raycast(transform.position+_size.y*Vector3.up,Vector3.down, out hit,1f,_collisionLayer)){
			if(Mathf.Abs(hit.point.y-transform.position.y)>0.25f)
			{
				//Debug.Log("Oh snap");
			}
			else
				transform.position=hit.point;
			SetTerrain(hit.transform.GetComponent<Terrain>());
		}
	}

	public void SetTerrain(Terrain t){
		if(_terrain==t)
			return;
		_terrain=t;
		if(_terrain!=null){
			_terrainData = _terrain.terrainData;
			_alphaMaps = _terrainData.GetAlphamaps(0,0,_terrainData.alphamapWidth,_terrainData.alphamapHeight);
			//show species list
			//BirdSpawner bs = t.transform.parent.Find("BirdSpawner").GetComponent<BirdSpawner>();
			//bs.ShowList();
		}
		else{
		}
	}

	Terrain _cardTerrain;
	public void SetCardTerrain(Terrain t){
		if(_cardTerrain==t)
			return;
		_cardTerrain=t;
		if(_cardTerrain!=null){
			BirdSpawner bs = t.transform.parent.Find("BirdSpawner").GetComponent<BirdSpawner>();
			bs.ShowList();
		}
		else{
		}
	}

	public Vector3 GetCamTarget(){
		if(_state==2)
			return _hop.GetCamTarget();
		else
			return transform.position;
		//return transform.position;
	}

	public void CallToMate(){
		//Debug.Log(name + " got walked into by: "+b.name);
		//Vector3 diff = _mate.transform.position-transform.position;
		//diff.y=0;
		//transform.forward=diff;
		//Call();
		//Ruffle();
	}

	public void RuffleToMate(){
		if(_mate==null)
			return;
		//Debug.Log(name + " got walked into by: "+b.name);
		Vector3 diff = _mate.transform.position-transform.position;
		diff.y=0;
		transform.forward=diff;
		Ruffle();
	}

	public void StartFollowing(){
		_follow.StartFollowingMate();
	}
	
	public void StopFollowing(){
		_follow.StopFollowingMate();
	}

	public void ShowTutorial(int index){
		_tutorial.ShowTutorial(index);
	}

	IEnumerator _flyRoutine;
	public void FlyTo(Vector3 target, float flapChance=1){
		_smr.enabled=true;
		//float dur = (target-transform.position).magnitude/_flySpeed;
		if(_flyRoutine!=null)
			StopCoroutine(_flyRoutine);
		_flyRoutine=FlyToR(target,flapChance);
		StartCoroutine(_flyRoutine);
		//_anim.SetTrigger("flyLoop");
		_anim.SetTrigger("fly");
	}

	public delegate void BirdEvent();
	public event BirdEvent _onDoneFlying;
	public event BirdEvent _onFlight;
	public event BirdEvent _onLand;
	public event BirdEvent _onCall;

	IEnumerator FlyToR(Vector3 target,float flapChance){
		_state=3;
		Vector3 start=transform.position;
		transform.LookAt(target);
		Vector3 eulers = transform.eulerAngles;
		eulers.x=0;
		transform.eulerAngles=eulers;
		Vector3 dir = (target-start);
		float dist = dir.magnitude;
		dir/=dist;
		float travelDist=0;
		float timer=0;
		float speed=0;
		float flapTimer=0;
		while(travelDist<dist){
			timer+=Time.deltaTime;
			flapTimer+=Time.deltaTime;
			if(flapTimer>_flapDur){
				if(Random.value<flapChance){
					_fly.FlapSounds(true);
					_anim.SetTrigger("fly");
					_fly.Soar(false);
				}
				else{
					_fly.Soar(true);
				}
				flapTimer=0;
			}
			speed = Mathf.Lerp(_flySpeed,_maxSpeed,timer);
			Vector3 v = dir*Time.deltaTime*speed;
			travelDist+=v.magnitude;
			transform.position+=v;
			yield return null;
		}
		_fly.Soar(false);
		_anim.SetTrigger("land");
		Ground();
		_state=0;
		if(_onDoneFlying!=null)
			_onDoneFlying.Invoke();
	}

	public void Drown(){
		_state=7;
		_anim.SetFloat("walkSpeed",0f);
		if(_fly.enabled)
		{
			_fly.Reset();
			_fly.Soar(false);
			_anim.SetTrigger("land");
		}
		_waddle.enabled=false;
		_hop.enabled=false;
		_fly.enabled=false;
		_smr.enabled=false;
	}

	public void Respawn(){
		transform.position=_lastSpot;
		Vector3 eulers = transform.eulerAngles;
		eulers.x=0;
		transform.eulerAngles=eulers;
		_smr.enabled=true;
	}

	public void ResetState(){
		_state=0;
		TransitionToRelevantCamera();
	}

	public void SaveLastSpot(){
		_lastSpot=transform.position;
	}

	public void TransitionToRelevantCamera(){

		switch(_state){
			case 0://idle cam
				_mCam.Transition(_waddleCam,MCamera.Transitions.CUT_BACK,0,null,0f,true);
				break;
			case 1://waddle cam
			case 2://hop
				//_mCam.Transition(_waddleCam,MCamera.Transitions.LERP,0,null,0.5f,true);
				_mCam.Transition(_waddleCam,MCamera.Transitions.CUT_BACK,0,null,0f,true);
				break;
			case 3://flying
				_mCam.Transition(_flyCam,MCamera.Transitions.CUT_BACK,0,null,0f,true);
				break;
			default://tbd 
				break;
		}
	}

	public void FlyAway(){
		FlyTo(transform.position+new Vector3(1,1,1)*3f);
		_mate=null;
		_onDoneFlying+=DoneFlying;
	}

	public void DoneFlying(){
		Destroy(gameObject);
	}

	public void AlertMates(){
		if(Physics.OverlapSphereNonAlloc(transform.position,2f,_cols,_birdLayer)>0){
			Bird b = _cols[0].GetComponent<Bird>();
			b.Call();
		}
	}

	Vector3 _posBeforeNestBox;
	Camera _camBeforeNestBox;
	public void WalkInNestBox(Transform t,BirdHouse bh){
		if(_state==7)
			return;
		if(_fly.enabled)
		{
			_fly.Soar(false);
			Land();
		}
		//disable walk, fly, and hop
		_waddle.enabled=false;
		_hop.enabled=false;
		_fly.enabled=false;
		//start coroutine
		Vector3 dir = t.forward;
		/*
		Transform parent = transform.parent;
		transform.SetParent(t);
		Vector3 pos=transform.localPosition;
		pos.x=0;
		transform.localPosition=pos;
		transform.SetParent(parent);
		*/
		//set state to something special
		_state=7;
		StartCoroutine(WalkThroughDoorR(dir,bh));
	}

	IEnumerator WalkR(){
		float timer=0;
		Vector3 dir=transform.forward;
		_anim.SetFloat("walkSpeed",0.1f);
		float dur = 1f;
		while(timer<dur){
			timer+=Time.deltaTime;
			transform.position+=dir*Time.deltaTime*_waddle._walkSpeed*0.75f;
			yield return null;
		}
		_posBeforeNestBox=transform.position;
		_anim.SetFloat("walkSpeed",0f);

	}

	IEnumerator WalkThroughDoorR(Vector3 dir,BirdHouse bh){
		transform.forward=dir;
		StartCoroutine(WalkR());

		//transition to nest box
		float dur = 3f;
		float halfDur=dur*0.5f;
		//Camera doorCam=t.GetComponentInChildren<Camera>();
		_camBeforeNestBox=GameManager._mCam.GetCurTargetCam();
		GameManager._mCam.Transition(_camBeforeNestBox,MCamera.Transitions.FADE,0,null,dur);

		yield return new WaitForSeconds(halfDur);
		bh.SetInteriorActive(true);
		Transform startT = bh.GetPlayerStart();
		transform.position=startT.position;
		transform.rotation=startT.rotation;
		Ground();
		_state=0;
	}

	public void WalkOutNestBox(Transform t,BirdHouse bh){
		if(_fly.enabled)
		{
			_fly.Soar(false);
			Land();
		}
		//start coroutine
		Vector3 dir = -t.forward;
		Camera doorCam=bh.GetDoorCam();
		StartCoroutine(WalkOutDoorR(doorCam,dir,bh));
	}

	IEnumerator WalkOutDoorR(Camera cam,Vector3 dir,BirdHouse bh){
		float dur = 2f;
		float halfDur=dur*0.5f;
		//GameManager._mCam.Transition(cam,MCamera.Transitions.FADE,0,null,dur,true,true);
		GameManager._mCam.Transition(_camBeforeNestBox,MCamera.Transitions.FADE,0,null,dur,true);

		yield return new WaitForSeconds(halfDur);
		bh.SetInteriorActive(false);
		//GameManager._islands.SetActive(true);
		//GameManager._sky.SetActive(true);
		_waddle.enabled=false;
		_hop.enabled=false;
		_fly.enabled=false;
		//enabled=false;
		_state=7;
		transform.forward=dir;
		Transform door = bh.GetDoor();
		transform.position=door.position;//+transform.forward*0.25f;
		door.GetComponent<Door>().Open();
		/*
		transform.position=_posBeforeNestBox;
		yield return null;
		if(transform.position!=_posBeforeNestBox){
			Debug.Log("Error! position is wack!: "+transform.position);
		}
		*/

		float timer=0;
		//_anim.SetFloat("walkSpeed",0.1f);
		//_anim.SetFloat("hopTime",3f);
		dur = 0.75f;
		while(timer<dur){
			timer+=Time.deltaTime;
			transform.position+=dir*Time.deltaTime*_waddle._walkSpeed*0.75f;
			yield return null;
		}
		_anim.SetFloat("walkSpeed",0f);
		bh.DoneWalkingOut();

		Ground();
		//_state=0;
		ResetState();
		door.GetComponent<Door>().Close();
		//GameManager._mCam.Transition(_camBeforeNestBox,MCamera.Transitions.FADE,0,null,dur,true);
	}

	IEnumerator ResetAfterDelay(float dur){
		yield return new WaitForSeconds(dur);
		_state=0;
	}

	public bool IsGrounded(){
		RaycastHit hit;
		if(Physics.Raycast(transform.position+Vector3.up*_hitYOffset, Vector3.down, out hit, 
					_hitRadius*1.1f,_collisionLayer)){
			return true;
		}
		return false;
	}

	public void NearLightSwitch(LightSwitch ls){
		_nearSwitch=ls;
	}

	public void NearTouchPlate(TouchPlate tp){
		_nearPlate=tp;
	}

	public void NearSeed(Seed seed){
		_nearSeed=seed;
		if(_nearSeed!=null)
			Debug.Log("near seed");
		else
			Debug.Log("Not near seed");
	}

	public void LeaveTouchPlate(TouchPlate tp){
		if(_nearPlate==tp)
			_nearPlate=null;
	}

	public void InWater(bool b){
		_inWater=b;
		if(_inWater)
			Instantiate(_splash,transform.position,Quaternion.identity);
	}

	void Interact(){
		if(_nearSwitch!=null){
			_state=9;
			_nearSwitch.Toggle();
			_waddle.enabled=false;
			_hop.enabled=false;
			//_fly.enabled=false;
			_player._anim.SetTrigger("peck");
			//_anim.SetFloat("walkSpeed",0f);
		}
		else if(_nearPlate!=null){
			_nearPlate.Toggle();
			/*
			_waddle.enabled=false;
			_hop.enabled=false;
			_fly.enabled=false;
			*/
			_player._anim.SetTrigger("peck");
		}
		else if(_nearSeed!=null){
			_player._anim.SetTrigger("peck");
			_nearSeed.GetHeld();
			HoldNearSeed();
		}
		else if(_inWater){
			StartCoroutine(Bathe());
		}
		else{
			_player._anim.SetTrigger("peck");

		}
	}

	public Material GetMaterial(){
		return _smr.material;
	}

	public void SetCheckPoint(){
		_checkPoint=transform.position;
	}

	void LoadCheckPoint(){
		transform.position=_checkPoint;
		Ground();
		ResetState();
	}

	public void AddForce(Vector3 v){
		if(_fly.enabled){
			_fly.AddForce(v);
		}
	}

	IEnumerator Bathe(){
		Ground();
		_state=9;
		_waddle.enabled=false;
		_hop.enabled=false;
		_fly.enabled=false;
		_anim.SetFloat("walkSpeed",0f);
		Sfx.PlayOneShot3DVol(_waterShake,transform.position,0.3f);

		//ruffle
		float ruffleDur=Ruffle()*0.5f;
		yield return new WaitForSeconds(ruffleDur);
		_state=0;
	}

	public void SetEditMode(bool edit){
		if(edit)
		{
			Ground();
			_state=10;
		}
		else
		{
			Ground();
			ResetState();
		}
	}

	//#temp - marked but in future I'd like this to be the one spot
	//where we get terrain stuff
	//#todo - rework footstep, fly to use this get terrain texture method
	public int GetTerrainTextureIndex(){
		Vector3 pos=transform.position;
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

	public void BoostSpeed(float amount,float dur){
		if(_fly.enabled)
			_fly.BoostSpeed(amount,dur);
	}

	public void HoldNearSeed(){
		Transform t = _nearSeed.transform;
		t.SetParent(_mandible);
		t.localPosition=Vector3.zero;
		t.localEulerAngles=new Vector3(90,0,0);
		t.localScale=Vector3.one*0.25f;
		//this prevents re-eating same seed
		_nearSeed=null;
	}

	public bool DestroyIfHasSeed(){
		bool hasSeed=_mandible.childCount>0;
		if(!hasSeed)
			return false;
		Destroy(_mandible.GetChild(0).gameObject);
		return true;
	}

	bool ScanForNpb(){
		if(_npb!=null&&_npb._scanned)
			return false;
		RaycastHit hit;
		bool targeted=false;
		if(Physics.Raycast(_camera.position,_camera.forward,out hit, 5f,_birdLayer)){
			NPB curTarget=hit.transform.GetComponent<NPB>();
			if(_npb!=null&&curTarget!=_npb){
				_npb.Targeted(false);
			}
			_npb=curTarget;
			targeted=true;
			_npb.Targeted(true);
		}
		else{
			if(_npb!=null){
				_npb.Targeted(false);
				_npb=null;
			}
		}
		return targeted;
	}

	public void ScanBird(){
		//_npb.StartListening();
		_waddle.enabled=false;
		_hop.enabled=false;
		GameManager._mCam.Transition(_speciesCam,MCamera.Transitions.CUT_BACK,0.3f,_npb.transform,1f);
		_state=6;
		//_butts.SetActive(true);
		//TipHud.ShowTip("Press E to Return",transform,Vector3.up*0.5f);
		//_sing.SetTargetBird(_npb._sing);
		//species screen -> show Species
		SpeciesScreen.ShowSpecies(_npb);
	}

	void EndScan(){
		StartCoroutine(EndScanR());
		//TipHud.ClearTip();
		//_butts.SetActive(false);
		//_npb.StopListening();
		//_sing.Reset();
		//_npb._sing.Reset();
	}

	IEnumerator EndScanR(){
		float revertDur=1f;
		_sCam.Revert(revertDur);
		SpeciesScreen.Hide();
		yield return new WaitForSeconds(revertDur);
		_state=0;
		float lbDur=0.5f;
		GameManager._mCam.Transition(_waddleCam,MCamera.Transitions.CUT_BACK,0f,null,lbDur);
		yield return new WaitForSeconds(lbDur);
		_npb.EndScan();
	}

	public bool LearnSong(Sing.BirdSong song,NPB npb){
		foreach(Sing.BirdSong s in _songs){
			if(s._fileName==song._fileName)
			{
				return false;
			}
		}
		song._species=npb._species;
		if(npb._sing._male)
		{
			song._male=npb;
			song._female=npb._sing._mate._npb;
		}
		else{
			song._female=npb;
			song._male=npb._sing._mate._npb;
		}
		_songs.Add(song);
		return true;
	}

	public void EnterMenuState(){
		_waddle.enabled=false;
		_hop.enabled=false;
		_fly.enabled=false;
		_anim.SetFloat("walkSpeed",0f);
		_state=7;
	}

	public void SingSong(Sing.BirdSong song){
		Debug.Log("Time to start singing: "+song._fileName);
		GameManager._mCam.Transition(_singCam,MCamera.Transitions.CUT_BACK,0f,transform);
		//_sCam._offset.y=0.5f;

	}

	void OnDrawGizmos(){
	}
}
