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
	float _diff;
	float _diffLerp;
	
	public float _minDiffLerp;
	public float _maxDiffLerp;
	public float _minDiff;
	public float _maxDiff;
	[Tooltip("The rate at which control vector approaches raw input")]
	public float _controlLerp;
	[Tooltip("The minimum control vector before grounding diff to 0")]
	public float _minControl;
	public float _minVel;
	Vector3 _playerPrevPos;
	MInput _mIn;
	Vector3 _playerTarget;
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
	float _thetaTarget;
	Vector3 _playerPos;
	Vector3 vel;
	Vector2 _mouseIn;
	Vector3 _controlIn;
	Vector3 _prevControlIn;
	Vector3 _controlSmoothed;
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

		//calculate cam coords for next frame
		switch(_state){
			case 0:
			default://regular cam
				//vel = _player.forward;
				vel=_bird.GetVelocity();
				if(vel.sqrMagnitude<=_minVel)
					vel=_player.forward;
				_thetaTarget = CalcNextTheta(-vel);
				break;
			case 1://surround
				Vector3 diff = _playerPos-_camTarget.position;
				_thetaTarget = CalcNextTheta(diff);
				break;
		}
		float diffTargetActual = _thetaTarget-_theta;

		//allow faster turns while moving
		if(_controlIn.sqrMagnitude>_controlSmoothed.sqrMagnitude)
			_controlSmoothed=_controlIn;
		//when moving less, smoothly adjust control 
		else
		{
			_controlSmoothed=Vector3.Lerp(_controlSmoothed,_controlIn,Time.deltaTime*_controlLerp);
			if(_controlSmoothed.sqrMagnitude<_minControl*_minControl)
			{
				_controlSmoothed=Vector3.zero;
				//force diff to 0 if control is grounded
				_diff=0;
				diffTargetActual=0;
			}
		}

		//calc diff
		_diffLerp=Mathf.Lerp(_maxDiffLerp,_minDiffLerp,Mathf.InverseLerp(_minDiff,_maxDiff,Mathf.Abs(diffTargetActual)));
		_diff=Mathf.Lerp(_diff,diffTargetActual,Time.deltaTime*_diffLerp);
		DebugScreen.Print(_diff,0);

		//rotate
		_theta+=_diff*Time.deltaTime*_controlSmoothed.magnitude;//_bird.GetVel();
		//theta offset
		_theta+=-_mouseIn.x;

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
		//_rotationLerp=_minRotationLerp;
	}

	public void DefaultCam(){
		_state=0;
		//_thetaOffset=0;
		_letterBox.SetActive(false);
		_mIn.LockInput(false);
		_controlSmoothed=Vector3.zero;
		_diff=0;
		//_rotationLerp=_minRotationLerp;
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

	void OnDrawGizmos(){
	}
}
