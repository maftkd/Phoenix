using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MCamera : MonoBehaviour
{
	Transform _player;
	Bird _bird;
	public float _theta;
	public float _phi;
	public float _radius;
	public float _positionLerp;
	public float _minRotationLerp;
	public float _maxRotationLerp;
	float _rotationLerp;
	public float _rotationLerpAccel;
	public float _thetaOffsetGravity;
	public float _maxThetaOffset;
	public float _phiMult;
	public float _trackLerp;
	public float _trackRotLerp;
	Vector3 _playerPrevPos;
	MInput _mIn;
	Vector3 _playerTarget;
	public bool _invertY;
	public int _state;
	Transform _camTarget;
	Vector3 _targetPos;
	Vector3 _targetOffset;
	float _thetaOffset;
	GameObject _letterBox;

	[Header("Camera Slide")]
	public Vector3 _cameraSlide;
	Transform _camTransform;
	Camera _cam;

	[Header("Collision")]
	public float _colLerp;

	[Header("Surround")]
	public float _surroundLerp;
	public float _surroundDotAdjust;

	void Awake(){
		_bird=GameManager._player;
		_player=_bird.transform;
		_mIn=GameManager._mIn;
		_playerTarget=_player.position;
		_letterBox=transform.GetChild(0).gameObject;
		_cam=Camera.main;
		_cam.depthTextureMode= DepthTextureMode.Depth;
		_camTransform=_cam.transform;
		_camTransform.localPosition=_cameraSlide;
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

	//#temp - maybe move these
	float theta;
	Vector3 _playerPos;
	Vector3 vel;
	Vector2 _mouseIn;
	Vector3 _controlIn;
	Vector3 _prevControlIn;
    // Update is called once per frame
    void Update()
    {
		_playerPos=_bird.GetCamTarget();

		//track position
		float y = Mathf.Sin(_phi);
		float xzRad = Mathf.Cos(_phi);
		float x = xzRad*Mathf.Cos(_theta+_thetaOffset);
		float z = xzRad*Mathf.Sin(_theta+_thetaOffset);

		//calculate forward vector
		Vector3 offset = new Vector3(x,y,z);
		transform.forward=-offset;
		_playerTarget=Vector3.Lerp(_playerTarget,_playerPos,_positionLerp*Time.deltaTime);

		//move camera
		_camTransform.localPosition=_cameraSlide;
		//check collision
		RaycastHit hit;
		if(Physics.Raycast(_playerTarget+_cameraSlide.y*Vector3.up,offset, out hit, _radius, 1)){
			Debug.Log("Hit "+hit.transform.name);
			_targetPos=_playerTarget+offset*hit.distance;
			transform.position=Vector3.Lerp(transform.position,_targetPos,_colLerp*Time.deltaTime);
		}
		else
		{
			_targetPos=_playerTarget+offset*_radius;
			transform.position=_targetPos;
		}

		//Get input stuff
		_mouseIn=_mIn.GetMouseMotion();
		_controlIn=_mIn.GetControllerInput();
		CalcThetaOffset(-_mouseIn.x);
		CalcRotationLerp();


		//calculate cam coords for next frame
		switch(_state){
			case 0:
			default://regular cam
				vel = _player.forward;
				theta = CalcNextTheta(-vel);
				//_phi+=(_invertY?-1f : 1f )*mouseIn.y*_phiMult;
				break;
			case 1://surround
				Vector3 diff = _playerPos-_camTarget.position;
				theta = CalcNextTheta(diff);
				//_phi+=(_invertY?-1f : 1f )*mouseIn.y*_phiMult;
				break;
		}
		_theta=Mathf.Lerp(_theta,theta,_rotationLerp*Time.deltaTime);

		_playerPrevPos=_playerPos;
		_prevControlIn=_controlIn;
    }


	public void Shake(float v){
		//StartCoroutine(ShakeR(v));
	}

	public void SetCamPlane(Transform t){
	}


	public void MoveToTransform(Transform t){
	}

	public void Surround(Transform t){
		_state=1;
		_camTarget=t;
		_rotationLerp=_minRotationLerp;
	}

	public void DefaultCam(){
		_state=0;
		//_thetaOffset=0;
		_letterBox.SetActive(false);
		_mIn.LockInput(false);
		_rotationLerp=_minRotationLerp;
	}

	public void LetterBox(bool lb){
		_letterBox.SetActive(lb);
	}

	public bool IsDefaultCam(){
		return _state==0;
	}

	float CalcNextTheta(Vector3 desired){
		float t = Mathf.Atan2(desired.z,desired.x);

		//prevent negative angles
		if(t<0)
			t=Mathf.PI*2f+t;

		//prevent differences over 180 degrees
		if(Mathf.Abs(t-_theta)>Mathf.PI)
		{
			if(t<_theta)
				_theta=-(Mathf.PI*2f-_theta);
			else
				_theta+=Mathf.PI*2f;
		}
		return t;
	}

	void CalcThetaOffset(float delta){
		_thetaOffset+=delta;
		_thetaOffset=Mathf.Lerp(_thetaOffset,0,_controlIn.magnitude*Time.deltaTime*_thetaOffsetGravity);
		if(_thetaOffset>_maxThetaOffset)
			_thetaOffset=-_maxThetaOffset;
		else if(_thetaOffset<-_maxThetaOffset)
			_thetaOffset=_maxThetaOffset;
	}

	void CalcRotationLerp(){
		float dt = Vector3.Dot(_player.forward,transform.forward);
		dt=(dt+1)*0.5f;
		float control = _controlIn.magnitude;
		//rotations are stronger when dt is higher and control is higher

		//gravity
		if(control==0 && _rotationLerp>_minRotationLerp){
			_rotationLerp-=_rotationLerpAccel*Time.deltaTime;
		}
		else if(_rotationLerp<_maxRotationLerp){
			_rotationLerp+=_rotationLerpAccel*control*dt*Time.deltaTime;
		}
		/*
		if(_controlIn.magnitude>0&&_rotationLerp<_maxRotationLerp){
			_rotationLerp+=_rotationLerpAccel*_controlIn.magnitude*Time.deltaTime;
		}
		else if(_controlIn.magnitude==0&&_rotationLerp>_minRotationLerp){
			_rotationLerp-=_rotationLerpAccel*Time.deltaTime;
		}
		*/

		/*
		if(_letterBox.activeSelf)
			_rotationLerp=_maxRotationLerp;
			*/
	}

	void OnDrawGizmos(){
	}
}
