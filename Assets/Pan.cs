using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pan : MonoBehaviour
{
	Transform _trackTarget;
	float _pitch;

	void Awake(){
		enabled=false;
		_pitch=transform.eulerAngles.x;
	}
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		transform.LookAt(_trackTarget);
		Vector3 eulers=transform.eulerAngles;
		eulers.x=_pitch;
		transform.eulerAngles=eulers;
    }

	public void StartTracking(Transform t){
		_trackTarget=t;
		enabled=true;
	}
}
