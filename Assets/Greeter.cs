using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Greeter : MonoBehaviour
{
	Animator _anim;
	int _state;
	AudioSource _audio;
	public AudioClip _clonk;
	public AudioClip _sing;
	float _peckTime;
	float _peckTimer=0;
	public int _pecksToWake;
	int _peckCount;
	float _singTimer;
	float _singTime;
	int _singCount;
	public float _scootBackAmount;
	Dialog _dialog;
	int _dialogCounter;
    // Start is called before the first frame update
    void Start()
    {
		_anim = GetComponent<Animator>();
		AnimationClip[] clips = _anim.runtimeAnimatorController.animationClips;
        foreach(AnimationClip clip in clips)
        {
            switch(clip.name)
            {
                case "sing":
					//_singTime=clip.length;
                    break;
                case "peck":
                    _peckTime=clip.length;
                    break;
				default:
					break;
            }
        }
		_singTime=_sing.length;
		_audio = GetComponent<AudioSource>();
		_anim.SetTrigger("peck");
		_dialog=transform.GetComponentInChildren<Dialog>();
		Hop._instance.enabled=false;
    }

    // Update is called once per frame
    void Update()
    {
		switch(_state){
			case 0:
			default:
				_peckTimer+=Time.deltaTime;
				if(_peckTimer>_peckTime){
					_peckCount++;
					if(_peckCount>=_pecksToWake){
						_state=1;
						_anim.SetTrigger("sing");
						_audio.clip=_sing;
						_audio.Play();
						_singCount++;
					}
					else{
						_peckTimer=0;
						_anim.SetTrigger("peck");
						_audio.clip=_clonk;
						_audio.Play();
					}
				}
				CheckForWake();
				break;
			case 1:
				_singTimer+=Time.deltaTime;
				if(_singTimer>=_singTime){
					_peckTimer=0;
					_state=0;
					_peckCount=0;
					_singTimer=0;
				}
				CheckForWake();
				break;
			case 2:
				//hello friend
				if(Input.anyKeyDown){
					_dialogCounter++;
					switch(_dialogCounter){
						case 0:
						default:
							break;
						case 1:
							_dialog.ShowText("[insert story text here]");
							break;
						case 2:
							_dialog.ShowText("I guess you're not used to your new legs yet");
							break;
						case 3:
							_dialog.ShowText("Try walking over yonder, and bring me back a tasty morsel");
							break;
						case 4:
							Hop._instance.enabled=true;
							_state=3;
							break;
					}
				}
				break;
			case 3:
				Debug.Log("I'm hunger");
				break;
		}
    }

	void CheckForWake(){
		if(_singCount>=1){
			if(Input.anyKeyDown)
			{
				//check for wake up
				Hop._instance.enabled=true;
				Hop._instance.ResetPosRot();
				Hop._instance.enabled=false;
				transform.position+=Vector3.forward*_scootBackAmount;
				_state=2;
				_dialog.ShowText("Hello Friend, that was quite a fall you took there");
			}
		}
	}
}
