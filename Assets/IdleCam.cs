using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleCam : Shot
{

	int _lerpState;
	float _yOffset;
	Vector3 _startOffset;

	[Header("Close in")]
	public float _closeInSpeed;
	public float _minDistance;
	public float _maxDistance;

	[Header("Behind Player lerp")]
	public float _distance;
	public AnimationCurve _plCurve;
	public float _plDur;
	public float _plDelay;

	[Header("Side lerp")]
	public float _sideDelay;
	public float _sideDur;
	public float _sideOffset;

	protected override void Awake(){
		base.Awake();
		_player=GameManager._player;
		_yOffset=transform.position.y-_player.transform.position.y;
		_startOffset=transform.position-_player.transform.position;
	}

	/*
	public override void StartTracking(Transform t){
		base.StartTracking(t);
	}
	*/

	void OnEnable(){
		StartLerp(0);
	}

	void OnDisable(){
		StopAllCoroutines();
	}

	void StartLerp(int index=-1){
		if(index!=-1)
			_lerpState=index;
		else
			_lerpState++;
		switch(_lerpState){
			case 0:
				StartCoroutine(CloseInOnPlayer());
				break;
			case 1:
				StartCoroutine(LerpToBehindPlayer());
				break;
			case 2:
				StartCoroutine(LerpToSide());
				break;
			case 3:
				break;
			default:
				break;
		}
	}


	IEnumerator LerpToBehindPlayer(){
		yield return new WaitForSeconds(_plDelay);
		Vector3 startPos=transform.position;
		Vector3 targetPos=_player.transform.position-_player.transform.forward*_distance;
		targetPos.y=_player.transform.position.y+_yOffset;
		Quaternion startRot=transform.rotation;
		transform.position=targetPos;
		transform.LookAt(_player.transform);
		Quaternion targetRot=transform.rotation;

		float timer=0;
		float dur=_plDur;
		while(timer<dur){
			timer+=Time.deltaTime;
			float frac=_plCurve.Evaluate(timer/dur);
			transform.position=Vector3.Lerp(startPos,targetPos,frac);
			transform.rotation=Quaternion.Slerp(startRot,targetRot,frac);
			yield return null;
		}
		transform.position=targetPos;
		transform.rotation=targetRot;

		StartLerp();
	}

	IEnumerator LerpToSide(){
		yield return new WaitForSeconds(_sideDelay);
		Vector3 startPos=transform.position;
		Vector3 targetPos=startPos+transform.right*MRandom.RandSign()*_sideOffset;
		float timer=0;
		float dur = _sideDur;

		while(timer<dur){
			timer+=Time.deltaTime;
			float frac=_plCurve.Evaluate(timer/dur);
			transform.position=Vector3.Lerp(startPos,targetPos,frac);
			yield return null;
		}
		transform.position=targetPos;

	}

	IEnumerator CloseInOnPlayer(){
		float sqrDist = (_player.transform.position-transform.position).sqrMagnitude;
		if(sqrDist>_maxDistance*_maxDistance){
			yield return null;
			transform.position=_player.transform.position+_startOffset*_minDistance;
		}
		else{
			while(sqrDist>_minDistance*_minDistance){
				Vector3 diff = _player.transform.position-transform.position;
				transform.position+=diff.normalized*_closeInSpeed*Time.deltaTime;
				sqrDist = (_player.transform.position-transform.position).sqrMagnitude;
				yield return null;
			}
		}
		StartLerp();
	}
	
    // Update is called once per frame
    protected override void Update()
    {
		//after all is said and done
		if(HandleMouseMotion()){
			StopAllCoroutines();
			_lerpState=-1;
		}
		else if(_lerpState==-1){
			StartLerp(0);
		}
	}

}
