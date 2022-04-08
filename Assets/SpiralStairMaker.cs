using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiralStairMaker : MonoBehaviour
{
	public float _rise;
	public float _theta;
	public Transform _stair;
	public int _numStairs;


	public bool _autoUpdate;

	void OnValidate(){
		if(_autoUpdate){
			UpdateStairs();
		}
	}

    void UpdateStairs()
    {
		StartCoroutine(Clear());
		for(int i=0; i<_numStairs; i++){
			Transform stair = Instantiate(_stair,transform);
			stair.localPosition=Vector3.up*_rise*i;
			stair.localEulerAngles=Vector3.up*_theta*i;
		}
    }

	IEnumerator Clear(){
		int numChildren=transform.childCount;
		Transform [] children = new Transform[numChildren];
		for(int i=0;i<numChildren; i++){
			children[i]=transform.GetChild(i);
		}

		yield return null;

		for(int i=0; i<numChildren; i++){
			DestroyImmediate(children[i].gameObject);
		}
	}
}
