﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cardinal : MonoBehaviour
{
	public AudioClip [] _calls;
	public Transform _callBubble;
	public float _maxScale;
	public float _scaleSpeed;
	Animator _anim;
	public float _flySpeed;
	public AudioClip _flapSound;
	public float _flapDur;
	
	void Awake(){
		_anim=GetComponent<Animator>();
	}

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void Call(){
		StartCoroutine(CallR());
	}

	IEnumerator CallR(){
		_anim.SetTrigger("sing");
		Sfx.PlayOneShot3D(_calls[Random.Range(0,_calls.Length)],transform.position,Random.Range(0.9f,1.1f));
		Transform call = Instantiate(_callBubble,transform.position,Quaternion.identity);
		float timer=0;
		float dur=_maxScale/_scaleSpeed;
		while(timer<dur){
			timer+=Time.deltaTime;
			float frac=timer/dur;
			call.localScale=Vector3.one*frac*_maxScale;
			yield return null;
		}
		Destroy(call.gameObject);
	}
	
	public void FlyToPuzzle(PuzzleBox pb){
		/*
		Transform target=pb.transform.Find("Cage");
		StartCoroutine(FlyTo(target.position+Vector3.up*0.3f));
		*/
	}

	public void FlyAway(){
		StartCoroutine(FlyAwayR());
	}

	IEnumerator FlyAwayR(){
		_anim.SetTrigger("flyLoop");
		Vector3 start=transform.position;
		Vector3 target=start+transform.forward*3f+transform.up*3f;
		transform.LookAt(target);
		Vector3 eulerAngles=transform.eulerAngles;
		eulerAngles.x=0;
		transform.eulerAngles=eulerAngles;

		float timer=0f;
		float dist=(target-start).magnitude;
		float dur=dist/_flySpeed;
		float flapTimer=10f;

		while(timer<dur){
			timer+=Time.deltaTime;
			float frac=timer/dur;
			transform.position=Vector3.Lerp(start,target,frac);
			flapTimer+=Time.deltaTime;
			if(flapTimer>_flapDur){
				Sfx.PlayOneShot3D(_flapSound,transform.position,Random.Range(0.7f,1.3f));
				flapTimer=0f;
			}
			yield return null;
		}
		Destroy(gameObject);
	}

	/*
	IEnumerator FlyTo(Vector3 p){
		_anim.SetTrigger("flyLoop");
		Vector3 start=transform.position;
		Vector3 target=p;
		transform.LookAt(target);
		Vector3 eulerAngles=transform.eulerAngles;
		eulerAngles.x=0;
		transform.eulerAngles=eulerAngles;

		float timer=0f;
		float dist=(target-start).magnitude;
		float dur=dist/_flySpeed;

		while(timer<dur){
			timer+=Time.deltaTime;
			float frac=timer/dur;
			transform.position=Vector3.Lerp(start,target,frac);
			yield return null;
		}
		transform.position=target;
		_anim.SetTrigger("land");

		transform.LookAt(GameManager._player.transform);
		eulerAngles=transform.eulerAngles;
		eulerAngles.x=0;
		transform.eulerAngles=eulerAngles;

		Call();
	}
	*/
}
