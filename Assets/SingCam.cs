using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingCam : Shot
{

	Vector3 _position;
	Quaternion _rotation;
	public float _lerp;
	public Vector3 _offset;
	public float _offsetAngle;
	public float _slerp;

	protected override void Awake(){
		base.Awake();
	}

	void OnEnable(){
		_position=transform.position;
		_rotation=transform.rotation;
	}

	/*
	public override void StartTracking(Transform t){
		base.StartTracking(t);
	}

	public override void StopTracking(){
		base.StopTracking();
	}
	*/

    // Start is called before the first frame update
    protected override void Start()
    {
		base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
		base.Update();

		transform.position=_position;
		transform.rotation=_rotation;

		Vector3 dir=_target.position-_player.transform.position;
		dir.Normalize();
		dir=Quaternion.Euler(0,_offsetAngle,0)*dir;
		Vector3 targetPos=_player.transform.position-dir*_offset.z;
		targetPos+=Vector3.up*_offset.y;
		targetPos+=transform.right*_offset.x;
		_position=Vector3.Lerp(_position,targetPos,_lerp*Time.deltaTime);

		Vector3 halfway=Vector3.Lerp(_player.transform.position,_target.position,0.5f);
		transform.LookAt(halfway+Vector3.up*_offset.y);
		_rotation=Quaternion.Slerp(_rotation,transform.rotation,_slerp*Time.deltaTime);

		transform.position=_position;
		transform.rotation=_rotation;

    }
}
