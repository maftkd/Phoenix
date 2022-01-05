﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lock : MonoBehaviour
{
	Bird _bird;
	public int _id;
	bool _opening;
	MCamera _mCam;
	public Transform _keyStart;
	public Transform _keyEnd;
	public Transform _keyStartRot;
	public Transform _keyEndRot;
	public ToolPath _toolPath;
	Transform _key;
	Transform _cylinder;
	[Header("Audio")]
	public AudioClip _pin;
	public AudioSource _click;
	public AudioSource _turn;
	float _prevPos;
	public float[] _pinPositions;
	public Vector2 _pinPitchRange;
	public float _turnVolMult;
	public float _turnPitchMult;

	void Awake(){
		GameObject player =GameObject.FindGameObjectWithTag("Player");
		_bird=player.GetComponent<Bird>();
		_mCam=Camera.main.GetComponent<MCamera>();
		_cylinder=transform.GetChild(0);
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if(_opening){
			float pos = _toolPath.GetPositionF();
			if(pos<1f){
				_key.position=Vector3.Lerp(_keyStart.position,_keyEnd.position,pos);
				if(_turn.isPlaying){
					_turn.Stop();
				}
			}
			else{
				_cylinder.rotation=Quaternion.Slerp(_keyStartRot.rotation,_keyEndRot.rotation,pos-1f);
				if(!_turn.isPlaying){
					_turn.Play();
				}
				_turn.volume=Mathf.Abs(pos-_prevPos)*_turnVolMult;
				_turn.pitch=1f+Mathf.Sign(pos-_prevPos)*_turnPitchMult;

				//if turn audio is not playing
				//	play it
				//set volume according to delta
				//set pitch according to dir
			}
			foreach(float f in _pinPositions){
				if(pos>f&&_prevPos<f){
					//pin up
					Sfx.PlayOneShot2D(_pin,1f+Random.Range(_pinPitchRange.x,_pinPitchRange.y));
				}
				else if(pos<f&&_prevPos>f){
					//pin down
					Sfx.PlayOneShot2D(_pin,0.5f+Random.Range(_pinPitchRange.x,_pinPitchRange.y));
				}
			}
			if(pos>=1f&&_prevPos<1f){
				//Sfx.PlayOneShot2D(_pin,1f+Random.Range(_pinPitchRange.x,_pinPitchRange.y));
				//Sfx.PlayOneShot2D(_click);
				if(!_click.isPlaying)
					_click.Play();
			}
			_prevPos=pos;
		}
    }

	public void CheckLock(){
		if(_opening)
			return;
		_key = _bird.GetKey();
		if(_key!=null){
			int id = _key.GetComponent<Key>().GetId();
			if(id==_id){
				Debug.Log("Got a match");
				//StartCoroutine(OpenLock(key));
				_opening=true;
				_mCam.MoveToTransform(transform.Find("CamTarget"));
				MoveKeyToStartPos();
			}
			else{
				Debug.Log("Mismatch");
			}
		}
		else{
			Debug.Log("ignore");
		}
	}

	void MoveKeyToStartPos(){
		_key.SetParent(_cylinder);
		_key.position=_keyStart.position;
		_key.localEulerAngles = new Vector3(-90f,-90f,0f);
		_key.localScale=Vector3.one;
		//give bird control of tool
		_bird.UseKey(_key,_toolPath);
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.green;
		Gizmos.DrawWireSphere(_keyStart.position,0.02f);
		Gizmos.color=Color.red;
		Gizmos.DrawWireSphere(_keyEnd.position,0.02f);
	}
}
