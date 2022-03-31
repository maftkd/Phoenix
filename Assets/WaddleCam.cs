using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaddleCam : Shot
{

	Vector3 _position;
	Quaternion _rotation;
	bool _debugLines;
	float _radius;
	float _phi;
	float _theta;
	float _yOffset=-1f;
	float _originalYOffset;
	float _originalTargetR;
	float _targetR=-1f;
	float _targetPhi;
	public Vector2 _phiRange;
	public float _rLerp;
	public float _thetaLerp;
	public float _phiLerp;
	public bool _camControl;

	protected override void Awake(){
		base.Awake();
		_position=transform.position;
		_rotation=transform.rotation;
		SetDebugLines(false);
	}

	void OnEnable(){
		_position=transform.position;
		_rotation=transform.rotation;
		//get initial phi radius and theta
		if(_yOffset<0){
			Vector3 dir = transform.forward;
			Vector3 sphereCenter=Vector3.zero;
			LineLineIntersection(out sphereCenter,transform.position,dir,
					_player.transform.position,Vector3.up);
			_yOffset=sphereCenter.y-_player.transform.position.y;
			_originalYOffset=_yOffset;
		}
		else
			_yOffset=_originalYOffset*_player.transform.localScale.x;
		Vector3 diff=transform.position-(_player.transform.position+Vector3.up*_yOffset);
		_radius = diff.magnitude;
		if(_targetR<0)
		{
			_targetR=_radius;
			_originalTargetR=_targetR;
		}
		else
			_targetR=_originalTargetR*_player.transform.localScale.x;
		_phi = Mathf.Asin(diff.y/diff.magnitude);
		if(_targetPhi==0)
			_targetPhi=_phi;

		//calc theta
		Vector3 camBack=-transform.forward;
		camBack.y=0;
		camBack.Normalize();
		_theta=Mathf.Atan2(camBack.z,camBack.x);
	}

    // Start is called before the first frame update
    protected override void Start()
    {

    }

    // Update is called once per frame
    protected override void Update()
    {
		//cancel out the parent/child effect
		transform.position=_position;
		transform.rotation=_rotation;

		//get radius
		Vector3 diff=transform.position-(_player.transform.position+
				Vector3.up*_yOffset);
		float radius=Mathf.Lerp(diff.magnitude,_targetR,
				_rLerp*Time.deltaTime);

		_phi = Mathf.Lerp(_phi,_targetPhi,_phiLerp*Time.deltaTime);

		//modify theta
		if(_camControl){
			Vector2 mouseMotion = _mIn.GetMouseMotion();
			_theta-=mouseMotion.x;
			_phi+=mouseMotion.y;
			_phi=Mathf.Clamp(_phi,_phiRange.x,_phiRange.y);
		}

		//theta help
		Vector3 birdBack=-_player.transform.forward;
		float pTheta=Mathf.Atan2(birdBack.z,birdBack.x);
		//check for large diff
		if(Mathf.Abs(pTheta-_theta)>Mathf.PI){
			if(pTheta<_theta)
				_theta=-(Mathf.PI*2f-_theta);
			else
				_theta+=Mathf.PI*2f;
		}

		float birdVel=_player.GetVel();
		_theta=Mathf.Lerp(_theta,pTheta,_thetaLerp*Time.deltaTime*birdVel);

			//update position
		float y = Mathf.Sin(_phi);
		float xzRad = Mathf.Cos(_phi);
		float x = xzRad*Mathf.Cos(_theta);
		float z = xzRad*Mathf.Sin(_theta);
		Vector3 offset = new Vector3(x,y,z)*radius;
		
		Vector3 targetPos=_player.transform.position+Vector3.up*_yOffset+offset;

		_position=targetPos;

		//float pitch=transform.eulerAngles.x;
		Quaternion prevRot=transform.rotation;
		transform.forward=-offset;
		_rotation=transform.rotation;

		transform.position=_position;
		transform.rotation=_rotation;

    }

	public void ToggleCamLines(){
		_debugLines=!_debugLines;
		SetDebugLines(_debugLines);
	}

	public void AdjustTheta(float amount){
		_theta+=amount;
	}

	void SetDebugLines(bool on){
		RawImage [] images = transform.GetComponentsInChildren<RawImage>();
		foreach(RawImage ri in images)
			ri.enabled=on;
	}

	public static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1,
	Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2){

		Vector3 lineVec3 = linePoint2 - linePoint1;
		Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
		Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

		float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

		//is coplanar, and not parallel
		if( Mathf.Abs(planarFactor) < 0.0001f 
		&& crossVec1and2.sqrMagnitude > 0.0001f)
		{
			float s = Vector3.Dot(crossVec3and2, crossVec1and2) 
				/ crossVec1and2.sqrMagnitude;
			intersection = linePoint1 + (lineVec1 * s);
			return true;
		}
		else
		{
			intersection = Vector3.zero;
			return false;
		}
	}

	public void ResetCamera(){
		OnEnable();
	}
	/*
	public void ScaleTargetRadius(float mult){
		//_targetR=_originalTargetR*mult;
	}
	*/
}
