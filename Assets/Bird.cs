using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
	AudioSource _callAudio;
	public bool _playerControlled;
	MCamera _mCam;
	MInput _mIn;
	public Bird _mate;
	[HideInInspector]
	public Vector3 _size;
	ParticleSystem _callParts;
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

	void Awake(){
		//calculations
		SkinnedMeshRenderer smr = transform.GetComponentInChildren<SkinnedMeshRenderer>();
		_size=smr.sharedMesh.bounds.size*smr.transform.localScale.x*transform.localScale.x;

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
		_callAudio=transform.Find("Call").GetComponent<AudioSource>();
		_callParts=_callAudio.GetComponent<ParticleSystem>();
		_starParts=transform.Find("StarParts").GetComponent<ParticleSystem>();
		_mCam = Camera.main.transform.parent.GetComponent<MCamera>();
		_mIn = _mCam.GetComponent<MInput>();

		//disable things
		_hop.enabled=false;
		_fly.enabled=false;

		//#temp - may not always want to start with runAway
		if(!_playerControlled){
			_runAway.enabled=false;
		}
		
		_state=0;
	}

    // Start is called before the first frame update
    void Start()
    {
		if(!_playerControlled){
			//_runAway.RunAwayOnPath(_startPath);
			//_follow.enabled=false;
			_seeds=1;
			StartFollowing();
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
					if(Input.GetButtonDown("Sing")){
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
					if(Input.GetButtonDown("Sing")){
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
					if(Input.GetButtonDown("Sing")){
						Call();
					}
					break;
				case 3://flying
					if(Input.GetButtonDown("Sing")){
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

			if(Input.GetButtonDown("Pause"))
			{
				SceneManager.LoadScene(SceneManager.GetActiveScene().name);
				/*
#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
#else
				Application.Quit();
#endif
*/
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

	public float Ruffle(){
		_anim.SetTrigger("ruffle");
		Instantiate(_ruffleEffects,transform.position+_size.y*0.5f*Vector3.up,Quaternion.identity,transform);
		//_state=2;
		_ruffleAudio.Play();
		return _ruffleAudio.clip.length;
	}

	public void Call(){
		//StartCoroutine(SpawnSoundRings(_numSoundRings));
		//_anim.SetTrigger("sing");
		_callAudio.Play();
		_callParts.Play();
		//_state=3;
		if(_playerControlled)
		{
			//StartCoroutine(CallMateR());
			if(_mate._seeds>0&&!_mate.IsRunningAway())
			{
				Vector3 diff=transform.position-_mate.transform.position;
				_mate.WaddleTo(transform.position-diff.normalized*_summonDist,1f);
			}
		}
		else{
			_anim.SetTrigger("sing");
		}
	}

	IEnumerator CallMateR(){
		float dur = _callAudio.clip.length;
		yield return new WaitForSeconds(dur);
		_mate.Call();
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
		SkinnedMeshRenderer smr = transform.GetComponentInChildren<SkinnedMeshRenderer>();
		Material[] mats = smr.materials;
		mats[1]=t.GetComponent<MeshRenderer>().materials[1];
		smr.materials=mats;
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
		if(t==null)
			Debug.Log("oopsies");
		t.SetParent(_mandible);
		t.localPosition=Vector3.zero;
		t.localEulerAngles=Vector3.up*90f;
		t.localScale=Vector3.one*0.25f;
		_curSeed=t;
		if(_playerControlled)
			_mCam.Surround(_mate.transform);
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
		if(_hunger.IsEating())
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

	void OnDrawGizmos(){
		Gizmos.color=Color.magenta;
		Gizmos.DrawWireSphere(transform.position,_triggerRadius);
	}
}
