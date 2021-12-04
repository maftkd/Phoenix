using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mailbox : MonoBehaviour
{
	Transform _lid;
	Quaternion _closedRot;
	Quaternion _openRot;
	public AudioClip _lidOpen;
	public AudioClip _lidClose;
	Transform _mainCam;
	public float _openRadius;
	bool _open;
	AudioSource _lidAudio;
    // Start is called before the first frame update
    void Start()
    {
		_lid=transform.GetChild(0);
		_lidAudio=_lid.GetComponent<AudioSource>();
		_closedRot=_lid.rotation;
		_lid.localEulerAngles=Vector3.zero;
		_openRot=_lid.rotation;
		_lid.rotation=_closedRot;
		_mainCam=Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
		if((_mainCam.position-transform.position).sqrMagnitude<_openRadius*_openRadius){
			if(!_open){
				_lid.rotation=_openRot;
				_open=true;
				_lidAudio.clip=_lidOpen;
				_lidAudio.Play();
			}
		}
		else{
			if(_open){
				_lid.rotation=_closedRot;
				_open=false;
				_lidAudio.clip=_lidClose;
				_lidAudio.Play();
			}
		}
    }

	void OnDrawGizmos(){
		Gizmos.color=Color.red;
		Gizmos.DrawWireSphere(transform.position,_openRadius);
	}
}
