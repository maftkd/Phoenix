using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Temptress : MonoBehaviour
{
	public Transform [] _temptSpots;
	AudioSource _chirp;
	AudioSource _flap;
	int _state;
	float _chirpTimer;
	Transform _mainCam;
	int _curSpot;
	Vector3 _flyTarget;
	Vector3 _startPos;
	float _t;
	Animator _anim;
	public float _flyTime;
	public float _reposRange;
	public Transform _portal;
    // Start is called before the first frame update
    void Start()
    {
		_chirp = transform.GetChild(2).GetComponent<AudioSource>();
		_flap = transform.GetChild(3).GetComponent<AudioSource>();
		_mainCam = Camera.main.transform;
		_anim = GetComponent<Animator>();
		transform.position = _temptSpots[0].position;
    }

    // Update is called once per frame
    void Update()
    {
		switch(_state){
			case 0:
			default:
				_chirpTimer+=Time.deltaTime;
				if(_chirpTimer>_chirp.clip.length*2)
				{
					_chirp.Play();
					_chirpTimer=0;
				}
				if((_mainCam.position-transform.position).sqrMagnitude<_reposRange*_reposRange)
				{
					_curSpot++;
					if(_curSpot<_temptSpots.Length){
						_flap.Play();
						_state=1;
						_flyTarget = _temptSpots[_curSpot].position;
						_startPos=transform.position;
						_anim.SetBool("flying",true);
						transform.LookAt(_flyTarget);
						_t=0;
					}
					else{
						Debug.Log("Ahhhh");
						_mainCam.position=_portal.position;
						Walk._instance.enabled=false;
						_state=2;
						_portal.GetComponent<AudioSource>().Play();
					}
				}
				break;
			case 1:
				_t+=Time.deltaTime;
				transform.position=Vector3.Lerp(_startPos,_flyTarget,_t/_flyTime);
				if(_t>=_flyTime){
					_state=0;
					_flap.Play();
					_anim.SetBool("flying",false);
				}
				break;
			case 2:
				if(_mainCam.position.y>-35f)
					_mainCam.position+=Vector3.down*Time.deltaTime*4f;
				else
					_state=3;
				break;
			case 3:
				break;
		}
    }

	void OnDrawGizmos(){
		if(_temptSpots!=null){
			foreach(Transform t in _temptSpots){
				Gizmos.color=Color.blue;
				Gizmos.DrawWireSphere(t.position,_reposRange);

			}
		}
	}
}
