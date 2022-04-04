using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScienceCamera : MonoBehaviour
{
	Transform _camera;
	Bird _player;
	public AudioClip _zoom;
	public float _slerp;
	Material _blinkMat;
	Vector3 _hiddenPos;
	Vector3 _activePos;
	Quaternion _hiddenRot;
	public Vector3 _offset;
	int _state;
	public float _animDur;
	public float _hideDur;
	public AudioClip _hydraulic;

	void Awake(){
		_player=GameManager._player;
		_camera=transform.GetChild(0);
		_blinkMat=_camera.GetChild(0).GetComponent<Renderer>().material;
		_blinkMat.SetFloat("_Blink",0);
		_hiddenPos=_camera.position;
		_activePos=_hiddenPos+_camera.right*_offset.x+_camera.up*_offset.y+_camera.forward*_offset.z;
		_camera.position=_hiddenPos;
		_hiddenRot=_camera.rotation;
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
				break;
			case 1:
				Quaternion cur=_camera.rotation;
				_camera.LookAt(_player.transform);
				_camera.rotation=Quaternion.Slerp(cur,_camera.rotation,_slerp*Time.deltaTime);
				break;
			case 2:
				break;
			case 3:
				break;
			default:
				break;
		}
    }

	public void Activate(bool a){
		if(a)
			StartCoroutine(ActivateR(true));
		else
			StartCoroutine(ActivateR(false));
	}

	IEnumerator ActivateR(bool a){
		_state=a?1:0;
		_blinkMat.SetFloat("_Blink",a?1:0);
		float timer=0;
		float dur=a?_animDur:_hideDur;
		Sfx.PlayOneShot3D(_hydraulic,_camera.position,a?1.5f:2f);
		Quaternion cur=_camera.rotation;
		while(timer<dur){
			timer+=Time.deltaTime;
			float frac=timer/dur;
			if(a)
				_camera.position=Vector3.Lerp(_hiddenPos,_activePos,frac);
			else
			{
				_camera.position=Vector3.Lerp(_hiddenPos,_activePos,1-frac);
				_camera.rotation=Quaternion.Slerp(cur,_hiddenRot,frac);
			}
			yield return null;
		}
	}
}
