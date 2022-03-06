using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleCam : Shot
{
	public Transform _center;
	public Transform _forceField;
	float _forceFieldRadius;
	bool _inZone;
	bool _prevInZone;
	MCamera _mCam;
	bool _tracking;
	public float _shotRadius;
	public float _shotHeight;
	public float _zoneHeight;
	Vector3 _targetPos;
	Quaternion _targetRot;
	public float _lerp;

	protected override void Awake(){
		base.Awake();
		_player=GameManager._player;
		_forceFieldRadius=_forceField.localScale.x*0.5f;
		_mCam=GameManager._mCam;
	}

    // Start is called before the first frame update
    protected override void Start()
    {
		base.Awake();
    }

    // Update is called once per frame
    protected override void Update()
    {
		base.Update();
		
		//zone check
		float sqrDist = (_player.transform.position-_center.position).sqrMagnitude;
		_inZone=sqrDist<_forceFieldRadius*_forceFieldRadius
			&&_player.transform.position.y<_center.position.y+_zoneHeight;
		//#todo - also do a vertical check for in zone. This stuff should kinda disable if the bird is flying or standing on top the box

		if(_inZone!=_prevInZone){
			if(_inZone)
			{
				//_mCam.Transition(_cam,MCamera.Transitions.FADE,0f,null,0.25f);
				//_mCam.Transition(_cam,MCamera.Transitions.CUT_BACK);
			}
			else
				_player.TransitionToRelevantCamera();
			_tracking=_inZone;
		}

		if(_tracking){
			Vector3 centerPos=_center.position;//+Vector3.up*_targetYOffset;
			centerPos.y=_player.transform.position.y;
			Vector3 diff=(centerPos-_player.transform.position).normalized;
			_targetPos=_center.position-diff*_shotRadius;
			_targetPos.y=centerPos.y+_shotHeight;
			Quaternion curRot=transform.rotation;
			Vector3 curPos=transform.position;
			transform.position=_targetPos;
			transform.LookAt(centerPos);
			_targetRot=transform.rotation;


			//lerp
			transform.position=Vector3.Lerp(curPos,_targetPos,_lerp*Time.deltaTime);
			transform.rotation=Quaternion.Slerp(curRot,_targetRot,_lerp*Time.deltaTime);
		}

		_prevInZone=_inZone;
    }

	public void ResetZone(){
		_inZone=false;
		_prevInZone=false;
	}
}
