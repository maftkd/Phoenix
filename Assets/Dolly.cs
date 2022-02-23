using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dolly : Shot
{
	float _offsetTimer;
	Vector3 _localPos;
	public float _offsetFrequency;
	public float _offsetAmp;

	protected override void Awake(){
		base.Awake();
		enabled=false;
	}

	public override void StartTracking(Transform t){
		base.StartTracking(t);
		transform.SetParent(t);
		_localPos=transform.localPosition;
		enabled=true;
	}

	protected override void Update(){
		base.Update();
		_offsetTimer+=Time.deltaTime;
		transform.localPosition=_localPos+Vector3.forward*Mathf.Sin(_offsetTimer*Mathf.PI*2*_offsetFrequency)*_offsetAmp;
	}
}
