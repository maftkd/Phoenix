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
		_bird._onDoneFlying+=BirdArrived;
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
		//StartCoroutine(GoToNextSeed());
		Transform seed = FindRandomSeed();
		FlyToSeed(seed);
	}

	void FlyToSeed(Transform seed){
		_bird.FlyTo(seed.position);
	}

	IEnumerator GoToNextSeed(){
		Transform seed = FindRandomSeed();
		if(seed!=null)
		{
			_waddleTarget=seed.position+Vector3.down*0.07f;
			Vector3 diff=seed.position-transform.position;
			_waddleTarget+=diff.normalized*0.05f;
			_bird.WaddleTo(_waddleTarget,_waddleSpeed);
			while(!_bird.ArrivedW())
			{
				yield return null;
			}
			StartCoroutine(GoToNextSeed());
		}
		else
		{
			yield return null;
			enabled=false;
			//_bird.GrandExit();
			Debug.Log("Done snacking");
			_bird.FlyToNextPuzzle();

		}
	}

	Transform FindRandomSeed(){
		int seeds = Physics.OverlapSphereNonAlloc(_bird._mate.transform.position,2f,_cols,_foodMask);
		if(seeds>0)
			return _cols[Random.Range(0,seeds)].transform;
		else
			return null;
	}

	void BirdArrived(){
		Debug.Log("Arrived");
		if(!enabled)
			return;
		Transform seed = FindRandomSeed();
		if(seed!=null)
		{
			Debug.Log("going to next seed");
			StartCoroutine(GoToNextSeed());
		}
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.red;
		Gizmos.DrawWireSphere(_waddleTarget,0.2f);
	}

}
