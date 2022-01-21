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
	RunAway _runAway;
	Hunger _hunger;
	Follow _follow;
	Tutorial _tutorial;
	Animator _anim;
	AudioSource _ruffleAudio;
	public bool _playerControlled;
	MCamera _mCam;
	MInput _mIn;
	public Bird _mate;
	[HideInInspector]
	public Vector3 _size;
	[Header("Call")]
	public Transform _callEffects;
	public Vector2 _callPitchRange;
	public LayerMask _collisionLayer;
	public float _controllerZero;
	[Header("Footprints")]
	public Transform _footprint;
	public Vector3 _footprintOffset;
	int _leftRightPrint=1;
	[Header("Explosion")]
	public float _afterDiveDelay;
	float _afterDiveTimer;
	public Transform _explodeParts;
	public float _divePartsDelay;
	[Header("Collisions")]
	public float _shakeDelay;
	float _shakeTimer;
	ParticleSystem _starParts;
	public float _waddleKnockVolume;
	public float _hopKnockVolume;
	Vector3 _prevPos;
	public Transform _mandible;
	Transform _curKey;
	Transform _curSeed;
	public int _seeds;
	public Transform _ruffleEffects;
	public float _summonDist;
	[HideInInspector]
	public Transform _puzzleZone;
	SkinnedMeshRenderer _smr;

	[Header("Grand entrance")]
	public UnityEvent _onGrandEntrance;
	public float _grandWaddleSpeed;
	public float _colorChangeDur;
	public UnityEvent _onGrandEntranceEnd;
	public bool _enterGrandly;

	public delegate void EventHandler();
	public event EventHandler _onCall;

	void Awake(){
		//calculations
		_smr = transform.GetComponentInChildren<SkinnedMeshRenderer>();
		_size=_smr.sharedMesh.bounds.size*_smr.transform.localScale.x*transform.localScale.x;

		//get references
		_hop=GetComponent<Hop>();
		_fly=GetComponent<Fly>();
		_waddle=GetComponent<Waddle>();
		_tool=GetComponent<Tool>();
		_runAway=GetComponent<RunAway>();
		_hunger=GetComponent<Hunger>();
		_follow=GetComponent<Follow>();
		_tutorial=GetComponentInChildren<Tutorial>();
		_anim=GetComponent<Animator>();
		_ruffleAudio=transform.Find("Ruffle").GetComponent<AudioSource>();
		_starParts=transform.Find("StarParts").GetComponent<ParticleSystem>();
		_mCam = Camera.main.transform.parent.GetComponent<MCamera>();
		_mIn = _mCam.GetComponent<MInput>();

		//disable things
		_hop.enabled=false;
		_fly.enabled=false;

		if(!_playerControlled)
		{
			GameObject player = GameObject.FindGameObjectWithTag("Player");
			_mate=player.GetComponent<Bird>();
			_mate._mate=this;
		}

		_state=0;
	}

    // Start is called before the first frame update
    void Start()
    {
		if(!_playerControlled){
			if(_enterGrandly)
				GrandEntrance();
			else
				ComeToPlayer();
		}
    }

    // Update is called once per frame
    void Update()
    {
		if(_playerControlled){
			//player update
			switch(_state){
				case 0://chilling
				default:
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
						_state=0;
						_waddle.enabled=false;
						_anim.SetFloat("walkSpeed",0f);
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
							//go to idle
							_state=0;
							_hop.enabled=false;
							_anim.SetFloat("walkSpeed",0f);
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
			}
		}
		else{
			switch(_state){
				case 0:
				default://chilling
					break;
				case 1://run away
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
		source.pitch=Random.Range(_callPitchRange.x,_callPitchRange.y);
		if(_playerControlled)
		{
			if(_onCall!=null)
				_onCall.Invoke();
		}
		else{
			_anim.SetTrigger("sing");
		}
	}

	public void ComeToPlayer(){
		Vector3 diff=transform.position-_mate.transform.position;
		//_mate.WaddleTo(transform.position-diff.normalized*_summonDist,1f);
		transform.position=_mate.transform.position+diff.normalized*_summonDist;
	}

	public void HopTo(Vector3 loc){
		_hop.enabled=true;
		_hop.HopTo(loc);
		_state=2;
	}

	public void WaddleTo(Vector3 loc,float speed){
		_waddle.enabled=true;
		_waddle.WaddleTo(loc,speed);
		_state=1;
	}

	public void StartHopping(){
		_state=2;
		_hop.enabled=true;
		_waddle.enabled=false;
	}

	void StartWaddling(){
		_state=1;
		_waddle.enabled=true;
		_hop.enabled=false;
	}

	public bool IsHopping(){
		return _hop.enabled&&_hop.IsHopping();
	}

	public void StopHopping(){
		_hop.StopHopping();
		_hop.enabled=false;
		_anim.SetFloat("walkSpeed",0f);
		_state=0;
	}

	public void StopWaddling(){
		_waddle.StopWaddling();
		_state=0;
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
	}
	public void Land(){
		_fly.enabled=false;
		_state=0;
		_anim.SetFloat("walkSpeed",0f);
		_anim.SetTrigger("land");
		_hop.PlayStepParticles();
	}

	public void Dive(float vel){
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

	public void MakeFootprint(float offset=0){
		Transform fp = Instantiate(_footprint,transform.position,Quaternion.identity);
		//orientate
		fp.forward=Vector3.down;
		//offset footprint
		fp.position+=Vector3.up*_footprintOffset.y+transform.right*_footprintOffset.x*_leftRightPrint;
		fp.position+=offset*transform.forward;
		_leftRightPrint*=-1;
	}

	public void EquipFeather(Transform t){
		Material[] mats = _smr.materials;
		mats[1]=t.GetComponent<MeshRenderer>().materials[1];
		_smr.materials=mats;
	}

	IEnumerator PlayExplodeParticlesR(float vel){
		Instantiate(_explodeParts,transform.position,Quaternion.identity);
		yield return new WaitForSeconds(_divePartsDelay);
		//_mCam.Shake(vel);
	}

	public void KnockBack(CollisionHelper ch, Vector3 dir,bool supress=false,bool ignoreNpc=false){
		if(ignoreNpc&&!_playerControlled)
			return;
		if(!supress){
			//add particel to collision
			Transform fp = Instantiate(_footprint,ch._hitPoint,Quaternion.identity);
			//orientate
			fp.forward=-dir;
			//offset footprint
			float vertMult = _state==3?0.4f : 0.9f;
			fp.position+=dir*_footprintOffset.y*0.1f+Vector3.up*_size.y*vertMult;

			_starParts.Play();
		}

		switch(_state){
			case 0:
			default:
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
		}
	}

	public void ShakeItOff(){
		_waddle.enabled=false;
		_hop.enabled=false;
		_fly.enabled=false;
		Debug.Log("Shaking it off");
		_anim.SetFloat("walkSpeed",0f);
		_shakeTimer=0;
		_state=5;
	}

	public void RevertToPreviousPosition(){
		//move back to previous frame, to prevent collider from getting stuck overlapping
		transform.position=_prevPos;
	}

	public void CollectKey(Transform t){
		t.SetParent(_mandible);
		t.localPosition=Vector3.zero;
		t.localEulerAngles=Vector3.up*90f;
		t.localScale=Vector3.one*0.25f;
		_curKey=t;
	}

	public void CollectSeed(Transform t){
		/*
		if(t==null)
			Debug.Log("oopsies");
		t.SetParent(_mandible);
		t.localPosition=Vector3.zero;
		t.localEulerAngles=Vector3.up*90f;
		t.localScale=Vector3.one*0.25f;
		_curSeed=t;
		if(_playerControlled)
			_mCam.Surround(_mate.transform);
			*/
		//Destroy(t.gameObject);
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

	public bool IsRunningAway(){
		return _runAway.enabled;
	}

	public void Ground(){
		_waddle.enabled=false;
		_hop.enabled=false;
		_fly.enabled=false;
		_anim.SetFloat("walkSpeed",0f);
		_state=0;
		RaycastHit hit;
		if(Physics.Raycast(transform.position+_size.y*Vector3.up,Vector3.down, out hit,1f,_collisionLayer)){
			transform.position=hit.point;
		}
	}

	public void TakeSeedFromMate(){
		Transform seed = _mate.GiveSeed();
		CollectSeed(seed);
	}

	public void RunAwayNextPath(){
		_runAway.RunAwayNextPath();
	}

	public void StopRunningAway(){
		if(_runAway!=null)
			_runAway.enabled=false;
	}

	public Vector3 GetCamTarget(){
		if(_state==2)
			return _hop.GetCamTarget();
		else
			return transform.position;
	}

	public void CallToMate(){
		//Debug.Log(name + " got walked into by: "+b.name);
		Vector3 diff = _mate.transform.position-transform.position;
		diff.y=0;
		transform.forward=diff;
		Call();
		//Ruffle();
	}

	public void RuffleToMate(){
		//if(_hunger.IsEating())
		//	return;
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

	public void SetZone(Transform t){
		_puzzleZone=t;
		if(!_playerControlled && _seeds>0){
			if(t!=null&&_puzzleZone==_mate._puzzleZone)
				StopFollowing();
			else
				StartFollowing();
		}
	}

	public void FlyTo(Vector3 target, float dur){
		_smr.enabled=true;
		StartCoroutine(FlyToR(target,dur));
		_anim.SetTrigger("flyLoop");
	}

	IEnumerator FlyToR(Vector3 target, float dur){
		Vector3 start=transform.position;
		transform.LookAt(target);
		Vector3 eulers = transform.eulerAngles;
		eulers.x=0;
		transform.eulerAngles=eulers;
		Vector3 end = target;
		float timer=0;
		while(timer<dur){
			timer+=Time.deltaTime;
			transform.position=Vector3.Lerp(start,end,timer/dur);
			yield return null;
		}
		_anim.SetTrigger("land");
	}

	public void SetSeeds(int s){
		_seeds=s;
		if(!_playerControlled&&s>0)
			_smr.enabled=true;
	}

	public void GrandEntrance(){
		//_onGrandEntrance.Invoke();
		StartCoroutine(GrandEntranceR());
	}

	IEnumerator GrandEntranceR(){
		_mIn.LockInput(true);
		_mCam.Surround(transform);
		_mCam.LetterBox(true);

		//oreint mate
		_mate.transform.LookAt(transform);
		Vector3 eulers = _mate.transform.eulerAngles;
		eulers.x=0;
		_mate.transform.eulerAngles=eulers;

		//fly to ground
		float dur=2f;
		Debug.Log("We flying");
		FlyTo(_mate.transform.position+_mate.transform.forward,dur);
		yield return new WaitForSeconds(dur);

		//waddle to spot
		WaddleTo(_mate.transform.position,_grandWaddleSpeed);
		Debug.Log("We waddling");
		//float colorTimer=0f;
		//ColorFilter cf = Camera.main.GetComponent<ColorFilter>();
		//cf.NextVisionLevel();
		//colorFilter.SetVisionLevel++
		//float colorDur=_colorChangeDur;
		while(!ArrivedW())
		{
			//colorTimer+=Time.deltaTime;
			//cf.LerpMask(colorTimer/colorDur);
			//colorFilter.LerpMask(colorTimer/colorDur)
			yield return null;
		}

		_mIn.LockInput(false);
		_mCam.DefaultCam();
		_mCam.LetterBox(false);
	}

	public void GrandExit(){
		StartCoroutine(GrandExitR());
	}

	IEnumerator GrandExitR(){
		Vector3 target=_mate.transform.position-_mate.transform.forward*3f+Vector3.up*3f;

		//fly to target
		float dur=2f;
		Debug.Log("We flying");
		FlyTo(target,dur);
		yield return new WaitForSeconds(dur);

		Destroy(gameObject);
	}

	public void PartySnacks(){
		if(_mate!=null)
			_mate.GetSomeSeeds();
	}

	public void GetSomeSeeds(){
		_hunger.enabled=true;
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.magenta;
		Gizmos.DrawWireSphere(transform.position,_triggerRadius);
	}
}
