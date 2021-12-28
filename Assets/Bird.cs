using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bird : MonoBehaviour
{
	Transform _player;
	public float _triggerRadius;
	int _state;
	Hop _hop;
	Fly _fly;
	Waddle _waddle;
	RunAway _runAway;
	Animator _anim;
	AudioSource _ruffleAudio;
	AudioSource _callAudio;
	public bool _playerControlled;
	MCamera _mCam;
	public Bird _mate;
	[HideInInspector]
	public Vector3 _size;
	ParticleSystem _callParts;
	public LayerMask _collisionLayer;
	public float _walkThreshold;

	void Awake(){
		//calculations
		SkinnedMeshRenderer smr = transform.GetComponentInChildren<SkinnedMeshRenderer>();
		_size=smr.sharedMesh.bounds.size;

		//get references
		_hop=GetComponent<Hop>();
		_fly=GetComponent<Fly>();
		_waddle=GetComponent<Waddle>();
		_runAway=GetComponent<RunAway>();
		_anim=GetComponent<Animator>();
		_ruffleAudio=transform.Find("Ruffle").GetComponent<AudioSource>();
		_callAudio=transform.Find("Call").GetComponent<AudioSource>();
		_callParts=_callAudio.GetComponent<ParticleSystem>();

		//disable things
		_hop.enabled=false;
		_fly.enabled=false;

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
		_mCam=FindObjectOfType<MCamera>();
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
						if(_mCam.GetControllerInput().sqrMagnitude<_walkThreshold*_walkThreshold)
							StartWaddling();
						else
							StartHopping();
					}
					if(Input.GetButtonDown("Sing")){
						Call();
					}
					if(Input.GetButtonDown("Jump")){
						//Debug.Log("Jump time");
						Fly();
					}

					break;
				case 1://waddling
					if(_mCam.GetControllerInput().sqrMagnitude<=0){
						_state=0;
						_waddle.enabled=false;
						_anim.SetFloat("walkSpeed",0f);
					}
					else if(_mCam.GetControllerInput().sqrMagnitude>=_walkThreshold*_walkThreshold){
						//Debug.Log(_mCam.GetControllerInput().magnitude);
						StartHopping();
					}
					if(Input.GetButtonDown("Sing")){
						Call();
					}
					if(Input.GetButtonDown("Jump")){
						//Debug.Log("Jump time");
						Fly();
					}
					break;
				case 2://hopping
					if(!_hop.IsHopping()&&_mCam.GetControllerInput().sqrMagnitude<=0){
						_state=0;
						_hop.enabled=false;
						_anim.SetFloat("walkSpeed",0f);
					}
					if(!_hop.IsHopping()&&_mCam.GetControllerInput().sqrMagnitude<=_walkThreshold*_walkThreshold){
						StartWaddling();
					}
					if(Input.GetButtonDown("Sing")){
						Call();
					}
					if(Input.GetButtonDown("Jump")){
						//Debug.Log("Jump time");
						Fly();
					}
					break;
				case 3://flying
					if(Input.GetButtonDown("Sing")){
						Call();
					}
					break;
			}

			if(Input.GetButtonDown("Pause"))
			{
				SceneManager.LoadScene(SceneManager.GetActiveScene().name);
				/*
#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
#else
				Application.Quit();
#endif
*/
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

	public void Call(){
		//StartCoroutine(SpawnSoundRings(_numSoundRings));
		//_anim.SetTrigger("sing");
		_callAudio.Play();
		_callParts.Play();
		//_state=3;
		if(_playerControlled)
			StartCoroutine(CallMateR());
	}

	IEnumerator CallMateR(){
		float dur = _callAudio.clip.length;
		yield return new WaitForSeconds(dur);
		_mate.Call();
	}

	public void HopTo(Vector3 loc){
		_hop.enabled=true;
		_hop.HopTo(loc);
		_state=2;
	}

	void StartHopping(){
		_state=2;
		_hop.enabled=true;
		_waddle.enabled=false;
	}

	void StartWaddling(){
		_state=1;
		_waddle.enabled=true;
		_hop.enabled=false;
	}

	public void StopHopping(){
		_hop.StopHopping();
		_hop.enabled=false;
		_anim.SetFloat("walkSpeed",0f);
		_state=0;
	}

	public bool Arrived(){
		return _hop.Arrived(_triggerRadius);
	}

	void Fly(){
		//disable waddle
		_waddle.enabled=false;
		//disable hop
		_hop.enabled=false;
		//enable flight
		_fly.enabled=true;
		_state=3;
		_anim.SetTrigger("fly");
	}
	public void Land(float vel){
		_fly.enabled=false;
		_state=0;
		_anim.SetTrigger("land");
		_hop.PlayStepParticles();
		_mCam.Shake(vel);
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.magenta;
		Gizmos.DrawWireSphere(transform.position,_triggerRadius);
	}
}
