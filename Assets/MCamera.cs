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
	[Header("Orbit")]
	public AnimationCurve _orbitCurve;

	public enum Transitions {CUT, FADE, WIPE, LERP, ORBIT};

	void Awake(){
		_camera=GetComponent<Camera>();
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
	}


    // Start is called before the first frame update
    void Start()
    {
    }


    // Update is called once per frame
    void Update()
    {
    }

	public void SnapToCamera(Camera cam){
		Transform camT = cam.transform;
		transform.position=camT.position;
		transform.rotation=camT.rotation;
		_camera.fieldOfView=cam.fieldOfView;
		transform.SetParent(camT);
	}

	public void Transition(Camera cam, Transitions transition,float letterBox=0,Transform target=null,float dur=1){
		switch(transition){
			case Transitions.WIPE:
				StartCoroutine(WipeTo(cam));
				break;
			case Transitions.FADE:
				StartCoroutine(FadeTo(cam,letterBox));
				break;
			case Transitions.ORBIT:
				StartCoroutine(OrbitTo(cam,target,dur,letterBox));
				break;
			default:
				break;
		}
	}

	//wipe
	IEnumerator WipeTo(Camera cam){
		float timer=0;
		float dur=_wipeDur*0.333f;
		while(timer<dur){
			timer+=Time.deltaTime;
			_wipe.SetFloat("_Amount",timer/dur);
			yield return null;
		}
		//snap to target cam
		SnapToCamera(cam);
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
	}

	//fade
	IEnumerator FadeTo(Camera cam,float letterBox){
		float timer=0;
		float dur=_fadeDur*0.333f;
		while(timer<dur){
			timer+=Time.deltaTime;
			_fade.SetFloat("_Amount",timer/dur);
			yield return null;
		}
		//snap to target cam
		SnapToCamera(cam);
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
	}

	//orbit
	IEnumerator OrbitTo(Camera cam, Transform target, float dur, float letterBox){
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

		transform.SetParent(null);
		float timer=0;
		float radius=0;
		while(timer<dur){
			timer+=Time.deltaTime;
			float frac=_orbitCurve.Evaluate(timer/dur);
			radius=Mathf.Lerp(startR,targetR,frac);
			transform.rotation=Quaternion.Slerp(startRot,endRot,frac);
			Vector3 pos=target.position-transform.forward*radius;
			pos.y=Mathf.Lerp(startHeight,endHeight,frac);
			transform.position=pos;
			_camera.fieldOfView=Mathf.Lerp(startFov,targetFov,frac);
			_letterBox.SetFloat("_Amount",Mathf.Lerp(startLb,endLb,frac));
			yield return null;
		}

		transform.SetParent(cam.transform);
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
}
