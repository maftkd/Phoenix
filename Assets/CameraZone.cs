using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZone : MonoBehaviour
{
	MCamera _mCam;
	Transform _camTarget;

	void Awake(){
		_mCam=Camera.main.GetComponent<MCamera>();
		_camTarget=transform.GetChild(0);
		if(_camTarget.GetComponent<MeshRenderer>()!=null)
			_camTarget.GetComponent<MeshRenderer>().enabled=false;
	}
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	void OnTriggerEnter(Collider other){
		Debug.Log("we are in");
		_mCam.SetCamPlane(_camTarget);
	}

	void OnTriggerExit(Collider other){
		Debug.Log("We are out");
		_mCam.SetCamPlane(null);
	}
}
