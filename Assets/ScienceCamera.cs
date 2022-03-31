using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScienceCamera : MonoBehaviour
{
	Transform _camera;
	Bird _player;
	public float _radius;
	bool _inZone;
	public AudioClip _zoom;
	public AudioClip _rewind;
	public float _slerp;
	public float _minDotToFocus;
	int _recordState;
	bool _recording;
	public float _requiredFootage;
	float _footage;
	Material _footageMat;
	Material _blinkMat;
	Transform _boundary;

	void Awake(){
		_camera=transform.Find("camera");
		_player=GameManager._player;
		_footageMat=_camera.Find("Footage").GetComponent<Renderer>().material;
		_footageMat.SetFloat("_FillAmount",0);
		_blinkMat=_camera.GetChild(0).GetComponent<Renderer>().material;
		_blinkMat.SetFloat("_Blink",1);
		_boundary=_camera.Find("Boundary");
	}

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
		switch(_recordState){
			case 0:
				_inZone=(_player.transform.position-_camera.position).sqrMagnitude<_radius*_radius;
				if(_inZone){
					_recordState=1;
					Sfx.PlayOneShot3D(_zoom,transform.position,1f);
				}
				break;
			case 1:
				_inZone=(_player.transform.position-_camera.position).sqrMagnitude<_radius*_radius;
				if(!_inZone){
					_recordState=0;
				}
				else{
					Quaternion curRot=_camera.rotation;
					_camera.LookAt(_player.transform);
					_camera.rotation=Quaternion.Slerp(curRot,_camera.rotation,_slerp*Time.deltaTime);
					_footage+=Time.deltaTime;
					_footageMat.SetFloat("_FillAmount",_footage/_requiredFootage);
					if(_footage>=_requiredFootage)
					{
						_recordState=2;
						Sfx.PlayOneShot3D(_rewind,transform.position,2f);
						_footageMat.SetColor("_FillColor",Color.green);
						_blinkMat.SetFloat("_Blink",0);
						StartCoroutine(DestroyBoundary());
					}
				}
				break;
			case 2:
				//done
				break;
			default:
				break;
		}
		if(_inZone){

			Vector3 diff=_player.transform.position-_camera.position;
			float dt=Vector3.Dot(diff.normalized,_camera.forward);
		}
    }

	IEnumerator DestroyBoundary(){
		float timer=0;
		float dur=1;
		Vector3 startScale=_boundary.localScale;
		Vector3 endScale=Vector3.zero;
		while(timer<dur){
			timer+=Time.deltaTime;
			_boundary.localScale=Vector3.Lerp(startScale,endScale,timer/dur);
			yield return null;
		}
		Destroy(_boundary.gameObject);
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.red;
		Gizmos.DrawWireSphere(transform.position,_radius);
	}

}
