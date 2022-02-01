using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour
{

	public Transform _birdPrefab;
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
		FLY_OVER=2,
		FORAGE=3
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
	public Terrain _terrain;

	void Awake(){
		_birds = new Transform[_numBirds];
		_birdStates = new Dictionary<int,int>();
		for(int i=0; i<_numBirds;i++){
			Transform bird = Instantiate(_birdPrefab,transform);
			switch(_flockMode){
				case FlockMode.IN_TREE:
					bird.position=GetRandomTreePerch().position;
					break;
				case FlockMode.FORAGE:
					bird.position=GetRandomPositionOnGround();
					break;
				default:
					break;

			}
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
		_player=GameManager._player;
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
				break;
			case FlockMode.FLY_AWAY:
				break;
			case FlockMode.FLY_OVER:
				break;
			case FlockMode.FORAGE:
				if(_player.IsPlayerInRange(_centerTransform,_outerRadius)){
					foreach(Transform t in _birds)
					{
						Vector3 flyDir=new Vector3(Random.value,0.5f,Random.value);
						t.GetComponent<BirdSimple>().FlyAway(flyDir);
					}
				}
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

	public Vector3 GetRandomPositionOnGround(){
		Vector2 v = Random.insideUnitCircle*_innerRadius;
		Vector3 pos = transform.position+Vector3.right*v.x+Vector3.forward*v.y;
		pos.y=_terrain.SampleHeight(pos);
		return pos;
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
