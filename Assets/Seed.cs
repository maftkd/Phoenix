using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Seed : MonoBehaviour
{
	public AudioClip _choir;
	public AudioClip _chalp;
	Transform _player;
	Bird _bird;
	public float _riseAmount;
	public float _riseTime;
	public float _riseDelay;
	public float _scaleDown;
	public float _minVel;
	public float _minEffectTime;
	float _timer;
	public UnityEvent _onEat;
	GameObject _sphere;

	void Awake(){
		_bird=GameManager._player;
		_player=_bird.transform;
		_sphere=transform.GetChild(0).gameObject;
		_sphere.SetActive(false);
	}
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
		_sphere.transform.rotation=Quaternion.identity;
    }

	void OnTriggerEnter(Collider other){
		/*
		if(other.GetComponent<Bird>()==null)
			return;
		CollectSeed(other.GetComponent<Bird>());
		GetComponent<Rotator>().enabled=false;
		_onEat.Invoke();
		*/
		_sphere.SetActive(true);
		_bird.NearSeed(this);
	}

	void OnTriggerExit(Collider other){
		_sphere.SetActive(false);
		_bird.NearSeed(null);
	}

	public void GetHeld(){
		GetComponent<Rotator>().enabled=false;
		_sphere.SetActive(false);
		Sfx.PlayOneShot2D(_chalp);
	}

	/*
	public void CollectSeed(Bird other){
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
	*/
}
