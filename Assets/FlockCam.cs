using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockCam : Shot
{
	Flock _flock;
	public float _targetOffset;
	public float _posLerp;
	protected override void Awake(){
		base.Awake();
		_flock=transform.GetComponentInParent<Flock>();
	}
	
    // Start is called before the first frame update
    protected override void Start()
    {

    }

	void OnEnable(){

	}

	protected override void Update(){
		base.Update();
		transform.LookAt(_flock._center);
		Vector3 offset=-transform.forward;
		offset.y=0;
		offset.Normalize();
		Vector3 targetPos=_flock._center+offset*_targetOffset;
		transform.position=Vector3.Lerp(transform.position,targetPos,_posLerp*Time.deltaTime);
	}
}
