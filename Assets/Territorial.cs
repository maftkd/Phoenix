using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Territorial : MonoBehaviour
{
	public Transform [] _trees;
	int _curTree;
	public Vector2 _delayRange;
	float _delay;
	float _delayTimer;
	int _state;
	public float _speed;
	public AudioClip _bushHit;

	void Awake(){
		_curTree=Random.Range(0,_trees.Length);
		transform.position=_trees[_curTree].position;
		_delay=Random.Range(_delayRange.x,_delayRange.y);
	}

    // Update is called once per frame
    void Update()
    {
		switch(_state){
			case 0:
				_delayTimer+=Time.deltaTime;
				if(_delayTimer>=_delay){
					_delay=Random.Range(_delayRange.x,_delayRange.y);
					_delayTimer=0f;
					StartCoroutine(FlyToAnotherTree());
				}
				break;
			case 1:
				break;
		}
    }

	IEnumerator FlyToAnotherTree(){
		_state=1;
		int nextTree=_curTree;
		int iters=0;
		while(nextTree==_curTree&&iters<1000){
			nextTree=Random.Range(0,_trees.Length);
			iters++;
		}
		Transform next = _trees[nextTree];
		Vector3 startPos=transform.position;
		Vector3 endPos=next.position;
		transform.LookAt(endPos);
		float dist=(endPos-startPos).magnitude;
		float dur=dist/_speed;
		float timer=0;
		while(timer<dur){
			timer+=Time.deltaTime;
			float frac=timer/dur;
			transform.position=Vector3.Lerp(startPos,endPos,frac);
			yield return null;
		}
		Sfx.PlayOneShot3D(_bushHit,transform.position,Random.Range(0.9f,1.1f));
		transform.position=endPos;
		_curTree=nextTree;
		_state=0;
	}
}
