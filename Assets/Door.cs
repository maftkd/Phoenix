using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
	public float _openDelay;
	public float _openTime;
	public float _openAngle;
	public AudioClip _openSound;
	public AudioClip _closeSound;
	public AnimationCurve _openCurve;
	Quaternion _leftClosed;
	Quaternion _rightClosed;
	Quaternion _leftOpened;
	Quaternion _rightOpened;
	Transform _right;
	Transform _left;
	Material _leftMat;
	Material _rightMat;
	GameObject _entranceTrigger;
	bool _open;
	Bird _player;
	BirdHouse _bh;

	void Awake(){
		_player=GameManager._player;
		_right=transform.Find("DoorRight");
		_left=transform.Find("DoorLeft");
		_leftClosed=_left.rotation;
		_rightClosed=_right.rotation;
		_left.Rotate(Vector3.up*_openAngle);
		_leftOpened=_left.rotation;
		_right.Rotate(-Vector3.up*_openAngle);
		_rightOpened=_right.rotation;
		_left.rotation=_leftClosed;
		_right.rotation=_rightClosed;
		_leftMat=_left.GetComponent<Renderer>().material;
		_rightMat=_right.GetComponent<Renderer>().material;
		_entranceTrigger=transform.Find("EntranceTrigger").gameObject;
		_entranceTrigger.SetActive(false);
		_bh=transform.GetComponentInParent<BirdHouse>();
	}

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void Open(){
		if(_player._state>=2||_open)
			return;
		if(!_bh._activated)
			return;
		StartCoroutine(OpenDoors(1));
		Debug.Log("Open doors");
	}

	public void Close(){
		if(!_open||_player._state==7)
			return;
		StartCoroutine(OpenDoors(-1));
		Debug.Log("Close doors");
	}

	IEnumerator OpenDoors(float dir){
		_open=dir>0;
		//_rightMat.SetFloat("_Lerp",dir>0?1:0);
		//_leftMat.SetFloat("_Lerp",dir>0?1:0);

		yield return null;
		if(dir>0)
			Sfx.PlayOneShot3D(_openSound,_left.position);
		else
			Sfx.PlayOneShot3D(_closeSound,_left.position);
		_entranceTrigger.SetActive(true);

		yield return new WaitForSeconds(_openDelay);
		float timer=0;
		float dur=_openTime;
		
		Quaternion leftStartRot=_left.rotation;
		Quaternion leftEndRot=_open? _leftOpened : _leftClosed;
		Quaternion rightStartRot=_right.rotation;
		Quaternion rightEndRot=_open? _rightOpened : _rightClosed;

		while(timer<dur){
			timer+=Time.deltaTime;
			float frac=_openCurve.Evaluate(timer/dur);
			_left.rotation=Quaternion.Slerp(leftStartRot,leftEndRot,frac);
			_right.rotation=Quaternion.Slerp(rightStartRot,rightEndRot,frac);
			yield return null;
		}
	}
}
