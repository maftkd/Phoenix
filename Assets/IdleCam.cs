using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleCam : MonoBehaviour
{

	int _lerpState;
	float _yOffset;
	Bird _player;
	Vector3 _restPos;

	[Header("Vertical lerp")]
	public float _vertAmount;
	public float _vDelay;
	public float _vDur;
	public AnimationCurve _vertCurve;

	[Header("Lower lerp")]
	public float _lDelay;
	public float _lDur;

	[Header("Behind Player lerp")]
	public float _distance;
	public AnimationCurve _plCurve;
	public float _plDur;
	public float _plDelay;

	[Header("Side lerp")]
	public float _sideDelay;
	public float _sideDur;
	public float _sideOffset;

	void Awake(){
		enabled=false;
		_player=GameManager._player;
		_yOffset=transform.position.y-_player.transform.position.y;
	}

	void OnEnable(){
		_restPos=transform.position;
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
				StartCoroutine(LerpToBehindPlayer());
				break;
			case 1:
				StartCoroutine(LerpToSide());
				break;
			case 2:
				break;
			case 3:
				break;
			default:
				break;
		}
	}

	IEnumerator LerpAbovePlayer(){
		Vector3 startPos=transform.position;
		Vector3 targetPos=startPos+Vector3.up*_vertAmount;

		yield return new WaitForSeconds(_vDelay);

		float timer=0;
		float dur = _vDur;
		while(timer<dur){
			timer+=Time.deltaTime;
			float frac = _vertCurve.Evaluate(timer/dur);
			transform.position=Vector3.Lerp(startPos,targetPos,frac);
			yield return null;
		}
		transform.position=targetPos;

		StartLerp();
	}

	IEnumerator LerpToRestPos(){
		Vector3 startPos=transform.position;
		Vector3 targetPos=_restPos;

		yield return new WaitForSeconds(_lDelay);
		float timer=0;
		float dur = _lDur;
		while(timer<dur){
			timer+=Time.deltaTime;
			float frac = _vertCurve.Evaluate(timer/dur);
			transform.position=Vector3.Lerp(startPos,targetPos,frac);
			yield return null;
		}
		transform.position=targetPos;

		StartLerp();
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

		_restPos=transform.position;


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

		StartLerp();

	}

}
