using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldBird : MonoBehaviour
{
	[Header("Species")]
	public string _species;
	Animator _anim;
	//[HideInInspector]
	public int _state;
	Transform _flyTarget;
	Perch _prevPerch;
	Perch _perchTarget;
	Feeder _curFeeder;
	[Header("Flying")]
	public float _flySpeed;
	public float _landSpeed;
	float _landTimer;
	float _landTime;
	Vector3 _dir;//#temp
	Vector3 _pos;//#temp - maybe prevDir and prevPos;
	Quaternion _prevRot;
	Quaternion _targetRot;
	public AudioClip _fleeSound;
	[Header("Eating")]
	public float _peckCheckTime;
	public float _peckChance;
	float _peckTimer;
	float _peckCheckTimer;
	static Feeder[] _feeders;
	public float _maxTurnAngle;//
	[Header("Energy")]
	public int _energy;
	public int _minEnergy;
	public int _idealEnergy;
	[Header("Singing")]
	public AudioClip _song;
	AudioSource _audio;
	float _singTimer;
	public Vector2 _pitchRange;
	float _pitch;
	[Header("Idling")]
	public float _idleCheckTime;
	[Range(0,1)]
	public float _ruffleChance;
	[Range(0,1)]
	public float _preenChance;
	[Range(0,1)]
	public float _reposChance;//not fleeing from anything in particular yet...
	float _idleTimer;
	float _idleCheckTimer;
	GameObject _curTree;
	public bool _startInTree;
	[Header("Startle")]
	public float _startleRunRange;
	public float _startleWalkRange;
	public float _startleSpeed;
	public float _fleeDistance;
	public AudioClip _fleeAlarm;
	public float _startleTime;
	float _startleTimer;
	[Header("Social")]
	public int _rank;
	static List<OldBird> _birds;

    // Start is called before the first frame update
    void Start()
    {
		_anim=GetComponent<Animator>();
		_audio = GetComponent<AudioSource>();
		Speaker._instance.OnSpeakerPlay+=HandleSound;
		//#slow
		Footstep [] steps = FindObjectsOfType<Footstep>();
		foreach(Footstep s in steps)
			s.OnFootstep+=HandleFootstep;

		//get static bird list
		if(_birds==null)
		{
			OldBird [] tBirds = FindObjectsOfType<OldBird>();
			_birds = new List<OldBird>();
			foreach(OldBird b in tBirds)
				_birds.Add(b);
		}
		if(_startInTree&&_rank==0){
			//if leader, pick tree
			FindRandomTree();
			FindRandomPerchInTree(_curTree);
			//FindRandomTreePerch();
			transform.position=_flyTarget.position;
			transform.rotation=_flyTarget.rotation;
			StartCoroutine(SendJoinCallR());
		}
		_pitch = Random.Range(_pitchRange.x,_pitchRange.y);
		_audio.pitch=_pitch;

		//get static feeder array
		_feeders = FindObjectsOfType<Feeder>();
		
		//random offset for idle check
		_idleCheckTimer=_idleCheckTime*Random.value;
    }

    // Update is called once per frame
    void Update()
    {
		switch(_state){
			case 0://idle
			default:
				_idleCheckTimer-=Time.deltaTime;
				if(_idleCheckTimer<=0){
					if(TimeManager._instance.IsNight())
					{
						//Sleep();
						_state=6;
					}
					else if(_energy<_minEnergy)
					{
						FindFeeder();
						StartFlyingToTarget();
					}
					else{
						float r = Random.value;
						if(r<_ruffleChance){
							//ruffle
							_anim.SetTrigger("ruffle");
						}
						else if(r<_ruffleChance+_preenChance){
							//preen
							_anim.SetTrigger("preen");
						}
						else if(r<_ruffleChance+_preenChance+_reposChance){
							//repose
							//FindRandomTreePerch();
							FindRandomPerchInTree(_curTree);
							StartFlyingToTarget();
						}
					}
					_idleCheckTimer=_idleCheckTime;
				}
				break;
			case 1://flying
				transform.position+=transform.forward*Time.deltaTime*_flySpeed;
				//start landing when close
				if((transform.position-_flyTarget.position).sqrMagnitude<0.09f){
					_state=2;
					_anim.SetBool("flying",false);
					_dir = _flyTarget.position-transform.position;
					_pos = transform.position;
					_landTime = _dir.magnitude/_landSpeed;
					_dir.Normalize();
					_landTimer=0;
					_prevRot = transform.rotation;
					_targetRot = _flyTarget.rotation;
					_audio.clip=_fleeSound;
					_audio.Play();
				}
				break;
			case 2://landing
				transform.position+=_dir*Time.deltaTime*_landSpeed;
				_landTimer+=Time.deltaTime;
				float t = _landTimer/_landTime;
				transform.rotation = Quaternion.Slerp(_prevRot,_targetRot,t);
				if(t>=1f){
					transform.position=_flyTarget.position;
					if(_flyTarget.parent!=null){
						Vector3 looky=_flyTarget.parent.position;
						looky.y=_flyTarget.position.y;
						transform.LookAt(looky);
					}
					//sing on arrival
					//Sing();
					ArriveAtPerch();
				}
				break;
			case 3://sing
				_singTimer-=Time.deltaTime;
				if(_singTimer<=0)
				{
					ArriveAtPerch();
				}
				break;
			case 4://feeding
				_peckCheckTimer-=Time.deltaTime;
				if(_peckCheckTimer<=0){
					if(Random.value<_peckChance){
						_energy++;
						if(_energy>=_idealEnergy)
						{
							Debug.Log(" done feeding at feeder");
							_state=0;
							_anim.SetInteger("hop",0);
							//fly to a perch on a tree
							FindRandomTreePerch();
							StartFlyingToTarget();
						}
						else
							_anim.SetTrigger("peck");
					}
					else if(_curFeeder._forage){
						//hop around
						transform.Rotate(Vector3.up*Random.Range(-_maxTurnAngle,_maxTurnAngle));
						//StartCoroutine(HopR());
						_anim.SetInteger("hop",1);
					}
					_peckCheckTimer=_peckCheckTime;
				}
				else
					_peckCheckTimer-=Time.deltaTime;
				break;
			case 5://startled
				if(_startleTimer>0){
					//Debug.Log("startlin yo "+_startleTimer);
					_startleTimer-=Time.deltaTime;
					if(_startleTimer<=0){
						if(_rank==0)
							StartCoroutine(SendJoinCallR());
						else
							_state=0;
					}
				}
				break;
			case 6://sleep
				//do nothing
				break;
		}
    }

	void StartFlyingToTarget(bool playFlee=true){
		if(_flyTarget==null || _perchTarget==null)
			return;
		if(_prevPerch!=null)
			_prevPerch._occupied=false;
		_anim.SetBool("flying",true);
		transform.LookAt(_flyTarget);
		_state = 1;
		_energy--;
		if(playFlee)
		{
			_audio.clip=_fleeSound;
			_audio.Play();
		}
		//set perch occupied on flight start
		_perchTarget._occupied=true;
	}

	void Sing(){
		_state=3;
		_singTimer=3f;
		_anim.SetTrigger("sing");
		_audio.clip=_song;
		_audio.Play();
	}

	void HandleSound(Speaker.SpeakerEventArgs args){
		//only really want the closest bird responding
		float minSqrDist=10000f;
		OldBird close=null;
		foreach(OldBird b in _birds)
		{
			if(args.name.ToLower().Contains(_species.ToLower())&&b._state==0||b._state==4)
			{
				float sqd = (args.pos-b.transform.position).sqrMagnitude;
				if(sqd<minSqrDist)
				{
					minSqrDist=sqd;
					close=b;
				}
			}
		}
		if(close!=this)
			return;
		StartCoroutine(SingAfterSecondsR(args.dur+Random.value));
	}

	IEnumerator SingAfterSecondsR(float s){
		yield return new WaitForSeconds(s);
		if(_state==0||_state==4)
		{
			Sing();
		}
	}

	void HandleFootstep(Footstep.FootstepEventArgs args){
		if(_state==1)
			return;
		if(args.alerted){
			//alerted by another bird's alarm
			FindRandomSafeTreePerch(args.pos);
			StartFlyingToTarget();
		}
		else{
			//alerted by sound of footsteps alone
			float sqrDist=(args.pos-transform.position).sqrMagnitude;
			if(sqrDist<=_startleRunRange*_startleRunRange){
				if(args.speed>=_startleSpeed||(sqrDist<=_startleWalkRange*_startleWalkRange&&args.speed>0))
				{
					Debug.Log("Startled!");
					StartCoroutine(SignalAlarmR(args));
				}
			}
		}
	}

	IEnumerator SignalAlarmR(Footstep.FootstepEventArgs args){
		//sing alarm song
		_audio.clip=_fleeAlarm;
		_audio.loop=false;
		_audio.Play();
		//fly simultaneously
		FindRandomSafeTreePerch(args.pos);
		StartFlyingToTarget(false);
		//wait for alarm song
		yield return new WaitForSeconds(_audio.clip.length);
		//play fly sound
		_audio.clip=_fleeSound;
		_audio.Play();
		//alert other birds
		args.alerted=true;
		foreach(OldBird b in _birds){
			if(b!=this){
				b.HandleFootstep(args);
			}
		}
	}

	void FindFeeder(){
		//GameObject [] feeders = GameObject.FindGameObjectsWithTag("Feeder");
		//check for man-made feeders
		//give preference - at least in winter
		foreach(Feeder f in _feeders){
			if(!f._forage){
				Perch p = f.HasSpace();
				if(p!=null)
				{
					_perchTarget=p;
					_flyTarget=p.transform;
					return;
				}
			}
		}
		Feeder f2 = _feeders[Random.Range(0,_feeders.Length)];
		Perch p2 = f2.HasSpace();
		int iters=0;
		while(p2==null&&iters<10){
			f2 = _feeders[Random.Range(0,_feeders.Length)];
			p2 = f2.HasSpace();
			iters++;
		}
		_perchTarget=p2;
		_flyTarget=p2==null? null : _perchTarget.transform;
	}

	void FindRandomTree(){
		GameObject [] trees = GameObject.FindGameObjectsWithTag("Tree");
		GameObject tree = trees[Random.Range(0,trees.Length)];
		_curTree=tree;
	}
	void FindRandomPerchInTree(GameObject tree){
		//get perch
		Transform perch = tree.transform.GetChild(
				Random.Range(0,tree.transform.childCount));
		//make sure perch is unoccupied
		int iters=0;
		while(perch.GetComponent<Perch>()._occupied&&iters<20){
			perch = tree.transform.GetChild(
					Random.Range(0,tree.transform.childCount));
			iters++;
		}
		if(perch.GetComponent<Perch>()._occupied){
			FindRandomTree();
			FindRandomPerchInTree(_curTree);
			return;
		}

		_flyTarget=perch;
		_perchTarget=_flyTarget.GetComponent<Perch>();
		_perchTarget._occupied=true;
	}

	//find random perch in a tree
	void FindRandomTreePerch(){
		GameObject [] trees = GameObject.FindGameObjectsWithTag("Tree");
		GameObject tree = trees[Random.Range(0,trees.Length)];
		while(_curTree!=null && tree==_curTree)
			tree = trees[Random.Range(0,trees.Length)];
		//get perch
		Transform perch = tree.transform.GetChild(
				Random.Range(0,tree.transform.childCount));
		//make sure perch is unoccupied
		while(perch.GetComponent<Perch>()._occupied){
			perch = tree.transform.GetChild(
					Random.Range(0,tree.transform.childCount));
		}
		_flyTarget=perch;
		_perchTarget=_flyTarget.GetComponent<Perch>();
		_perchTarget._occupied=true;
	}

	void FindRandomSafeTreePerch(Vector3 threat){
		_startleTimer=_startleTime;
		//skip if already in safe pos
		if(_flyTarget!=null&&(threat-_flyTarget.position).sqrMagnitude>_fleeDistance*_fleeDistance)
			return;
		GameObject [] trees = GameObject.FindGameObjectsWithTag("Tree");
		GameObject tree = trees[Random.Range(0,trees.Length)];
		int iters=0;
		while((tree.transform.position-threat).sqrMagnitude<_fleeDistance*_fleeDistance&&
				iters<10){
			tree = trees[Random.Range(0,trees.Length)];
			iters++;
		}
		//get perch
		Transform perch = tree.transform.GetChild(
				Random.Range(0,tree.transform.childCount));
		//make sure perch is unoccupied
		iters=0;
		while(perch.GetComponent<Perch>()._occupied&&iters<10){
			perch = tree.transform.GetChild(
					Random.Range(0,tree.transform.childCount));
			iters++;
		}
		_flyTarget=perch;
		_perchTarget=_flyTarget.GetComponent<Perch>();
		_perchTarget._occupied=true;
	}


	IEnumerator FlapR(){
		_anim.SetBool("flying",true);
		_audio.clip=_fleeSound;
		_audio.Play();
		yield return new WaitForSeconds(0.5f);
		_anim.SetBool("flying",false);
	}

	IEnumerator SendJoinCallR(){
		//sing join song
		_audio.clip=_song;
		_audio.Play();
		_state=0;
		yield return null;
		foreach(OldBird b in _birds){
			if(b!=this){
				b.GoToTree(_curTree);
			}
		}
	}

	public void GoToTree(GameObject tree){
		_startleTimer=0;
		_curTree=tree;
		FindRandomPerchInTree(_curTree);
		StartFlyingToTarget();
	}

	void ArriveAtPerch(){
		if(_flyTarget==null || _flyTarget.parent.GetComponent<Tree>()!=null){
			//go to startle state if arrived after a flee call
			if(_startleTimer>0)
				_state=5;
			else
				_state=0;
			if(_flyTarget!=null)
				_curTree=_flyTarget.parent.gameObject;
		}
		else if(_flyTarget.parent.GetComponent<Feeder>()!=null)
		{
			_state=4;
			_curTree=null;
			_curFeeder=_flyTarget.parent.GetComponent<Feeder>();
		}
		_prevPerch=_perchTarget;
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.blue;
		if(_flyTarget!=null)
			Gizmos.DrawSphere(_flyTarget.position,0.01f);
	}
}
