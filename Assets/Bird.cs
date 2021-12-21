using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour
{
	Transform _player;
	public float _triggerRadius;
	int _state;
	Hop _hop;
	RunAway _runAway;
	Animator _anim;
	AudioSource _ruffleAudio;
	AudioSource _callAudio;
	public bool _playerControlled;
	MCamera _mCam;
	public Bird _mate;

	void Awake(){
		//get references
		_hop=GetComponent<Hop>();
		_runAway=GetComponent<RunAway>();
		_anim=GetComponent<Animator>();
		_ruffleAudio=transform.Find("Ruffle").GetComponent<AudioSource>();
		_callAudio=transform.Find("Call").GetComponent<AudioSource>();
		_mCam=FindObjectOfType<MCamera>();

		//disable things
		_hop.enabled=false;

		/*#temp - may not always want to start with runAway
		if(!_playerControlled){
			_runAway.enabled=false;
		}
		*/
		_state=0;
	}

    // Start is called before the first frame update
    void Start()
    {
		_player=MCamera._player;
    }

    // Update is called once per frame
    void Update()
    {
		if(_playerControlled){
			//player update
			switch(_state){
				case 0://chilling
				default:
					if(_mCam.GetControllerInput().sqrMagnitude>0)
					{
						StartHopping();
					}
					else if(Input.GetButtonDown("Sing")){
						Call();
					}
					break;
				case 1://hopping
					if(!_hop.IsHopping()&&_mCam.GetControllerInput().sqrMagnitude<=0){
						_state=0;
						_hop.enabled=false;
					}
					else if(Input.GetButtonDown("Sing")){
						_hop.FinishCurrentHop();
						Call();
					}
					break;
				case 2://ruffling
					break;
				case 3://singing
					if(!_callAudio.isPlaying){
						_state=0;
						_mate.Call();
					}
					break;

			}
		}
    }

	public bool IsPlayerClose(){
		return (_player.position-transform.position).sqrMagnitude<_triggerRadius*_triggerRadius;
	}

	public float Ruffle(){
		_anim.SetTrigger("ruffle");
		_state=2;
		_ruffleAudio.Play();
		return _ruffleAudio.clip.length;
	}

	public float Call(){
		_anim.SetTrigger("sing");
		_callAudio.Play();
		_state=3;
		return _callAudio.clip.length;
	}

	public void HopTo(Vector3 loc){
		_hop.enabled=true;
		_hop.HopTo(loc);
		_state=1;
	}

	void StartHopping(){
		_state=1;
		_hop.enabled=true;
	}

	public void StopHopping(){
		_hop.StopHopping();
		_hop.enabled=false;
		_state=0;
	}

	public bool Arrived(){
		return _hop.Arrived(_triggerRadius);
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.magenta;
		Gizmos.DrawWireSphere(transform.position,_triggerRadius);
	}
}
