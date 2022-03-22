using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseSparrow : MonoBehaviour
{

	public float _flyInDist;
	public float _flySpeed;
	Animator _anim;

	void Awake(){
		_anim=GetComponent<Animator>();
	}

    // Start is called before the first frame update
    void Start()
    {
		StartCoroutine(ArriveAtHouse());
    }

	IEnumerator ArriveAtHouse(){
		Vector3 endPos=transform.position;
		Vector3 dir=transform.forward-Vector3.up;
		dir.Normalize();

		Vector3 startPos=endPos-dir*_flyInDist;
		float dist = _flyInDist;
		float travelDist=0;
		float timer=0;
		float speed=0;
		float flapTimer=0;

		transform.position=startPos;
		_anim.SetTrigger("flyLoop");
		while(travelDist<dist){
			timer+=Time.deltaTime;
			/*
			flapTimer+=Time.deltaTime;
			if(flapTimer>_flapDur){
				flapTimer=0;
			}
			*/
			//speed = Mathf.Lerp(_flySpeed,_maxSpeed,timer);
			speed=_flySpeed;
			Vector3 v = dir*Time.deltaTime*speed;
			travelDist+=v.magnitude;
			transform.position+=v;
			yield return null;
		}
		transform.position=endPos;
		_anim.SetTrigger("land");
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
