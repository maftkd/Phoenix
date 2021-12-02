using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scope : MonoBehaviour
{
	Camera _cam;
	public float _minZoomFov;
	public float _maxZoomFov;
	float _defaultFov;
	float _zoomFov;
	public float _scrollThresh;
    // Start is called before the first frame update
    void Start()
    {
		_cam = Camera.main;
		_defaultFov = _cam.fieldOfView;
		_zoomFov = _maxZoomFov;
    }

    // Update is called once per frame
    void Update()
    {
		if(Input.GetMouseButton(0)||Input.GetMouseButton(1)){
			_cam.fieldOfView=_zoomFov;
			float scroll = Input.mouseScrollDelta.y;
			if(scroll>_scrollThresh){
				_zoomFov+=5f;
				if(_zoomFov>_maxZoomFov)
					_zoomFov=_maxZoomFov;
			}
			else if(scroll<-_scrollThresh){
				_zoomFov-=5f;
				if(_zoomFov<_minZoomFov)
					_zoomFov=_minZoomFov;
			}
		}
		else
		{
			_cam.fieldOfView=_defaultFov;
		}
        
    }
}
