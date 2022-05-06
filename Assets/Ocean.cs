using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ocean : MonoBehaviour
{
	bool _wet;
	MInput _mIn;
	MCamera _mCam;

	public AudioClip _splash;
	public AudioClip _emergeSplash;
	public AudioClip _bubbles;
	public Transform _waterRings;
	public Transform _bubbleParts;
	public float _resetDur;
	public bool _drown;
	public float _breathHoldDur;
	float _breathTimer;
	Bird _player;

	void Awake(){
		_mIn=GameManager._mIn;
		_player=GameManager._player;
		_mCam=GameManager._mCam;
		_mCam.SetVignette(0);
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	void OnTriggerEnter(Collider other){
		if(_wet)
			return;
		StopAllCoroutines();
		StartCoroutine(UnderWater());
		/*
		if(_wet)
			return;
		if(other.GetComponent<Bird>()!=null){
			if(_drown)
				StartCoroutine(Drown(other.GetComponent<Bird>()));
			else{
				Sfx.PlayOneShot3D(_splash,_player.transform.position);
				Vector3 pos=_player.transform.position;
				pos.y=transform.position.y+0.01f;
				Transform rings = Instantiate(_waterRings);
				rings.position=pos;
				_player.InWater(true);
			}
		}
		*/
	}

	Transform bubbleAudio;
	IEnumerator UnderWater(){
		//play splash sound
		Sfx.PlayOneShot3D(_splash,_player.transform.position);
		bubbleAudio = Sfx.PlayOneShot3D(_bubbles,_player.transform.position).transform;
		while(_breathTimer<_breathHoldDur){
			_breathTimer+=Time.deltaTime;
			_mCam.SetVignette(_breathTimer/_breathHoldDur);
			Vector3 bubblePos=_player.transform.position;
			bubblePos.y=5f;
			bubbleAudio.position=bubblePos;
			yield return null;
		}
		_mCam.SetVignette(1);
		Debug.Log("Drowned!");
		StartCoroutine(Respawn());
	}

	void OnTriggerExit(Collider other){
		if(_wet)
			return;
		StopAllCoroutines();
		StartCoroutine(Emerge());
	}

	IEnumerator Emerge(){
		//play emerge splash
		Sfx.PlayOneShot3D(_emergeSplash,_player.transform.position);
		if(bubbleAudio!=null)
			bubbleAudio.GetComponent<AudioSource>().Stop();
		while(_breathTimer>0){
			_breathTimer-=Time.deltaTime;
			_mCam.SetVignette(_breathTimer/_breathHoldDur);
			yield return null;
		}
		_mCam.SetVignette(0);
		//start moving vignette to 0
	}

	[ContextMenu("Test")]
	public void DrownTest(){
		//StartCoroutine(Drown(GameManager._player));
	}

	IEnumerator Respawn(){
		_wet=true;
		_mIn.LockInput(true);
		_player.Drown();
		yield return new WaitForSeconds(_resetDur);
		_player.Respawn();
		_player.Ruffle();
		_player.ResetState();
		_mIn.LockInput(false);
		float timer=0;
		while(timer<_resetDur){
			timer+=Time.deltaTime;
			_mCam.SetVignette(1-timer/_resetDur);
			yield return null;
		}
		_mCam.SetVignette(0);
		_wet=false;
	}
}
