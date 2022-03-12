using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seed : MonoBehaviour
{
	public Transform _groundEffectsPrefab;
	Transform _groundEffects;
	public AudioClip _choir;
	Transform _player;
	public float _riseAmount;
	public float _riseTime;
	public float _riseDelay;
	public float _scaleDown;
	Rigidbody _rb;

	void Awake(){
		_player=GameManager._player.transform;
		//StartEffects();
	}
    // Start is called before the first frame update
    void Start()
    {
		_rb=GetComponent<Rigidbody>();
		_rb.freezeRotation=true;
        
    }

    // Update is called once per frame
    void Update()
    {

    }

	void StartEffects(){

		RaycastHit hit;
		if(Physics.Raycast(transform.position,Vector3.down, out hit, 1f, 1)){
			_groundEffects = Instantiate(_groundEffectsPrefab,hit.point,Quaternion.identity);
		}
		else
			_groundEffects = Instantiate(_groundEffectsPrefab,transform.position,Quaternion.identity);
	}

	void OnTriggerEnter(Collider other){
		if(other.GetComponent<Bird>()==null)
			return;
		//activate next puzzle
		//PuzzleBox puzzle = transform.GetComponentInParent<PuzzleBox>();
		//puzzle.ActivateNextPuzzle();

		//collect seed
		CollectSeed(other.GetComponent<Bird>());
		GetComponent<Rotator>().enabled=false;

	}

	public void CollectSeed(Bird other){
		if(_groundEffects!=null)
			Destroy(_groundEffects.gameObject);
		else
			Debug.Log("bug alert - effects not started");
		//play some sound
		Sfx.PlayOneShot2D(_choir);
		other.CollectSeed(transform);
		GetComponent<Collider>().enabled=false;
		StartCoroutine(CollectSeedR(other.transform));
	}

	IEnumerator CollectSeedR(Transform bird){
		transform.SetParent(bird);
		transform.localScale*=_scaleDown;
		Vector3 curLocal=transform.localPosition;
		curLocal.x=0;
		curLocal.z=0;
		transform.localPosition=curLocal;
		Vector3 startLocal=transform.localPosition;
		Vector3 endLocal=startLocal+_riseAmount*Vector3.up;
		float timer=0;
		while(timer<_riseTime){
			timer+=Time.deltaTime;
			transform.localPosition=Vector3.Lerp(startLocal,endLocal,timer/_riseTime);
			yield return null;
		}
		yield return new WaitForSeconds(_riseDelay);
		Destroy(gameObject);
	}
}
