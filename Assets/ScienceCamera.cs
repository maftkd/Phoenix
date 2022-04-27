using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScienceCamera : MonoBehaviour
{

	public AudioClip _shutter;
	public AudioClip _flashClip;
	public Camera _cam;
	MCamera _mCam;
	public float _flashDur;
	public Transform _photo;
	Transform _printer;
	public float _printDelay;
	public AudioClip _printSound;
	public float _printDur;
	public float _printDist;

	void Awake(){
		_cam.enabled=true;
		_mCam=GameManager._mCam;
		_printer=transform.parent.Find("Printer");
	}

	public void ChargeFlash(){
		Sfx.PlayOneShot3D(_flashClip,transform.position);
	}

	public void TakePhoto(){
		Sfx.PlayOneShot3D(_shutter,transform.position);
		StartCoroutine(Flash());
		StartCoroutine(Print());
	}

	IEnumerator Flash(){
		_mCam.SetFlash(1f);
		yield return null;
		float timer=_flashDur;
		while(timer>0){
			timer-=Time.deltaTime;
			float frac=timer/_flashDur;
			_mCam.SetFlash(frac);
			yield return null;
		}
		_mCam.SetFlash(0f);
	}

	IEnumerator Print(){
		yield return new WaitForSeconds(_printDelay);
		Sfx.PlayOneShot3D(_printSound,transform.position);
		Transform photo = Instantiate(_photo,_printer.position,Quaternion.identity);
		photo.Rotate(Vector3.right,90f);
		yield return null;
		float dur = _printDur;
		float timer = 0;
		Vector3 startPos=photo.position;
		Vector3 endPos=startPos+transform.forward*_printDist;
		while(timer<dur){
			timer+=Time.deltaTime;
			float frac=timer/dur;
			photo.position=Vector3.Lerp(startPos,endPos,frac);
			yield return null;
		}
		photo.position=endPos;
	}

}
