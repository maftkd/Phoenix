using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour
{

	public Transform _birdPrefab;
	[Tooltip("Should be 2, one male, one female")]
	public Material [] _mats;
	public int _numBirds;
	public Transform [] _trees;
	Transform [] _birds;
	Dictionary<int,int> _birdStates;

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

	void Awake(){
		_birds = new Transform[_numBirds];
		_birdStates = new Dictionary<int,int>();
		for(int i=0; i<_numBirds;i++){
			Transform bird = Instantiate(_birdPrefab);
			bird.position=GetRandomTreePerch().position;
			bird.localEulerAngles = Vector3.up*Random.value*360f;
			SkinnedMeshRenderer smr = bird.GetComponentInChildren<SkinnedMeshRenderer>();
			smr.material = _mats[i%2];
			_birds[i]=bird;
			_birdStates.Add(i,0);
		}
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
					//birds have chance to fly
					for(int i=0; i<_numBirds; i++){
						if(_birdStates[i]==0&&Random.value<_flyChance){
							StartCoroutine(FlyToRandomPerch(i));
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

	IEnumerator FlyToRandomPerch(int index){
		_birdStates[index]=1;
		yield return null;
		Transform bird = _birds[index];
		Animator anim = bird.GetComponent<Animator>();
		Transform target = GetRandomTreePerch();
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
		anim.SetTrigger("land");
		_birdStates[index]=0;
	}
}
