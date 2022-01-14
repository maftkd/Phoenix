using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seed : MonoBehaviour
{
	public Transform _groundEffectsPrefab;
	Transform _groundEffects;
	public AudioClip _choir;
	Transform _player;
	bool _effectsPlayed;
	Rigidbody _rb;
	float _lifeTimer=0;
	public float _riseAmount;
	public float _riseTime;
	public float _riseDelay;
	public float _scaleDown;

	void Awake(){
		_player=GameObject.FindGameObjectWithTag("Player").transform;
		_rb = GetComponent<Rigidbody>();
	}
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if(!_effectsPlayed){
			_lifeTimer+=Time.deltaTime;
			if(_rb.velocity.sqrMagnitude<0.1f&&_lifeTimer>0.25f)
				StartEffects();
		}
    }

	void StartEffects(){

		_effectsPlayed=true;
		RaycastHit hit;
		if(Physics.Raycast(transform.position,Vector3.down, out hit, 1f, 1)){
			_groundEffects = Instantiate(_groundEffectsPrefab,hit.point,Quaternion.identity);
		}
		else
			_groundEffects = Instantiate(_groundEffectsPrefab,transform.position,Quaternion.identity);
	}

	public void CollectSeed(){
		_effectsPlayed=true;//prevents effects from starting after collection
		if(_groundEffects!=null)
			Destroy(_groundEffects.gameObject);
		else
			Debug.Log("bug alert - effects not started");
		//play some sound
		Sfx.PlayOneShot2D(_choir);
		_player.GetComponent<Bird>().CollectSeed(transform);
		GetComponent<Collider>().enabled=false;
		_rb.isKinematic=true;
		_rb.useGravity=false;
		StartCoroutine(CollectSeedR());
	}

	IEnumerator CollectSeedR(){
		transform.SetParent(_player);
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
