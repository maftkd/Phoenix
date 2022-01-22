using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour
{

	public Transform _birdPrefab;
	public Transform _matePrefab;
	public Material [] _mats;
	public int _numBirds;
	public Transform [] _trees;
	Transform [] _birds;
	Dictionary<int,int> _birdStates;
	Transform _centerTransform;
	Vector3 _center;
	public AudioClip [] _calls;
	Bird _player;
	float _playerDist;

	public enum FlockMode {
		IN_TREE=0,
		FLY_AWAY=1,
		FLY_OVER=2
	}
	public FlockMode _flockMode;
	float _updateTimer;
	public float _updateTime;
	public float _flyChance;
	public float _flySpeed;
	public float _callChance;
	public float _callVol;

	public float _innerRadius;
	public float _outerRadius;
	Bird _mate;

	void Awake(){
		_birds = new Transform[_numBirds];
		_birdStates = new Dictionary<int,int>();
		for(int i=0; i<_numBirds;i++){
			Transform bird = Instantiate(_birdPrefab);
			bird.position=GetRandomTreePerch().position;
			bird.localEulerAngles = Vector3.up*Random.value*360f;
			SkinnedMeshRenderer smr = bird.GetComponentInChildren<SkinnedMeshRenderer>();
			if(_mats.Length>1)
				smr.material = _mats[i%2];
			else
				smr.material = _mats[0];

			_birds[i]=bird;
			_birdStates.Add(i,0);
		}
		_centerTransform=transform.GetChild(0);
		_center=_centerTransform.position;
		_centerTransform.gameObject.SetActive(false);
		_player=GameObject.FindGameObjectWithTag("Player").GetComponent<Bird>();
		_player._onCall+=PlayerCalled;
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		switch(_flockMode){
			case FlockMode.IN_TREE:
				_updateTimer+=Time.deltaTime;
				if(_updateTimer>=_updateTime){
					_playerDist=(_center-_player.transform.position).magnitude;
					if(_playerDist<_outerRadius){
						//birds have chance to fly
						for(int i=0; i<_numBirds; i++){
							if(_birdStates[i]==0&&Random.value<_flyChance){
								Transform target = GetRandomTreePerch();
								if((target.position-_birds[i].position).sqrMagnitude>0.01f)
									StartCoroutine(FlyToRandomPerch(i,target));
							}
						}
					}
					_updateTimer=0;
				}
				break;
			case FlockMode.FLY_AWAY:
				break;
			case FlockMode.FLY_OVER:
				break;
			default:
				break;
		}
    }

	Transform GetRandomTreePerch(){
		Transform tree = _trees[Random.Range(0,_trees.Length)];
		return tree.GetChild(Random.Range(0,tree.childCount));
	}

	IEnumerator FlyToRandomPerch(int index,Transform target){
		_birdStates[index]=1;
		yield return null;
		Transform bird = _birds[index];
		Animator anim = bird.GetComponent<Animator>();
		Vector3 start=bird.position;
		Vector3 end=target.position;
		bird.LookAt(end);
		float timer=0;
		float dist = (end-start).magnitude;
		float dur=dist/_flySpeed;
		if(dur>0)
			anim.SetTrigger("flyLoop");
		while(timer<dur){
			timer+=Time.deltaTime;
			bird.position=Vector3.Lerp(start,end,timer/dur);
			yield return null;
		}
		_birdStates[index]=0;
		if(Random.value<_callChance){
			anim.SetTrigger("sing");
			Sfx.PlayOneShot3DVol(_calls[Random.Range(0,_calls.Length)],end,Mathf.InverseLerp(_outerRadius,_innerRadius,_playerDist)*_callVol);
		}
		else
			anim.SetTrigger("land");
	}

	public void PlayerCalled(){
		/*
		if(_mate!=null){
			Debug.Log("Mate comes to player");
			//_mate.ComeToPlayer();
		}
		else if(_player._mate==null){//check that player does not have a mate from any other flock
			if((_player.transform.position-_center).sqrMagnitude<=_innerRadius){
				Transform perch = GetRandomTreePerch();
				Transform m = Instantiate(_matePrefab,perch.position,Quaternion.identity);
				_mate=m.GetComponent<Bird>();
				_mate._enterGrandly=true;
			}
		}
		*/
		//if player is within range
		//	if mate is not already deployed
		//		deploy mate from random perch
		//		have mate make grand entrance
	}

	void OnDrawGizmos(){
		if(_centerTransform==null)
		{
			_centerTransform=transform.GetChild(0);
		}
		if(_centerTransform!=null){
			Gizmos.color=Color.green;
			Gizmos.DrawWireSphere(_centerTransform.position,_innerRadius);
			Gizmos.color=Color.red;
			Gizmos.DrawWireSphere(_centerTransform.position,_outerRadius);
		}
	}
}
