//#temp
//
//This is for prototyping the scanning stuff
//Right now it will just be triggered by key press
//In future ideally this is triggered by the bird landing on the table
//And also this would involve the research scientist

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhotoScanner : MonoBehaviour
{
	public Transform [] _photos;
	int _state;
	int _photoCount;
	public PressurePlate _button;
	public Renderer _progress;
	Material _progressMat;
	float _scanTimer;
	public float _scanDur;
	public Text _output;
	AudioSource _scanSound;
	MInput _mIn;

	void Awake(){
		_progressMat=_progress.material;
		_progressMat.SetFloat("_FillAmount",0);
		_scanSound=GetComponent<AudioSource>();
		_mIn=GameManager._mIn;
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
				/*
				if(_mIn.GetSingDown()&&_photoCount<_photos.Length){
					StartCoroutine(PlacePhotoOnScanner());
				}
				*/
				break;
			case 1://moving photo
				break;
			case 2://photo in place
				if(_button.IsPowered()){
					if(!_scanSound.isPlaying)
						_scanSound.Play();
					_scanTimer+=Time.deltaTime;
					_progressMat.SetFloat("_FillAmount",_scanTimer/_scanDur);
					if(_scanTimer>_scanDur){
						//done scanning
						_progressMat.SetFloat("_FillAmount",1);
						_state=3;
						_output.text=(_photoCount+1)+"/"+_photos.Length;
					}
				}
				else if(_scanSound.isPlaying)
					_scanSound.Pause();
				break;
			case 3://scanned photo in scanner
				if(!_button.IsPowered()){
					_progressMat.SetFloat("_FillAmount",0);
					Destroy(_photos[_photoCount].gameObject);
					_photoCount++;
					_state=0;
				}
				break;
			default:
				break;
		}
    }

	IEnumerator PlacePhotoOnScanner(){
		_state=1;
		Transform t = _photos[_photoCount];
		Vector3 startPos=t.position;
		Quaternion startRot=t.rotation;
		Vector3 endPos=transform.position+Vector3.up*transform.localScale.y*0.5f;
		Quaternion endRot=Quaternion.identity;

		float timer=0;
		float dur=1f;
		while(timer<dur){
			timer+=Time.deltaTime;
			float frac=timer/dur;
			t.position=Vector3.Lerp(startPos,endPos,frac);
			t.rotation=Quaternion.Slerp(startRot,endRot,frac);
			yield return null;
		}
		t.position=endPos;
		t.rotation=endRot;
		_scanTimer=0;
		_state=2;
	}
}
