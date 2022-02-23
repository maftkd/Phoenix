using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HopCam : Shot
{

	Vector3 _position;
	Quaternion _rotation;

	protected override void Awake(){
		base.Awake();
	}

	void OnEnable(){
		_position=transform.position;
		_rotation=transform.rotation;
	}
    // Start is called before the first frame update
    protected override void Start()
    {
		base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
		//cancel out the parent/child effect
		transform.position=_position;
		transform.rotation=_rotation;
        
    }
}
