using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Bird : MonoBehaviour
{
	public float _triggerRadius;
	public float _arriveRadius;
	public int _state;
	public int _startPath;
	Hop _hop;
	Fly _fly;
	Waddle _waddle;
	Tool _tool;
	Follow _follow;
	Tutorial _tutorial;
	Animator _anim;
	AudioSource _ruffleAudio;
	public bool _playerControlled;
	MInput _mIn;
	MCamera _mCam;
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
	public Transform _footprint;
	public Transform _wallprint;
	public Vector3 _footprintOffset;
	//int _leftRightPrint=1;
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
	public float _hitRadius;
	public Transform _mandible;
	Transform _curKey;
	Transform _curSeed;
	public int _seeds;
	public Transform _ruffleEffects;
	public float _summonDist;
	SkinnedMeshRenderer _smr;
	Vector3 _lastSpot;

	[Header("NPC - peck")]
	public float _peckCheckTime;
	public float _peckChance;
	float _peckTimer;
	Collider[] _cols;
	public float _flySpeed;
	public float _maxSpeed;
	public float _flapDur;

	Bird[] _birds;

	public Transform _band;
	public Color _bandColor;
	Bird _player;

	//cameras
	[Header("Cameras")]
	public Camera _waddleCam;
	public Camera _idleCam;
	public Camera _hopCam;
	public Camera _flyCam;

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
		_ruffleAudio=transform.Find("Ruffle").GetComponent<AudioSource>();
		_starParts=transform.Find("StarParts").GetComponent<ParticleSystem>();
		_mIn = GameManager._mIn;
		_mCam = GameManager._mCam;
		_cols = new Collider[3];
		_birds = FindObjectsOfType<Bird>();
		if(_playerControlled)
		{
			_waddleCam = transform.Find("WaddleCam").GetComponent<Camera>();
			_idleCam = transform.Find("IdleCam").GetComponent<Camera>();
			_hopCam = transform.Find("HopCam").GetComponent<Camera>();
			_flyCam = transform.Find("FlyCam").GetComponent<Camera>();
		}

		//disable things
		_hop.enabled=false;
		_fly.enabled=false;

		_lastSpot=transform.position;

		//band
		_band.GetComponent<MeshRenderer>().material.SetColor("_Color",_bandColor);

		//setup things
		_fly.SetupFlyShadow();

		_player=GameManager._player;

		if(_state==0)
			Ground();
		else if(!_playerControlled){
			_anim.SetTrigger("flyLoop");
		}
		_prevPos=transform.position;

		_hitRadius=GetComponent<SphereCollider>().radius*transform.localScale.x;
		Debug.Log("bird: "+name+" radius: "+_hitRadius);
	}

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
		_vel=transform.position-_prevPos;
		if(_playerControlled){
			//player update
			switch(_state){
				case 0://chilling
					Ground();
					if(_mIn.GetJump())
						StartHopping();
					else if(_mIn.GetControllerInput().sqrMagnitude>_controllerZero*_controllerZero)
						StartWaddling();
					if(_mIn.GetSingDown()){
						Call();
					}
					/*
					if(Input.GetButtonDown("Jump")){
						//Debug.Log("Jump time");
						Fly();
					}
					*/

					break;
				case 1://waddling
					if(!_waddle.IsWaddling()){
						StopWaddling();
						/*
						_state=0;
						_waddle.enabled=false;
						_anim.SetFloat("walkSpeed",0f);
						*/
					}
					else if(_waddle.IsKnockBack()){
						//just wait
					}
					else if(_mIn.GetJump()){
						StartHopping();
					}
					if(_mIn.GetSingDown()){
						Call();
					}
					/*
					if(Input.GetButtonDown("Jump")){
						//Debug.Log("Jump time");
						Fly();
					}
					*/
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
					if(_mIn.GetSingDown()){
						Call();
					}
					break;
				case 3://flying
					if(_mIn.GetSingDown()){
						Call();
					}
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
				case 6://using tool
					break;
				case 7://entering house
					break;
				case 8://feeding
					break;
				default:
					break;
			}
			if(Input.GetKeyDown(KeyCode.F1)){
				_waddle.ToggleCamLines();
			}
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
							RaycastHit hit;
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
    }

	public bool IsPlayerClose(Bird other){
		return (other.transform.position-transform.position).sqrMagnitude<_triggerRadius*_triggerRadius;
	}

	public bool IsPlayerInRange(Transform other,float rad){
		return (other.position-transform.position).sqrMagnitude<rad*rad;
	}

	public float Ruffle(){
		_anim.SetTrigger("ruffle");
		Instantiate(_ruffleEffects,transform.position+_size.y*0.5f*Vector3.up,Quaternion.identity,transform);
		//_state=2;
		_ruffleAudio.Play();
		return _ruffleAudio.clip.length;
	}

	public void Call(){
		//_anim.SetTrigger("sing");
		Transform call = Instantiate(_callEffects,transform.position+Vector3.up*_size.y,Quaternion.identity);
		AudioSource source = call.GetComponent<AudioSource>();
		source.clip=_call;
		source.pitch=Random.Range(_callPitchRange.x,_callPitchRange.y);
		source.Play();
		if(_playerControlled)
		{
			//#temp
			//this stuff should be reworked a bit
			//The logic for finding a new mate should depend on both bird and mate being within the same puzzle sphere
			//the logic for finding an existing mate, should verify that the target location is within puzzle sphere
			//basically we just need a quick check, what puzzle sphere is a bird in?
			//find existing mate
			foreach(Bird b in _birds)
			{
				if(b!=null&&b._mate==this){
					//only fly to a spot if it is within current puzzle bounds
					Vector3 diff=b.transform.position-transform.position;
					diff.y=0;
					b.FlyTo(transform.position+diff.normalized*_summonDist);
					return;
				}
			}
			//find new mate
			if(Physics.OverlapSphereNonAlloc(transform.position,2f,_cols,_birdLayer)>0){
				Bird b = _cols[0].GetComponent<Bird>();
				//b.ComeTo(transform);
				Vector3 diff=b.transform.position-transform.position;
				diff.y=0;
				b.FlyTo(transform.position+diff.normalized*_summonDist);
				b._mate=this;
			}

		}
		else{
			_anim.SetTrigger("sing");
		}
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

	public void StartHopping(){
		_state=2;
		_hop.enabled=true;
		_waddle.enabled=false;
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
		//GameManager._mCam.Transition(_idleCam,MCamera.Transitions.CUT_BACK);
		//Debug.Log("We stop waddling yooo");
	}

	public bool Arrived(){
		bool arrived=_hop.Arrived(_arriveRadius);
		if(arrived)
			Ground();
		return arrived;
	}

	public bool ArrivedW(){
		return _waddle.Arrived(_arriveRadius);
	}
	
	public bool IsWaddling(){
		return _waddle.IsWaddling();
	}

	void Fly(){
		//disable waddle
		_waddle.enabled=false;
		//disable hop
		_hop.enabled=false;
		//enable flight
		_fly.enabled=true;
		_state=3;
		_anim.SetTrigger("fly");
		//GameManager._mCam.Transition(_flyCam,MCamera.Transitions.LERP,0f,null,0.5f);
		GameManager._mCam.Transition(_flyCam,MCamera.Transitions.CUT_BACK);
	}
	public void Land(){
		SaveLastSpot();
		_fly.enabled=false;
		_state=0;
		_anim.SetFloat("walkSpeed",0f);
		_anim.SetTrigger("land");
		_hop.PlayStepParticles();
		//reset puzzle box cam - puzzle box needs to re-check if player is in zone after landing
		//because typically flight takes priority
		PuzzleBox._latestPuzzle.ResetCamera();

		//reset flight priority to 0
		_flyCam.GetComponent<FlyCam>().ResetPriority();

		Instantiate(_landParts,transform.position,Quaternion.identity);

		//which allows us to switch back to the idle cam
		GameManager._mCam.Transition(_waddleCam,MCamera.Transitions.CUT_BACK);
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
		return ;
		/*
		Transform fp = Instantiate(_footprint,transform.position,Quaternion.identity,surface);
		//orientate
		fp.forward=transform.forward;
		//fp.up=transform.forward;
		//offset footprint
		fp.position+=Vector3.up*_footprintOffset.y+transform.right*_footprintOffset.x*_leftRightPrint;
		fp.position+=offset*transform.forward;
		_leftRightPrint*=-1;
		*/
	}

	public void EquipFeather(Transform t){
		Material[] mats = _smr.materials;
		mats[1]=t.GetComponent<MeshRenderer>().materials[1];
		_smr.materials=mats;
	}

	IEnumerator PlayExplodeParticlesR(float vel){
		Instantiate(_explodeParts,transform.position,Quaternion.identity);
		yield return new WaitForSeconds(_divePartsDelay);
	}

	public void KnockBack(CollisionHelper ch, Vector3 dir,bool supress=false,bool ignoreNpc=false){
		if(ignoreNpc&&!_playerControlled)
			return;
		if(!supress){
			//add particel to collision
			Transform fp = Instantiate(_wallprint,ch._hitPoint,Quaternion.identity);
			//orientate
			fp.forward=-dir;
			//offset footprint
			float vertMult = _state==3?0.4f : 0.9f;
			fp.position+=dir*_footprintOffset.y*0.1f+Vector3.up*_size.y*vertMult;

			if(_state>1)
				_starParts.Play();
		}

		switch(_state){
			case 0:
			case 1://waddling
				ch.Sound(_waddleKnockVolume);
				_waddle.KnockBack(dir);
				break;
			case 2://hopping
				ch.Sound(_hopKnockVolume);
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
	}

	public bool GoingUp(){
		return _vel.y>0;
	}

	public Vector3 GetVelocity(){
		return _vel/Time.deltaTime;
	}

	public float GetVel(){
		return _vel.magnitude/Time.deltaTime;
	}

	public void CollectKey(Transform t){
		t.SetParent(_mandible);
		t.localPosition=Vector3.zero;
		t.localEulerAngles=Vector3.up*90f;
		t.localScale=Vector3.one*0.25f;
		_curKey=t;
	}

	public void CollectSeed(Transform t){
		_anim.SetTrigger("peck");
		_seeds++;
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

	public bool HasSeed(){
		return _curSeed!=null;
	}

	public Transform GiveSeed(){
		Transform seed=_curSeed;
		_curSeed=null;
		return seed;
	}

	public Transform GetSeed(){
		return _curSeed;
	}


	public void Ground(bool resetState=true){
		if(resetState){
			_waddle.enabled=false;
			_hop.enabled=false;
			_fly.enabled=false;
			_anim.SetFloat("walkSpeed",0f);
			_state=0;
		}
		RaycastHit hit;
		/*
		if(Physics.SphereCast(transform.position+_size.y*Vector3.up,0.05f,Vector3.down, out hit, 1f, _collisionLayer))
			transform.position=hit.point;
			*/
		if(Physics.Raycast(transform.position+_size.y*Vector3.up,Vector3.down, out hit,1f,_collisionLayer)){
			if(Mathf.Abs(hit.point.y-transform.position.y)>0.25f)
			{
				//Debug.Log("Oh snap");
			}
			else
				transform.position=hit.point;
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

	public void GainSeed(){
		_seeds++;
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
					_fly.FlapSounds();
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

	public void SetSeeds(int s){
		_seeds=s;
		if(!_playerControlled&&s>0)
			_smr.enabled=true;
	}

	public void PartySnacks(){
		if(Physics.OverlapSphereNonAlloc(transform.position,2f,_cols,_birdLayer)>0){
			Bird b = _cols[0].GetComponent<Bird>();
		}
	}

	public void FlyToNextPuzzle(){
		PuzzleBox latest = PuzzleBox._latestPuzzle;
		Transform perch=latest.GetPerch();
		FlyTo(perch.position);
	}

	public void Drown(){
		_state=7;
		_anim.SetFloat("walkSpeed",0f);
		if(_fly.enabled)
		{
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

	public void FlyAwayMates(){
		foreach(Bird b in _birds)
		{
			if(b!=null&&b._mate==this){
				b.FlyAway();
			}
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
	public void WalkInNestBox(Transform t,GameObject nb){
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
		Vector3 dir = -t.forward;
		Vector3 pos=transform.position;
		Vector3 newPos=t.position+t.forward*0.25f;
		newPos.y=pos.y;
		transform.position=newPos;
		Debug.Log("Setting pos on enter box");
		//set state to something special
		_state=7;
		StartCoroutine(WalkThroughDoorR(dir,nb));
	}

	IEnumerator WalkThroughDoorR(Vector3 dir,GameObject nb){
		float timer=0;
		transform.forward=dir;
		_anim.SetFloat("walkSpeed",0.1f);
		float dur = 0.4f;
		while(timer<dur){
			timer+=Time.deltaTime;
			transform.position+=dir*Time.deltaTime*_waddle._walkSpeed*0.5f;
			yield return null;
		}
		_posBeforeNestBox=transform.position;
		_anim.SetFloat("walkSpeed",0f);

		//transition to nest box
		dur = 3f;
		float halfDur=dur*0.5f;
		Camera nestCam = nb.GetComponentInChildren<Camera>();
		_camBeforeNestBox=GameManager._mCam.GetCurTargetCam();
		GameManager._mCam.Transition(nestCam,MCamera.Transitions.FADE,0,null,dur);

		yield return new WaitForSeconds(halfDur);
		nb.SetActive(true);
		Transform startT = MUtility.FindRecursive(nb.transform,"PlayerStart");
		transform.position=startT.position;
		transform.rotation=startT.rotation;
		Ground();
		GameManager._islands.SetActive(false);
		_state=0;
	}

	public void WalkOutNestBox(Transform t,GameObject nb){
		if(_fly.enabled)
		{
			_fly.Soar(false);
			Land();
		}
		//start coroutine
		Vector3 dir = t.forward;
		StartCoroutine(WalkOutDoorR(dir,nb));
	}

	IEnumerator WalkOutDoorR(Vector3 dir,GameObject nb){
		float dur = 3f;
		float halfDur=dur*0.5f;
		GameManager._mCam.Transition(_camBeforeNestBox,MCamera.Transitions.FADE,0,null,dur,true);

		yield return new WaitForSeconds(halfDur);
		nb.SetActive(false);
		GameManager._islands.SetActive(true);
		_waddle.enabled=false;
		_hop.enabled=false;
		_fly.enabled=false;
		//enabled=false;
		_state=7;
		transform.position=_posBeforeNestBox;
		transform.forward=dir;
		yield return null;
		if(transform.position!=_posBeforeNestBox){
			Debug.Log("Error! position is wack!: "+transform.position);
		}

		float timer=0;
		_anim.SetFloat("walkSpeed",0.1f);
		dur = 0.8f;
		while(timer<dur){
			timer+=Time.deltaTime;
			transform.position+=dir*Time.deltaTime*_waddle._walkSpeed*0.5f;
			yield return null;
		}
		_anim.SetFloat("walkSpeed",0f);

		Ground();
		_state=0;
	}

	IEnumerator ResetAfterDelay(float dur){
		yield return new WaitForSeconds(dur);
		_state=0;
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.magenta;
		Gizmos.DrawWireSphere(transform.position,_triggerRadius);
	}
}
