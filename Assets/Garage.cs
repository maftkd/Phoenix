using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Garage : MonoBehaviour
{
	bool _powered;
	float _openAngle;
	public float _openRate;
	public float _minOpenAngle;
	AudioSource _audio;
	public float _audioLerp;
	Transform _door;
	public AudioClip _closeClip;

	void Awake(){
		_audio=GetComponent<AudioSource>();
		_door=transform.GetChild(0);
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if(_powered){
			float deg=Time.deltaTime*_openRate;
			_door.Rotate(Vector3.right*deg);
			_openAngle+=deg;
			if(_openAngle>_minOpenAngle){
				enabled=false;
				_audio.Stop();
				Sfx.PlayOneShot3D(_closeClip,transform.position);
				return;
			}
			if(!_audio.isPlaying)
				_audio.Play();
			_audio.volume=Mathf.Lerp(_audio.volume,1f,_audioLerp*Time.deltaTime);
		}
		else{
			_audio.volume=Mathf.Lerp(_audio.volume,0,_audioLerp*Time.deltaTime*3f);
			if(_audio.volume<0.0001f)
				_audio.Stop();

		}
    }

	public void Powered(bool p)
	{
		_powered=p;
	}
}
