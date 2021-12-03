using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour
{
	[Header("Species")]
	public string _species;
	Animator _anim;
	int _state;
	Transform _flyTarget;
	[Header("Flying")]
	public float _flySpeed;
	public float _landSpeed;
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
	[Header("Energy")]
	public int _energy;
	public int _minEnergy;
	public int _idealEnergy;
	[Header("Singing")]
	public AudioClip _song;
	AudioSource _audio;
	float _singTimer;
	[Header("Idling")]
	public float _idleCheckTime;
	[Range(0,1)]
	public float _ruffleChance;
	[Range(0,1)]
	public float _preenChance;
	[Range(0,1)]
	public float _fleeChance;//not fleeing from anything in particular yet...
	float _idleTimer;
	float _idleCheckTimer;
	GameObject _curTree;
	[Header("Startle")]
	public float _startleRunRange;
	public float _startleWalkRange;
	public float _startleSpeed;
	public float _fleeDistance;
	public AudioClip _fleeAlarm;
	[Header("Social")]
	public int _rank;
	static List<Bird> _birds;

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
			Bird[] tBirds = FindObjectsOfType<Bird>();
			_birds = new List<Bird>();
			foreach(Bird b in tBirds)
				_birds.Add(b);
		}
    }

    // Update is called once per frame
    void Update()
    {
		switch(_state){
			case 0://idle
			default:
				_idleCheckTimer-=Time.deltaTime;
				if(_idleCheckTimer<=0){
					if(_energy<_minEnergy)
					{
						//forage
						//#temp - maybe should be more logic than just picking random feeder
						//what if it knows a feeder is empty?
						//what if there's another bird at the feeder?
						//what if there's a new feeder it doesn't know about?
						GameObject [] feeders = GameObject.FindGameObjectsWithTag("Feeder");
						GameObject feeder = feeders[Random.Range(0,feeders.Length)];
						Transform feederPerch = feeder.transform.GetChild(
								Random.Range(0,feeder.transform.childCount));
						StartFlyingTo(feederPerch);
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
						else if(r<_ruffleChance+_preenChance+_fleeChance){
							//flee
							Transform perch = FindRandomPerch();
							StartFlyingTo(perch);
						}
					}
					_idleCheckTimer=_idleCheckTime;
				}
				break;
			case 1://flying
				transform.position+=transform.forward*Time.deltaTime*_flySpeed;
				//start landing when close
				if((transform.position-_flyTarget.position).sqrMagnitude<0.09f){
					bool retargeting=false;
					foreach(Bird b in _birds){
						if(b._flyTarget==_flyTarget){
							if(_rank>b._rank){
								Transform perch = FindRandomPerch();
								StartFlyingTo(perch);
								retargeting=true;
							}
						}
					}
					if(!retargeting){
						_state=2;
						_anim.SetBool("flying",false);
						_dir = _flyTarget.position-transform.position;
						_pos = transform.position;
						_prevRot = transform.rotation;
						_targetRot = _flyTarget.rotation;
					}
				}
				break;
			case 2://landing
				transform.position+=_dir.normalized*Time.deltaTime*_landSpeed;
				float tx = (transform.position.x-_pos.x)/_dir.x;
				float tz = (transform.position.z-_pos.z)/_dir.z;
				float ty = (transform.position.y-_pos.y)/_dir.y;
				float t = Mathf.Max(Mathf.Max(tx,tz),ty);
				transform.rotation = Quaternion.Slerp(_prevRot,_targetRot,t);
				if(t>=1f){
					transform.position=_flyTarget.position;
					if(_flyTarget.parent!=null){
						Vector3 looky=_flyTarget.parent.position;
						looky.y=_flyTarget.position.y;
						transform.LookAt(looky);
					}
					//sing on arrival
					Sing();
				}
				break;
			case 3://sing
				_singTimer-=Time.deltaTime;
				if(_singTimer<=0)
				{
					if(_flyTarget==null || _flyTarget.parent.tag=="Tree"){
						_state=0;
						if(_flyTarget!=null)
							_curTree=_flyTarget.parent.gameObject;
					}
					else if(_flyTarget.parent.tag=="Feeder")
					{
						_state=4;
						_curTree=null;
					}
				}
				break;
			case 4://feeding
				if(_peckTimer<=0){
					if(_peckCheckTimer<=0){
						if(Random.value<_peckChance){
							_anim.SetTrigger("peck");
							_peckTimer=1f;
							_energy++;
						}
						_peckCheckTimer=_peckCheckTime;
					}
					else
						_peckCheckTimer-=Time.deltaTime;
				}
				else
				{
					_peckTimer-=Time.deltaTime;
					if(_peckTimer<=0){
						if(_energy>=_idealEnergy)
						{
							Debug.Log("Bird done feeding at feeder");
							_state=0;
						}
					}
				}
				break;
		}
    }

	void StartFlyingTo(Transform t){
		if(_flyTarget==t && _state==0)
		{
			//dont fly if already idle at existing perch
			return;
		}
		_flyTarget=t;
		_anim.SetBool("flying",true);
		transform.LookAt(_flyTarget);
		_state = 1;
		_energy--;
		_audio.clip=_fleeSound;
		_audio.loop=true;
		_audio.Play();
	}

	void Sing(){
		_state=3;
		_singTimer=3f;
		_anim.SetTrigger("sing");
		_audio.clip=_song;
		_audio.loop=false;
		_audio.Play();
	}

	void HandleSound(Speaker.SpeakerEventArgs args){
		//only really want the closest bird responding
		//Debug.Log(name+" heard audio: "+args.name + " for duration "+args.dur);
		float minSqrDist=10000f;
		Bird close=null;
		foreach(Bird b in _birds)
		{
			if(args.name.ToLower().Contains(_species.ToLower()))
			{
				float sqd = (args.pos-transform.position).sqrMagnitude;
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
			Transform perch = FindRandomSafePerch(args.pos);
			StartFlyingTo(perch);
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

	Transform FindRandomPerch(){
		GameObject [] trees = GameObject.FindGameObjectsWithTag("Tree");
		GameObject tree = trees[Random.Range(0,trees.Length)];
		while(_curTree!=null && tree==_curTree)
			tree = trees[Random.Range(0,trees.Length)];
		//get perch
		Transform perch = tree.transform.GetChild(
				Random.Range(0,tree.transform.childCount));
		return perch;
	}

	Transform FindRandomSafePerch(Vector3 threat){
		if(_flyTarget!=null&&(threat-_flyTarget.position).sqrMagnitude>_fleeDistance*_fleeDistance)
			return _flyTarget;
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
		return perch;
	}

	IEnumerator SignalAlarmR(Footstep.FootstepEventArgs args){
		_audio.clip=_fleeAlarm;
		_audio.loop=false;
		_audio.Play();
		yield return new WaitForSeconds(_audio.clip.length);
		Transform perch = FindRandomSafePerch(args.pos);
		StartFlyingTo(perch);
		//alert other birds
		args.alerted=true;
		foreach(Bird b in _birds){
			if(b!=this){
				b.HandleFootstep(args);
			}
		}
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.blue;
		if(_flyTarget!=null)
			Gizmos.DrawSphere(_flyTarget.position,0.01f);
	}
}
