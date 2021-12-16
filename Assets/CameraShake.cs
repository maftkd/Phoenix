using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
	Camera _cam;
	Vector3 _defaultPos;
	public float _amplitude;
	public float _frequencyMult;
	public AnimationCurve _ampCurve;
	float _defaultSize;
	public float _zoomSize;
	public float _restoreSizeDur;
	public float _durationMult;
    // Start is called before the first frame update
    void Start()
    {
		_cam=GetComponent<Camera>();
		_defaultPos=Vector3.zero;
		_defaultPos.z=transform.position.z;
		_defaultSize=_cam.orthographicSize;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void Shake(float v){
		StartCoroutine(ShakeR(v));
	}

	IEnumerator ShakeR(float v){
		transform.position=_defaultPos;
		float timer =0;
		float t=0;
		float dur=v*_durationMult;
		_cam.orthographicSize=_zoomSize;
		while(timer<dur){
			timer+=Time.deltaTime;
			t=timer/v;
			transform.position=_defaultPos+Vector3.up*Mathf.Sin(timer*v*_frequencyMult)*_amplitude*v*_ampCurve.Evaluate(t);
			//_cam.orthographicSize=Mathf.Lerp(_defaultSize,_zoomSize,_fovCurve.Evaluate(t));
			yield return null;
		}
		timer=0;
		transform.position=_defaultPos;
		while(timer<_restoreSizeDur){
			timer+=Time.deltaTime;
			_cam.orthographicSize=Mathf.Lerp(_zoomSize,_defaultSize,timer/_restoreSizeDur);
			yield return null;
		}
		_cam.orthographicSize=_defaultSize;
	}
}
