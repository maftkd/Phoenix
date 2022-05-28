using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingCam : Shot
{
	Vector3 _position;
	Quaternion _rotation;
	public Vector3 _offset;
	Vector3 _startPos;
	Vector3 _targetPos;
	Quaternion _startRot;
	Quaternion _targetRot;
	public float _animDur;
	float _timer;
	public AnimationCurve _curve;

	protected override void Awake(){
		base.Awake();
	}

	void OnEnable(){
		_position=transform.position;
		_rotation=transform.rotation;
		_startPos=_position;
		_startRot=_rotation;
		_targetPos=_target.position-_target.forward*_offset.z+Vector3.up*_offset.y+transform.right*_offset.x;
		transform.forward=_target.forward;
		_targetRot=transform.rotation;
		transform.rotation=_rotation;
		_timer=0f;
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

		if(_timer<_animDur){
			_timer+=Time.deltaTime;
			if(_timer>=_animDur){
				_position=_targetPos;
				_rotation=_targetRot;
			}
			else{
				float frac=_timer/_animDur;
				frac=_curve.Evaluate(frac);
				_position=Vector3.Lerp(_startPos,_targetPos,frac);
				_rotation=Quaternion.Slerp(_startRot,_targetRot,frac);
			}
		}

		transform.position=_position;
		transform.rotation=_rotation;

    }

}
