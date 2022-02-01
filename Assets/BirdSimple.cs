using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdSimple : MonoBehaviour
{
	int _state;

	[Header("Forage")]
	public float _peckChance;
	public float _moveChance;
	public float _stopChance;

	public float _updateTime;
	float _updateTimer;
	Animator _anim;
	public float _maxTurn;
	public float _degPerSec;
	Vector3 _size;
	public float _walkChance;
	public float _walkSpeed;
	Flock _flock;
	public float _flyAwayDur;
	public float _flySpeed;
	public AudioClip _flapSound;
	public float _flapPeriod;
	public float _flapVol;

	void Awake(){
		//randomly offset update timer
		_updateTimer=Random.value*_updateTime;
		_anim=GetComponent<Animator>();
		SkinnedMeshRenderer smr = transform.GetComponentInChildren<SkinnedMeshRenderer>();
		_size=smr.sharedMesh.bounds.size*smr.transform.localScale.x*transform.localScale.x;
		_flock=transform.GetComponentInParent<Flock>();
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		_updateTimer+=Time.deltaTime;
		if(_updateTimer>=_updateTime){
			switch(_state){
				case 0:
					float rand=Random.value;
					if(rand<_peckChance)
					{
						StopAllCoroutines();
						StartCoroutine(TurnAndPeck());
					}
					else if(rand<_peckChance+_walkChance){
						_state=1;
						StartCoroutine(Walk());
					}
					break;
				case 1:
					break;
				case 2:
					break;
			}
			_updateTimer=0;
		}
    }

	IEnumerator TurnAndPeck(){
		float timer=0;
		Quaternion start=transform.rotation;
		float degrees=Random.value*_maxTurn;
		transform.Rotate(Vector3.up*degrees*MRandom.RandSign());
		Quaternion end=transform.rotation;

		float dur=degrees/_degPerSec;
		while(timer<dur){
			timer+=Time.deltaTime;
			transform.rotation=Quaternion.Slerp(start,end,timer/dur);
			yield return null;
		}

		_anim.SetTrigger("peck");
		RaycastHit hit;
		if(Physics.Raycast(transform.position+Vector3.up*_size.y*0.5f, Vector3.down, out hit, _size.y,1)){
			Footstep f = hit.transform.GetComponent<Footstep>();
			if(f!=null)
				f.Sound(hit.point);
		}
	}

	IEnumerator Walk(){
		Vector3 pos = _flock.GetRandomPositionOnGround();
		Vector3 cur = transform.position;
		transform.LookAt(pos);
		Vector3 eulers=transform.eulerAngles;
		eulers.x=0;
		transform.eulerAngles=eulers;
		float dst = (cur-pos).magnitude;
		float dur = dst/_walkSpeed;
		float timer=0;
		_anim.SetFloat("walkSpeed",_walkSpeed*0.5f);
		while(timer<dur){
			timer+=Time.deltaTime;
			transform.position=Vector3.Lerp(cur,pos,timer/dur);
			yield return null;
		}
		_anim.SetFloat("walkSpeed",0);
		_state=0;
	}

	public void FlyAway(Vector3 dir){
		if(_state==2)
			return;
		_state=2;
		StopAllCoroutines();
		StartCoroutine(FlyDir(dir));
	}

	IEnumerator FlyDir(Vector3 dir){
		float timer=0;
		float delay=Random.value*_flapPeriod;
		yield return new WaitForSeconds(delay);
		_anim.SetTrigger("flyLoop");

		transform.forward=dir;

		Vector3 eulers=transform.eulerAngles;
		eulers.x=0;
		transform.eulerAngles=eulers;

		float flapTimer=0;

		while(timer<_flyAwayDur){
			timer+=Time.deltaTime;
			flapTimer+=Time.deltaTime;
			if(flapTimer>=_flapPeriod){
				Sfx.PlayOneShot3D(_flapSound,transform.position,Random.Range(0.8f,1.2f),_flapVol);
				flapTimer=0;
			}
			transform.position+=dir*Time.deltaTime*_flySpeed;
			yield return null;
		}
		gameObject.SetActive(false);
	}
}
