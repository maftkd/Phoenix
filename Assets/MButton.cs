using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MButton : MonoBehaviour
{
	public Material _lit;
	public Material _unlit;
	MeshRenderer _renderer;
	int _state;
	Vector3 _restPos;
	Vector3 _pushedPos;
	public float _pushOffset;
	public float _pushDur;
	float _pushTimer;
	public AudioClip _pushAudio;
	public float _pushPitch;
	public float _releasePitch;
	public UnityEvent _onButtonPressed;

	void Awake(){
		_restPos=transform.localPosition;
		_pushedPos=_restPos-Vector3.forward*_pushOffset;
		_renderer=GetComponent<MeshRenderer>();
	}
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
		switch(_state){
			case 0:
			default://unpressed
				break;
			case 1://pressing
				_pushTimer+=Time.deltaTime;
				if(_pushTimer>=_pushDur){
					_state=2;
					_renderer.material=_lit;
					_onButtonPressed.Invoke();
				}
				break;
			case 2://pressed
				break;
			case 3://releasing
				_pushTimer+=Time.deltaTime;
				if(_pushTimer>=_pushDur){
					_state=0;
					_renderer.material=_unlit;
				}
				break;
		}
    }

	void OnTriggerEnter(Collider other){
		Debug.Log("Entered button!");
		switch(_state){
			case 0:
			default://unpressed
				_state=1;
				transform.localPosition=_pushedPos;
				_pushTimer=0;
				Sfx.PlayOneShot2D(_pushAudio,_pushPitch);
				break;
			case 1://pressing
				break;
			case 2://pressed
				_state=3;
				transform.localPosition=_restPos;
				_pushTimer=0;
				Sfx.PlayOneShot2D(_pushAudio,_releasePitch);
				break;
			case 3://releasing
				break;
		}
	}

	public bool IsPressed(){
		return _state==2;
	}
}
