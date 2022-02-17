using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dolly : MonoBehaviour
{
	float _offsetTimer;
	Vector3 _localPos;
	public float _offsetFrequency;
	public float _offsetAmp;
	void Awake(){
		enabled=false;
	}

	void OnValidate(){
	}

	public void StartTracking(Transform t){
		transform.SetParent(t);
		_localPos=transform.localPosition;
		enabled=true;
	}

	void Update(){
		_offsetTimer+=Time.deltaTime;
		transform.localPosition=_localPos+Vector3.forward*Mathf.Sin(_offsetTimer*Mathf.PI*2*_offsetFrequency)*_offsetAmp;
	}

	void OnDrawGizmos(){
	}
}
