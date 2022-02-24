using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyCam : Shot
{
	Vector3 _position;
	Quaternion _rotation;
	Vector3 _forward;
	Bird _player;
	public float _targetPhi;
	public float _targetR;
	public float _lerp;
	public float _slerp;
	public float _rLerp;
	public int _flightPriority;
	int _defaultPriority;
	Vector3 _offset;

	protected override void Awake(){
		base.Awake();
		_player=GameManager._player;
		_defaultPriority=_priority;
	}

	void OnEnable(){
		_position=transform.position;
		_rotation=transform.rotation;
		_forward=transform.forward;
	}

	public override void StartTracking(Transform t){
		base.StartTracking(t);
		SetPriority();
	}

	public override void StopTracking(){
		base.StopTracking();
		ResetPriority();
	}

	public void SetPriority(){
		_priority=_flightPriority;
	}

	public void ResetPriority(){
		_priority=_defaultPriority;
	}

    // Start is called before the first frame update
    protected override void Start()
    {
		base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
		base.Update();

		transform.position=_position;
		transform.rotation=_rotation;

		//calc phi
		Vector3 diff=transform.position-_player.transform.position;
		float r = diff.magnitude;
		float phi=Mathf.Asin(diff.y/diff.magnitude);
		phi=Mathf.Lerp(phi,_targetPhi,_lerp*Time.deltaTime);
		DebugScreen.Print(phi,0);
		r=Mathf.Lerp(r,_targetR,_rLerp*Time.deltaTime);
		DebugScreen.Print(r,1);

		//calc theta
		Vector3 camBack=-transform.forward;
		camBack.y=0;
		camBack.Normalize();
		float theta=Mathf.Atan2(camBack.z,camBack.x);
		/*
		if(theta>Mathf.PI)
			theta=-(Mathf.PI*2f-theta);
		else if(theta<-Mathf.PI)
			theta=(Mathf.PI*2f+theta);
			*/
		Vector3 playerBack=-_player.transform.forward;
		playerBack.y=0;
		playerBack.Normalize();
		float targetTheta=Mathf.Atan2(playerBack.z,playerBack.x);

		if(Mathf.Abs(theta-targetTheta)>Mathf.PI)
		{
			if(theta<targetTheta)
				targetTheta=-(Mathf.PI*2f-targetTheta);
			else
				targetTheta+=Mathf.PI*2f;
		}

		theta=Mathf.Lerp(theta,targetTheta,_lerp*Time.deltaTime);
		DebugScreen.Print(theta,2);
		DebugScreen.Print(targetTheta,3);

		float y = Mathf.Sin(phi);
		float xzRad = Mathf.Cos(phi);
		float x = xzRad*Mathf.Cos(theta);
		float z = xzRad*Mathf.Sin(theta);
		_offset = new Vector3(x,y,z)*r;
		
		Vector3 targetPos=_player.transform.position+_offset;

		_position=targetPos;

		transform.forward=-_offset;
		Quaternion targetRot=transform.rotation;
		_rotation=Quaternion.Slerp(_rotation,targetRot,_slerp*Time.deltaTime);
		//_position=Vector3.Lerp(_position,targetPos,_lerp*Time.deltaTime);
		/*
		transform.forward=-offset;
		Quaternion targetRot=transform.rotation;

		_rotation=Quaternion.Slerp(_rotation,targetRot,_lerp*Time.deltaTime);
		*/

		transform.position=_position;
		transform.rotation=_rotation;


		//old code
		//warmup
		/*
		if(_warmUp<1f){
			_warmUp+=Time.deltaTime*_warmUpSpeed;
			if(_warmUp>1f)
				_warmUp=1f;
		}
		*/
		//determine lerp speed
		/*
		Vector3 viewPoint = _cam.WorldToViewportPoint(_player.transform.position+_player._size.y*Vector3.up);
		float lerp = Mathf.Lerp(_minLerp,_maxLerp,Mathf.Abs(viewPoint.y-0.5f)*2f)*_warmUp;
		float slerp = Mathf.Lerp(_minLerp,_maxLerp,Mathf.Abs(viewPoint.x-0.5f)*2f)*_warmUp;
		*/

		//calc targetpos
		/*
		Vector3 flatRight=_player.transform.right;
		flatRight.y=0;
		flatRight.Normalize();
		Vector3 targetPos=_player.transform.position 
			- _player.transform.forward*_trackOffset.z
			+ Vector3.up*_trackOffset.y
			+ flatRight*_trackOffset.x;
			*/

		//calc targetRot
		/*
		transform.position=_position;
		Quaternion curRot=_rotation;
		transform.LookAt(_player.transform);
		Quaternion targetRot=transform.rotation;
		*/

		//lerp transform
		/*
		_position=Vector3.Lerp(_position,targetPos,lerp*Time.deltaTime);
		_rotation=Quaternion.Slerp(_rotation,targetRot,slerp*Time.deltaTime);
		*/

		/*
		transform.position=_position;
		transform.rotation=_rotation;
		*/

		//cancel out roll
		/*
		Vector3 eulers=transform.eulerAngles;
		eulers.z=0;
		transform.eulerAngles=eulers;
		_rotation=transform.rotation;
		*/

    }

	void OnDrawGizmos(){
		Gizmos.color=Color.blue;
		Gizmos.DrawLine(transform.position,transform.position+_offset);
	}
}
