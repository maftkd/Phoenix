using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyCam : Shot
{
	Vector3 _position;
	Quaternion _rotation;
	Vector3 _forward;
	Fly _fly;
	public Vector2 _phiRange;
	float _targetPhi;
	public float _targetPhiOffset;
	public float _targetR;
	public float _lerp;
	public float _phiLerp;
	public float _slerp;
	public float _rLerp;
	public int _flightPriority;
	int _defaultPriority;
	Vector3 _offset;
	float _warmUp;
	public float _warmUpMult;
	public float _mouseSens;
	float _phiOffset;
	float _thetaOffset;
	float _theta;
	float _phi;

	protected override void Awake(){
		base.Awake();
		_player=GameManager._player;
		_fly=_player.GetComponent<Fly>();
		_defaultPriority=_priority;
	}

	void OnEnable(){
		_position=transform.position;
		_rotation=transform.rotation;
		_forward=transform.forward;
		_warmUp=0;
		Vector3 camBack=-transform.forward;
		camBack.y=0;
		camBack.Normalize();
		_theta=Mathf.Atan2(camBack.z,camBack.x);

		Vector3 diff=transform.position-_player.transform.position;
		_phi=Mathf.Asin(diff.y/diff.magnitude);
	}

	public override void StartTracking(Transform t){
		base.StartTracking(t);
		//SetPriority();
	}

	public override void StopTracking(){
		base.StopTracking();
		//ResetPriority();
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

		//warmup
		if(_warmUp<1f){
			_warmUp+=Time.deltaTime*_warmUpMult;
			if(_warmUp>1f)
				_warmUp=1f;
		}

		Vector2 mouseMotion = _mIn.GetMouseMotion();

		//calc phi
		Vector3 playerBack=-_player.transform.forward;
		Vector3 vel = _player.GetVelocity().normalized;
		/*
		float velMag=vel.magnitude;
		if(Mathf.Abs(velMag)>0.000001){
			vel/=velMag;
			float velFrac=velMag/_fly._maxVel;
			*/

		float targetPhi=0;
			if(vel.sqrMagnitude>=0.000001)
				targetPhi=Mathf.Asin(-vel.y);

			targetPhi+=_targetPhiOffset;
			_phi=Mathf.Lerp(_phi,targetPhi,_phiLerp*Time.deltaTime*_warmUp);
			_phi=Mathf.Clamp(_phi,_phiRange.x,_phiRange.y);
		//}
		//_phi-=mouseMotion.y*_mouseSens;

		//calc r
		Vector3 diff=transform.position-_player.transform.position;
		float r = diff.magnitude;
		r=Mathf.Lerp(r,_targetR,_rLerp*Time.deltaTime*_warmUp);

		//calc theta
		//_theta-=mouseMotion.x*_mouseSens;
		playerBack.y=0;
		playerBack.Normalize();
		float targetTheta=Mathf.Atan2(playerBack.z,playerBack.x);

		if(Mathf.Abs(_theta-targetTheta)>Mathf.PI)
		{
			if(_theta<targetTheta)
				_theta+=Mathf.PI*2f;
			else
				_theta-=Mathf.PI*2f;
		}

		_theta=Mathf.Lerp(_theta,targetTheta,_lerp*Time.deltaTime*_warmUp);

		//calc offset
		float y = Mathf.Sin(_phi);
		float xzRad = Mathf.Cos(_phi);
		float x = xzRad*Mathf.Cos(_theta);
		float z = xzRad*Mathf.Sin(_theta);
		_offset = new Vector3(x,y,z)*r;
		
		Vector3 targetPos=_player.transform.position+_offset;

		_position=targetPos;

		transform.forward=-_offset;
		Quaternion targetRot=transform.rotation;
		_rotation=Quaternion.Slerp(_rotation,targetRot,_slerp*Time.deltaTime*_warmUp);
		//_rotation=targetRot;

		transform.position=_position;
		transform.rotation=_rotation;

    }

	void OnDrawGizmos(){
		Gizmos.color=Color.blue;
		Gizmos.DrawLine(transform.position,transform.position+_offset);
	}
}
