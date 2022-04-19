using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MCamera : MonoBehaviour
{
	Camera _camera;
	Camera[] _cams;
	[Header("Wipe")]
	public Material _wipe;
	public float _wipeDur;
	[Header("Fade")]
	public Material _fade;
	public float _fadeDur;
	[Header("Letterbox")]
	public Material _letterBox;
	[Header("Vignette")]
	public Material _vignette;
	[Header("Orbit")]
	public AnimationCurve _orbitCurve;
	[Header("Fov")]
	public float _bigFov;
	float _defaultFov;
	public float _fovLerp;
	float _targetFov;

	public enum Transitions {CUT, FADE, WIPE, LERP, ORBIT, CUT_BACK};

	Camera _prevCam;
	public bool _cutBackEnabled;
	Camera _targetCam;
	public float _lerp;
	Shot _curShot;
	bool _transitioning;

	void Awake(){
		_cutBackEnabled=true;
		_camera=GetComponent<Camera>();
		_defaultFov=_camera.fieldOfView;
		_targetFov=_defaultFov;
		//disable all cameras
		_cams=FindObjectsOfType<Camera>();
		foreach(Camera c in _cams)
			c.enabled=false;
		//except this one
		_camera.enabled=true;

		//reset transitions
		_wipe.SetFloat("_Amount",0);
		_fade.SetFloat("_Amount",0);
		_letterBox.SetFloat("_Amount",0);
		_vignette.SetFloat("_Amount",0);
	}


    // Start is called before the first frame update
    void Start()
    {
    }


    // Update is called once per frame
    void Update()
    {
		if(Input.GetKeyDown(KeyCode.F2))
			_cutBackEnabled=!_cutBackEnabled;

		//tracking
		if(_targetCam!=null)
		{
			transform.position=Vector3.Lerp(transform.position,_targetCam.transform.position,_lerp*Time.deltaTime);
			transform.rotation=Quaternion.Slerp(transform.rotation,_targetCam.transform.rotation,_lerp*Time.deltaTime);
			_camera.fieldOfView=Mathf.Lerp(_camera.fieldOfView,_targetFov,_fovLerp*Time.deltaTime);
		}
    }

	public void Snap(){
		transform.rotation=_targetCam.transform.rotation;
		transform.position=_targetCam.transform.position;
	}

	public void SnapToCamera(Camera cam,bool handleTracking=true){
		Transform camT = cam.transform;
		transform.position=camT.position;
		transform.rotation=camT.rotation;
		_camera.fieldOfView=cam.fieldOfView;
		_camera.orthographic=cam.orthographic;
		if(_camera.orthographic)
			_camera.orthographicSize=cam.orthographicSize;
		//if(_cutBackEnabled)
			//transform.SetParent(camT);
		if(_cutBackEnabled)
			_targetCam=cam;
		else
			_targetCam=null;
		if(handleTracking)
		{
			HandleTracking(cam,null,0.001f);
			_prevCam=cam;
		}
	}
	public void Transition(Camera cam, Transitions transition,float letterBox=0,Transform target=null,float dur=0f,
			bool overridePriority=false,bool stopImmediate=false){
		if(this==null)
			return;

		//random ass logic used when going into birdhouse, cam is same, and fading
		//Any other time the cams are the same, we ignore this operation
		//Cams are sometimes same when switching from hopping to waddling in Bird
		//Not ideal, but may work
		bool sameCam=_prevCam!=null&&cam==_prevCam;
		if(sameCam&&transition!=Transitions.FADE)
			return;
		//check priority
		if(!overridePriority&&_prevCam!=null){
			if(cam.GetComponent<Shot>()!=null){
				Shot prevShot = _prevCam.GetComponent<Shot>();
				int prevPriority = prevShot==null? 10 : prevShot._priority;
				Shot nextShot = cam.GetComponent<Shot>();
				int nextPriority = nextShot._priority;

				//ignore transition of newer camera is lower priority
				if(nextPriority<prevPriority){
					return;
				}
			}
		}
		StartCoroutine(TransitionR(cam,transition,letterBox,target,dur,overridePriority,stopImmediate));
	}

	IEnumerator TransitionR(Camera cam, Transitions transition,float letterBox=0,Transform target=null,float dur=0f,
			bool overridePriority=false,bool stopImmediate=false){

		//wait for any existing transition to finish first
		while(_transitioning)
			yield return null;

		switch(transition){
			case Transitions.WIPE:
				StartCoroutine(WipeTo(cam));
				break;
			case Transitions.FADE:
				StartCoroutine(FadeTo(cam,letterBox,dur));
				break;
			case Transitions.ORBIT:
				StartCoroutine(OrbitTo(cam,target,dur,letterBox));
				break;
			case Transitions.CUT_BACK:
				CutBack(cam);
				break;
			case Transitions.LERP:
				StartCoroutine(LerpTo(cam,dur));
				break;
			default:
				break;
		}

		//stop tracking old, start tracking new
		//check for same cam transition
		bool sameCam=_prevCam!=null&&cam==_prevCam;
		if(!sameCam)
			HandleTracking(cam,target, dur,stopImmediate);
	}

	void HandleTracking(Camera newCam,Transform t,float dur, bool stopImmediate=false){
		//new camera starts tracking
		Shot shot = newCam.GetComponent<Shot>();
		if(shot!=null){
			shot.StartTracking(t);
			_curShot=shot;
		}

		//old camera stops tracking after some duration
		if(_prevCam!=null){
			shot = _prevCam.GetComponent<Shot>();
			if(shot!=null)
			{
				if(dur==0||stopImmediate)
					shot.StopTracking();
				else
					StartCoroutine(StopTrackingShot(shot,dur));
			}
		}


		_prevCam=newCam;

	}

	IEnumerator StopTrackingShot(Shot shot, float delay){
		yield return new WaitForSeconds(delay);
		shot.StopTracking();
	}

	//wipe
	IEnumerator WipeTo(Camera cam){
		_transitioning=true;
		float timer=0;
		float dur=_wipeDur*0.333f;
		while(timer<dur){
			timer+=Time.deltaTime;
			_wipe.SetFloat("_Amount",timer/dur);
			yield return null;
		}
		//snap to target cam
		SnapToCamera(cam,false);
		_wipe.SetFloat("_Amount",1f);

		//hold
		yield return new WaitForSeconds(dur);

		//wipe down
		timer=0;
		while(timer<dur){
			timer+=Time.deltaTime;
			_wipe.SetFloat("_Amount",1-timer/dur);
			yield return null;
		}
		_wipe.SetFloat("_Amount",0f);
		_transitioning=false;
	}

	//fade
	IEnumerator FadeTo(Camera cam,float letterBox,float dur){
		_transitioning=true;
		float timer=0;
		dur*=0.333f;
		while(timer<dur){
			timer+=Time.deltaTime;
			_fade.SetFloat("_Amount",timer/dur);
			yield return null;
		}
		//snap to target cam
		SnapToCamera(cam,false);
		_fade.SetFloat("_Amount",1f);

		//hold
		yield return new WaitForSeconds(dur);
		_letterBox.SetFloat("_Amount",letterBox);

		//wipe down
		timer=0;
		while(timer<dur){
			timer+=Time.deltaTime;
			_fade.SetFloat("_Amount",1-timer/dur);
			yield return null;
		}
		_fade.SetFloat("_Amount",0f);
		_transitioning=false;
	}

	//orbit
	IEnumerator OrbitTo(Camera cam, Transform target, float dur, float letterBox){
		_transitioning=true;
		float startR=(target.position-transform.position).magnitude;
		float targetR=(target.position-cam.transform.position).magnitude;
		float startLb=_letterBox.GetFloat("_Amount");
		float endLb=letterBox;
		Quaternion startRot=transform.rotation;
		Quaternion endRot=cam.transform.rotation;
		float startFov=_camera.fieldOfView;
		float targetFov=cam.fieldOfView;
		float startHeight=transform.position.y;
		float endHeight=cam.transform.position.y;

		Vector3 startPos=transform.position;
		Vector3 endPos=cam.transform.position;

		transform.SetParent(null);
		float timer=0;
		float radius=0;
		while(timer<dur){
			timer+=Time.deltaTime;
			float frac=_orbitCurve.Evaluate(timer/dur);
			radius=Mathf.Lerp(startR,targetR,frac);
			transform.rotation=Quaternion.Slerp(startRot,endRot,frac);
			//Vector3 pos=target.position-transform.forward*radius;
			//pos.y=startHeight;
			//pos.y=Mathf.Lerp(startHeight,endHeight,frac);
			//transform.position=pos;
			transform.position=Vector3.Lerp(startPos,endPos,frac);
			_camera.fieldOfView=Mathf.Lerp(startFov,targetFov,frac);
			_letterBox.SetFloat("_Amount",Mathf.Lerp(startLb,endLb,frac));
			yield return null;
		}
		_letterBox.SetFloat("_Amount",endLb);
		SnapToCamera(cam,false);
		_transitioning=false;
	}

	public void LerpLetterBox(float target, float dur){
		StartCoroutine(LerpLetterBoxR(target,dur));
	}

	IEnumerator LerpLetterBoxR(float target, float dur){
		float startLb=_letterBox.GetFloat("_Amount");
		float endLb=target;
		float timer=0;
		while(timer<dur){
			timer+=Time.deltaTime;
			float frac=(timer/dur);
			_letterBox.SetFloat("_Amount",Mathf.Lerp(startLb,endLb,frac));
			yield return null;
		}
		_letterBox.SetFloat("_Amount",target);
	}

	void CutBack(Camera cam){
		//copy from cur to next
		cam.transform.position=transform.position;
		cam.transform.rotation=transform.rotation;
		cam.fieldOfView=_camera.fieldOfView;
		//parent
		SnapToCamera(cam,false);
	}

	IEnumerator LerpTo(Camera target, float dur){
		_transitioning=true;
		transform.SetParent(null);

		Vector3 startPos=transform.position;
		Quaternion startRot=transform.rotation;
		float startFov=_camera.fieldOfView;

		float timer=0;
		while(timer<dur){
			timer+=Time.deltaTime;
			float frac=_orbitCurve.Evaluate(timer/dur);
			transform.position=Vector3.Lerp(startPos,target.transform.position,frac);
			transform.rotation=Quaternion.Slerp(startRot,target.transform.rotation,frac);
			_camera.fieldOfView=Mathf.Lerp(startFov,target.fieldOfView,frac);
			yield return null;
		}

		SnapToCamera(target,false);
		_transitioning=false;
	}

	public void EnableCurrentShot(bool en){
		if(_curShot!=null)
			_curShot.enabled=en;
	}

	public void SetVignette(float f){
		_vignette.SetFloat("_Amount", f);
	}

	public void SetFovFrac(float frac){
		_targetFov=Mathf.Lerp(_defaultFov,_bigFov,frac);
	}

	void OnDrawGizmos(){
		if(_camera!=null){
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color=Color.green;
			Gizmos.DrawFrustum(Vector3.zero,_camera.fieldOfView,1f,0.01f,1.777f);
		}
		if(_cams!=null){
			foreach(Camera c in _cams){
				if(c!=_camera){
					Gizmos.matrix=c.transform.localToWorldMatrix;
					Gizmos.color=Color.red;
					Gizmos.DrawFrustum(Vector3.zero,c.fieldOfView,0.5f,0.01f,1.777f);
				}
			}
		}
	}

	public Camera GetCurTargetCam(){
		return _targetCam;
	}
}
