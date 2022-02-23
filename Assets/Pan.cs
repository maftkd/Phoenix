using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pan : Shot
{
	float _pitch;

	protected override void Awake(){
		base.Awake();
		_pitch=transform.eulerAngles.x;
	}
    // Start is called before the first frame update
    protected override void Start()
    {
        
    }

    // Update is called once per frame
    protected override void Update()
    {
		transform.LookAt(_target);
		Vector3 eulers=transform.eulerAngles;
		eulers.x=_pitch;
		transform.eulerAngles=eulers;
    }

	public override void StartTracking(Transform t){
		base.StartTracking(t);
	}
}
