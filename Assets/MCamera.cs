using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MCamera : MonoBehaviour
{
	public Vector3 _followOffset;
	public Vector3 _followBack;
	public float _lerpSpeed;
	public float _slerpMult;
	public float _minAngleLerp;
	public float _maxAngleLerp;
	Vector3 _worldSpaceInput;
	Vector3 _controllerInput;
	public static Transform _player;
	Hop _hop;
	Fly _fly;
	[Header("flying")]
	public float _flyAngleLerp;
	public float _maxPitch;
	public float _flyingPitchOffset;
	[Header("Camera shake")]
	public float _durationMult;
	public float _frequencyMult;
	public float _amplitude;
	public AnimationCurve _ampCurve;
	Vector3 _shake;
	int _state;
	Transform _camTarget;
	[Header("Planar cam")]
	public float _planeCamLerp;
	Text _debugText;
	Quaternion _prevTargetRot;
	float _slerpTimer;
	[Header("Fov")]
	public float _maxFov;
	public float _fovLerp;
	float _defaultFov;
	Camera _cam;
	[Header("Input")]
	public float _shiftSlowDown;
	
	void Awake(){
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		_player=player.transform;
		_hop=_player.GetComponent<Hop>();
		_fly=_player.GetComponent<Fly>();
		_followBack=-_player.forward;
		_debugText=transform.GetComponentInChildren<Text>();
		_cam = GetComponent<Camera>();
		_defaultFov=_cam.fieldOfView;
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

	Vector3 targetPos;
    // Update is called once per frame
    void Update()
    {
		CalcInputVector();
		switch(_state){
			case 0:
			default:
				float angleLerp=_maxAngleLerp;
				//float angleLerp = Mathf.Lerp(_maxAngleLerp,_minAngleLerp,-_controllerInput.y);

				if(_fly.enabled)
					angleLerp=_flyAngleLerp;

				float targetFov=_defaultFov;

				//face players back
				Quaternion curRot=transform.rotation;
				Vector3 birdBack=-_player.forward;
				transform.forward=_player.forward;
				Vector3 eulers;
				if(_fly.enabled)
				{
					//if flying forward, follow velocity
					if(_fly._forwardness>0)
						transform.forward=_fly._velocity.normalized;
					eulers = transform.eulerAngles;
					float x = eulers.x;
					if(x>180)
						x=-(360-x);
					else if(x<-180)
						x=(360+x);
					if(Mathf.Abs(x)>_maxPitch){
						eulers.x=_maxPitch*Mathf.Sign(x);
					}
					eulers.x+=_flyingPitchOffset;
					transform.eulerAngles=eulers;
					targetFov=Mathf.Lerp(_defaultFov,_maxFov,_fly.GetSpeedFraction());
				}

				Quaternion targetRot=transform.rotation;
				float ang = Quaternion.Angle(targetRot,curRot);
				//lerps slower for larger angle diffs
				float lerpMult=Mathf.Min(1f,_slerpMult/ang);
				transform.rotation=Quaternion.Slerp(curRot,targetRot,angleLerp*Time.deltaTime*lerpMult);

				//lerp fov
				_cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView,targetFov,_fovLerp*Time.deltaTime);

				//remove roll
				eulers = transform.eulerAngles;
				eulers.z=0;
				transform.eulerAngles=eulers;

				//lerp position based on rotation and follow offset vector
				targetPos=_player.position-transform.forward*_followOffset.z+Vector3.up*_followOffset.y;
				if(_hop.enabled)
					targetPos = _hop.GetCamTarget()-transform.forward*_followOffset.z+Vector3.up*_followOffset.y;
				transform.position = Vector3.Lerp(transform.position,targetPos,_lerpSpeed*Time.deltaTime)+_shake;
				break;
			case 1://planar camera
				targetPos = _camTarget.position;
				targetPos.z=_player.position.z;
				transform.position = Vector3.Lerp(transform.position,targetPos,_planeCamLerp*Time.deltaTime);
				transform.rotation = Quaternion.Slerp(transform.rotation,_camTarget.rotation,_planeCamLerp*Time.deltaTime);
				break;
			case 2:
				break;
		}
    }

	void CalcInputVector(){
		float verIn = Input.GetAxis("Vertical");
		float horIn = Input.GetAxis("Horizontal");
		_controllerInput=new Vector3(horIn,verIn,0);
		RemapInputFromSquareToCircle();
		Vector3 flatForward=transform.forward;
		flatForward.y=0;
		flatForward.Normalize();
		Vector3 flatRight=Vector3.Cross(Vector3.up,flatForward);
		_worldSpaceInput = Vector3.zero;
		//if(Mathf.Abs(verIn)>_hop._inThresh)
			_worldSpaceInput+=verIn*flatForward;
		//if(Mathf.Abs(horIn)>_hop._inThresh)
			_worldSpaceInput+=horIn*flatRight;
		float sqrMag=_worldSpaceInput.sqrMagnitude;
		if(sqrMag>1)
			_worldSpaceInput.Normalize();
	}

	public Vector3 GetInputDir(){
		//#temp - until pc input integrated
		if(Input.GetKey(KeyCode.LeftShift))
			return _worldSpaceInput*_shiftSlowDown;
		return _worldSpaceInput;
	}

	public Vector3 GetControllerInput(){
		//#temp - until pc input integrated
		if(Input.GetKey(KeyCode.LeftShift))
			return _controllerInput*_shiftSlowDown;
		return _controllerInput;
	}

	public void Shake(float v){
		StartCoroutine(ShakeR(v));
	}

	IEnumerator ShakeR(float v){
		yield return null;
		float timer = 0;
		float t = 0;
		float dur = v*_durationMult;
		while(timer<dur){
			timer+=Time.deltaTime;
			t=timer/dur;
			_shake=Vector3.up*Mathf.Sin(timer*v*_frequencyMult)*_amplitude*v*_ampCurve.Evaluate(t);
			yield return null;
		}
		_shake=Vector3.zero;
	}

	public void SetCamPlane(Transform t){
		if(t==null)
			_state=0;
		else{
			_camTarget=t;
			_state=1;
		}
	}

	void RemapInputFromSquareToCircle(){
		float theta = Mathf.Atan2(_controllerInput.y,_controllerInput.x)*Mathf.Rad2Deg;
		float maxX=0;
		float maxY=0;
		if(45f-Mathf.Abs(theta)>=0||Mathf.Abs(theta)>=135f)
		{
			maxX=1f;
			maxY=Mathf.Abs(Mathf.Tan(theta*Mathf.Deg2Rad));
		}
		else
		{
			maxY=1f;
			maxX=Mathf.Abs(1f/(Mathf.Tan(theta*Mathf.Deg2Rad)));
		}
		float max = Mathf.Sqrt(maxX*maxX+maxY*maxY);
		//#hack - some reason we aren't seeing values above the 1.2 range I would expect 1*sqrt(2)
		max = Mathf.Min(1.2f,max);
		string db ="raw: "+_controllerInput.magnitude;
		//Debug.Log("raw: "+_controllerInput.magnitude);
		db+="\nmax: "+max;
		_debugText.text=db;
		//Debug.Log("max: "+max);
		_controllerInput/=max;
		//Debug.Log("mag: "+_controllerInput.magnitude);
		if(_controllerInput.magnitude>1f)
		{
			//Debug.Log("yoooo");
			//Debug.Break();
		}
	}
}
