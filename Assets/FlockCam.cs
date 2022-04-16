using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockCam : Shot
{
	Flock _flock;
	public float _targetOffset;
	public float _posLerp;
	float _theta;
	float _phi;

	protected override void Awake(){
		base.Awake();
		_flock=transform.GetComponentInParent<Flock>();
	}
	
    // Start is called before the first frame update
    protected override void Start()
    {

    }

	void OnEnable(){
		transform.LookAt(_flock._center);
		Vector3 offset=-transform.forward;
		offset.y=0;
		offset.Normalize();
		Vector3 targetPos=_flock._center+offset*_targetOffset;
		_phi=0;
		_theta=Mathf.Atan2(offset.z,offset.x);

	}

	protected override void Update(){
		base.Update();

		Vector2 mouseMotion = _mIn.GetMouseMotion();
		_theta-=mouseMotion.x;
		_phi+=mouseMotion.y;
		_phi=Mathf.Clamp(_phi,-1f,1f);

		float y = Mathf.Sin(_phi);
		float xzRad = Mathf.Cos(_phi);
		float x = xzRad*Mathf.Cos(_theta);
		float z = xzRad*Mathf.Sin(_theta);
		Vector3 offset = new Vector3(x,y,z)*_targetOffset;
		
		Vector3 targetPos=_flock._center+offset;

		//Vector3 targetPos=_flock._center+offset*_targetOffset;
		transform.position=Vector3.Lerp(transform.position,targetPos,_posLerp*Time.deltaTime);
		transform.LookAt(_flock._center);
	}
}
