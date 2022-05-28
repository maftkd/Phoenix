using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeciesCam : Shot
{
	Vector3 _position;
	Quaternion _rotation;
	public float _lerp;
	public Vector3 _offset;
	public float _offsetAngle;
	public float _slerp;
	Vector3 _startPos;
	bool _revert;
	float _revertDur;
	float _revertTimer;
	Vector3 _revertStart;

	protected override void Awake(){
		base.Awake();
	}

	void OnEnable(){
		_position=transform.position;
		_rotation=transform.rotation;
		_startPos=_position;
		_revert=false;
		_revertTimer=0f;
	}

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

		if(!_revert){
			transform.LookAt(_target.position);
			Quaternion targetRot=transform.rotation;
			_rotation=Quaternion.Slerp(_rotation,targetRot,_slerp*Time.deltaTime);
			transform.rotation=_rotation;
			Vector3 targetPos=_target.position-transform.forward*_offset.z;
			_position=Vector3.Lerp(_position,targetPos,_lerp*Time.deltaTime);
		}
		else{
			_revertTimer+=Time.deltaTime;
			float frac=_revertTimer/_revertDur;
			if(_revertTimer>_revertDur){
				frac=1f;
			}
			_position=Vector3.Lerp(_revertStart,_startPos,frac);
		}

		transform.position=_position;
		transform.rotation=_rotation;

    }

	public void Revert(float f){
		_revert=true;
		_revertDur=f;
		_revertStart=_position;
	}
}
