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
	public float _rotationLerp;
	public float _surroundLerp;
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
	
	void Awake(){
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		_player=player.transform;
		_bird=_player.GetComponent<Bird>();
		_mIn=GetComponent<MInput>();
		_playerTarget=_player.position;
		_letterBox=transform.GetChild(0).gameObject;
		_cam=Camera.main;
		_camTransform=_cam.transform;
		_camTransform.localPosition=_cameraSlide;
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

	float theta;
	Vector3 _playerPos;
    // Update is called once per frame
    void Update()
    {
		_playerPos=_bird.GetCamTarget();

		//track position
		float y = _radius*Mathf.Sin(_phi);
		float xzRad = _radius*Mathf.Cos(_phi);
		float x = xzRad*Mathf.Cos(_theta+_thetaOffset);
		float z = xzRad*Mathf.Sin(_theta+_thetaOffset);
		Vector3 offset = new Vector3(x,y,z);
		_playerTarget=Vector3.Lerp(_playerTarget,_playerPos,_positionLerp*Time.deltaTime);

		//track rotation
		Vector2 mouseIn=_mIn.GetMouseMotion();
		Vector3 controlIn=_mIn.GetControllerInput();
		float in01=controlIn.magnitude;
		switch(_state){
			case 0:
			default://regular cam
				_camTransform.localPosition=_cameraSlide;
				transform.forward=-offset;
				transform.position=_playerTarget+offset;
				if(in01>0){
					Vector3 vel = _playerPos-_playerPrevPos;
					theta = Mathf.Atan2(-vel.z,-vel.x);
					if(theta<0)
						theta=Mathf.PI*2f+theta;

					if(Mathf.Abs(theta-_theta)>Mathf.PI)
					{
						if(theta<_theta)
							_theta=-(Mathf.PI*2f-_theta);
						else
							_theta+=Mathf.PI*2f;
					}
					_theta=Mathf.Lerp(_theta,theta,in01*_rotationLerp*Time.deltaTime);
				}
				_thetaOffset+=-mouseIn.x;
				_thetaOffset=Mathf.Lerp(_thetaOffset,0,controlIn.magnitude*Time.deltaTime*_thetaOffsetGravity);
				if(_thetaOffset>_maxThetaOffset)
					_thetaOffset=-_maxThetaOffset;
				else if(_thetaOffset<-_maxThetaOffset)
					_thetaOffset=_maxThetaOffset;
				//_thetaOffset=Mathf.Clamp(_thetaOffset,-_maxThetaOffset,_maxThetaOffset);
				_phi+=(_invertY?-1f : 1f )*mouseIn.y*_phiMult;
				break;
			case 1://surround
				_camTransform.localPosition=_cameraSlide;
				transform.forward=-offset;
				transform.position=_playerTarget+offset;
				if(in01>0||_mIn.InputLocked()){
					Vector3 diff = _playerPos-_camTarget.position;
					theta = Mathf.Atan2(diff.z,diff.x);
					if(theta<0)
						theta=Mathf.PI*2f+theta;

					if(Mathf.Abs(theta-_theta)>Mathf.PI)
					{
						if(theta<_theta)
							_theta=-(Mathf.PI*2f-_theta);
						else
							_theta+=Mathf.PI*2f;
					}
					_theta=Mathf.Lerp(_theta,theta,_surroundLerp*Time.deltaTime);
				}
				_thetaOffset+=-mouseIn.x;
				_thetaOffset=Mathf.Lerp(_thetaOffset,0,controlIn.magnitude*Time.deltaTime*_thetaOffsetGravity);
				//_thetaOffset=Mathf.Clamp(_thetaOffset,-_maxThetaOffset,_maxThetaOffset);
				if(_thetaOffset>_maxThetaOffset)
					_thetaOffset=-_maxThetaOffset;
				else if(_thetaOffset<-_maxThetaOffset)
					_thetaOffset=_maxThetaOffset;
				_phi+=(_invertY?-1f : 1f )*mouseIn.y*_phiMult;
				break;
			case 2:
				_camTransform.localPosition=Vector3.zero;
				transform.position=Vector3.Lerp(transform.position,_targetPos,_trackLerp*Time.deltaTime);
				Quaternion rot=transform.rotation;
				transform.LookAt(_camTarget.position+_targetOffset);
				Quaternion tRot=transform.rotation;
				transform.rotation=Quaternion.Slerp(rot,tRot,_trackRotLerp);
				break;
		}

		_playerPrevPos=_playerPos;
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
		Debug.Log("Surrounding "+t.name);
	}

	public void DefaultCam(){
		_state=0;
		//_thetaOffset=0;
		_letterBox.SetActive(false);
		_mIn.LockInput(false);
		Debug.Log("herm");
	}

	public void TrackTarget(Transform t,Vector3 offset){
		TrackTargetFrom(t,transform.position,offset);
	}

	public void TrackTargetFrom(Transform t,Vector3 pos,Vector3 offset){
		_state=2;
		_camTarget=t;
		_targetPos=pos;
		_targetOffset=offset;
		_letterBox.SetActive(true);
		_mIn.LockInput(true);
	}

	public void LetterBox(bool lb){
		_letterBox.SetActive(lb);
	}
}
