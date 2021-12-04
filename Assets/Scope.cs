using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scope : MonoBehaviour
{
	Camera _cam;
	public float _minZoomFov;
	public float _maxZoomFov;
	float _defaultFov;
	float _zoomFov;
	public float _scrollThresh;
	public Canvas _can;
	bool _wasZoom;
	Text _debugText;
	AudioSource _zoomSource;
	AudioSource _shutterSource;
    // Start is called before the first frame update
    void Start()
    {
		_cam = Camera.main;
		_defaultFov = _cam.fieldOfView;
		_zoomFov = _maxZoomFov;
		_can.enabled=false;
		_debugText=_can.transform.GetComponentInChildren<Text>();
		_zoomSource = transform.Find("Zoom").GetComponent<AudioSource>();
		_shutterSource = transform.Find("Shutter").GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
		bool zoom=false;
		if(Input.GetMouseButton(1)){
			zoom=true;
			_cam.fieldOfView=_zoomFov;
			float scroll = Input.mouseScrollDelta.y;
			if(scroll>_scrollThresh){
				//zoom audio
				if(_zoomFov<_maxZoomFov)
					_zoomSource.Play();
				_zoomFov+=5f;
				if(_zoomFov>_maxZoomFov)
					_zoomFov=_maxZoomFov;
			}
			else if(scroll<-_scrollThresh){
				//zoom audio
				if(_zoomFov>_minZoomFov)
					_zoomSource.Play();
				_zoomFov-=5f;
				if(_zoomFov<_minZoomFov)
					_zoomFov=_minZoomFov;
			}

			//raycast for bird
			RaycastHit hit;
			if(Physics.Raycast(transform.position,transform.forward,out hit,150f,1)){
				_debugText.text=hit.transform.name;
				if(hit.transform.GetComponent<Bird>()!=null){
					int state = hit.transform.GetComponent<Bird>()._state;
					_debugText.text+="\nState: "+state;
				}
			}
			else
				_debugText.text="";

			if(Input.GetMouseButtonDown(0)){
				_shutterSource.Play();
			}
		}
		else
		{
			_cam.fieldOfView=_defaultFov;
		}

		if(zoom!=_wasZoom)
			_can.enabled=zoom;
        
		_wasZoom=zoom;
    }
}
