using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hunger : MonoBehaviour
{
	Bird _bird;
	int _state;
	Collider [] _cols;
	public LayerMask _foodMask;
	public float _waddleSpeed;
	Vector3 _waddleTarget;

	void Awake(){
		_bird=GetComponent<Bird>();
		_cols = new Collider[10];
	}

	void OnEnable(){
		Debug.Log("Time to nom!");
		/*
		//teleport to first seed
		int seeds = Physics.OverlapSphereNonAlloc(_bird._mate.transform.position,1f,_cols,_foodMask);
		Debug.Log("Found "+seeds+" seeds");
		int startSeed=Random.Range(0,seeds);
		Transform seed = _cols[startSeed].transform;
		transform.position=seed.position+Vector3.down*0.07f;
		*/
		StartCoroutine(GoToNextSeed());
	}

	IEnumerator GoToNextSeed(){
		int seeds = Physics.OverlapSphereNonAlloc(_bird._mate.transform.position,2f,_cols,_foodMask);
		Debug.Log("Found "+seeds+" seeds");
		int startSeed=Random.Range(0,seeds);
		_waddleTarget=_cols[startSeed].transform.position+Vector3.down*0.07f;
		_bird.WaddleTo(_waddleTarget,_waddleSpeed);
		while(!_bird.ArrivedW())
			yield return null;
		seeds = Physics.OverlapSphereNonAlloc(_bird._mate.transform.position,2f,_cols,_foodMask);
		if(seeds>0)
			StartCoroutine(GoToNextSeed());
		else
		{
			enabled=false;
			//_bird.GrandExit();
			Debug.Log("Done snacking");
			_bird.FlyToNextPuzzle();
		}
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.red;
		Gizmos.DrawWireSphere(_waddleTarget,0.2f);
	}

}
