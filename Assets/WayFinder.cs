using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayFinder : MonoBehaviour
{
	Transform _mate;
	Camera _cam;
	public float _vertOffset;

	void Awake(){
		_mate=GameManager._mate;
		_cam=GameManager._mCam.GetComponent<Camera>();
	}
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		Vector3 screenPoint=_cam.WorldToScreenPoint(_mate.position+Vector3.up*_vertOffset);
		if(screenPoint.z>0)
			transform.position=screenPoint;
    }
}
