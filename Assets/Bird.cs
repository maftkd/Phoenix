using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour
{
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
	public float _startleRange;
	public float _startleSpeed;
	public float _fleeDistance;
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
							GameObject [] trees = GameObject.FindGameObjectsWithTag("Tree");
							GameObject tree = trees[Random.Range(0,trees.Length)];
							while(_curTree!=null && tree==_curTree)
								tree = trees[Random.Range(0,trees.Length)];
							//get perch
							Transform perch = tree.transform.GetChild(
									Random.Range(0,tree.transform.childCount));
							StartFlyingTo(perch);
						}
					}
					_idleCheckTimer=_idleCheckTime;
				}
				//if energy <= minEnergy
				//	//go to food
				//else
				//	hang out
				break;
			case 1://flying
				transform.position+=transform.forward*Time.deltaTime*_flySpeed;
				//if distance to target <.3
				//	landing
				if((transform.position-_flyTarget.position).sqrMagnitude<0.09f){
					_state=2;
					_anim.SetBool("flying",false);
					_dir = _flyTarget.position-transform.position;
					_pos = transform.position;
					_prevRot = transform.rotation;
					_targetRot = _flyTarget.rotation;
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
		_flyTarget=t;
		_anim.SetBool("flying",true);
		transform.LookAt(_flyTarget);
		_state = 1;
		_energy--;
		_audio.clip=_fleeSound;
		_audio.Play();
	}

	void Sing(){
		_state=3;
		_singTimer=3f;
		_anim.SetTrigger("sing");
		_audio.clip=_song;
		_audio.Play();
	}

	void HandleSound(Speaker.SpeakerEventArgs args){
		//Debug.Log(name+" heard audio: "+args.name + " for duration "+args.dur);
		if(args.name.ToLower().Contains(name.ToLower()))
			StartCoroutine(SingAfterSecondsR(args.dur));
	}

	IEnumerator SingAfterSecondsR(float s){
		yield return new WaitForSeconds(s);
		Sing();
	}

	void HandleFootstep(Footstep.FootstepEventArgs args){
		if(_state==1)
			return;
		if((args.pos-transform.position).sqrMagnitude<=_startleRange*_startleRange){
			if(args.speed>=_startleSpeed)
			{
				Debug.Log("Startled!");
				GameObject [] trees = GameObject.FindGameObjectsWithTag("Tree");
				GameObject tree = trees[Random.Range(0,trees.Length)];
				int iters=0;
				while((tree.transform.position-args.pos).sqrMagnitude<_fleeDistance*_fleeDistance&&
						iters<10){
					tree = trees[Random.Range(0,trees.Length)];
					iters++;
				}
				//get perch
				Transform perch = tree.transform.GetChild(
						Random.Range(0,tree.transform.childCount));
				StartFlyingTo(perch);
			}
		}
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.blue;
		if(_flyTarget!=null)
			Gizmos.DrawSphere(_flyTarget.position,0.01f);
	}
}
