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
	public float _controllerZero;
	[Header("Footprints")]
	public Transform _footprint;
	public Vector3 _footprintOffset;
	int _leftRightPrint=1;
	[Header("Explosion")]
	public float _afterDiveDelay;
	float _afterDiveTimer;
	public Transform _explodeParts;
	public float _divePartsDelay;
	[Header("Collisions")]
	public float _shakeDelay;
	float _shakeTimer;
	ParticleSystem _starParts;
	public float _waddleKnockVolume;
	public float _hopKnockVolume;
	Vector3 _prevPos;

	void Awake(){
		//calculations
		SkinnedMeshRenderer smr = transform.GetComponentInChildren<SkinnedMeshRenderer>();
		_size=smr.sharedMesh.bounds.size*smr.transform.localScale.x*transform.localScale.x;

		//get references
		_hop=GetComponent<Hop>();
		_fly=GetComponent<Fly>();
		_waddle=GetComponent<Waddle>();
		_runAway=GetComponent<RunAway>();
		_anim=GetComponent<Animator>();
		_ruffleAudio=transform.Find("Ruffle").GetComponent<AudioSource>();
		_callAudio=transform.Find("Call").GetComponent<AudioSource>();
		_callParts=_callAudio.GetComponent<ParticleSystem>();
		_starParts=transform.Find("StarParts").GetComponent<ParticleSystem>();

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
					if(Input.GetButton("Jump"))
						StartHopping();
					else if(_mCam.GetControllerInput().sqrMagnitude>_controllerZero*_controllerZero)
						StartWaddling();
					if(Input.GetButtonDown("Sing")){
						Call();
					}
					/*
					if(Input.GetButtonDown("Jump")){
						//Debug.Log("Jump time");
						Fly();
					}
					*/

					break;
				case 1://waddling
					if(!_waddle.IsWaddling()){
						_state=0;
						_waddle.enabled=false;
						_anim.SetFloat("walkSpeed",0f);
					}
					else if(_waddle.IsKnockBack()){
						//just wait
					}
					else if(Input.GetButton("Jump")){
						StartHopping();
					}
					if(Input.GetButtonDown("Sing")){
						Call();
					}
					/*
					if(Input.GetButtonDown("Jump")){
						//Debug.Log("Jump time");
						Fly();
					}
					*/
					break;
				case 2://hopping
					if(!_hop.IsHopping()){
						Debug.Log("Hm?");
						if(_mCam.GetControllerInput().sqrMagnitude<=_controllerZero*_controllerZero){
							//go to idle
							_state=0;
							_hop.enabled=false;
							_anim.SetFloat("walkSpeed",0f);
						}
						else{
							StartWaddling();
						}
					}
					if(Input.GetButtonDown("Sing")){
						Call();
					}
					/*
					if(Input.GetButtonDown("Jump")){
						Fly();
					}
					*/
					break;
				case 3://flying
					if(Input.GetButtonDown("Sing")){
						Call();
					}
					break;
				case 4://after dive
					_afterDiveTimer+=Time.deltaTime;
					if(_afterDiveTimer>_afterDiveDelay)
						_state=0;
					break;
				case 5://shake it off
					_shakeTimer+=Time.deltaTime;
					if(_shakeTimer>=_shakeDelay){
						_state=0;
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
		_prevPos=transform.position;
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

	public void StartHopping(){
		_state=2;
		_hop.enabled=true;
		_waddle.enabled=false;
	}

	void StartWaddling(){
		_state=1;
		_waddle.enabled=true;
		_hop.enabled=false;
	}

	public bool IsHopping(){
		return _hop.enabled&&_hop.IsHopping();
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
	public void Land(){
		_fly.enabled=false;
		_state=0;
		_anim.SetFloat("walkSpeed",0f);
		_anim.SetTrigger("land");
		_hop.PlayStepParticles();
	}

	public void Dive(float vel){
		_fly.enabled=false;
		_hop.enabled=false;
		_state=4;
		//reset pitch
		Vector3 eulerAngles=transform.eulerAngles;
		float targetPitch=0;
		eulerAngles.x=targetPitch;
		transform.eulerAngles=eulerAngles;
		//polish
		StartCoroutine(PlayExplodeParticlesR(vel));
		_anim.SetFloat("walkSpeed",0f);
		_anim.SetTrigger("land");
		_afterDiveTimer=0;
	}

	public void MakeFootprint(float offset=0){
		Transform fp = Instantiate(_footprint,transform.position,Quaternion.identity);
		//orientate
		fp.forward=Vector3.down;
		//offset footprint
		fp.position+=Vector3.up*_footprintOffset.y+transform.right*_footprintOffset.x*_leftRightPrint;
		fp.position+=offset*transform.forward;
		_leftRightPrint*=-1;
	}

	public void EquipFeather(Transform t){
		SkinnedMeshRenderer smr = transform.GetComponentInChildren<SkinnedMeshRenderer>();
		Material[] mats = smr.materials;
		mats[1]=t.GetComponent<MeshRenderer>().materials[1];
		smr.materials=mats;
	}

	IEnumerator PlayExplodeParticlesR(float vel){
		Instantiate(_explodeParts,transform.position,Quaternion.identity);
		yield return new WaitForSeconds(_divePartsDelay);
		_mCam.Shake(vel);
	}

	public void KnockBack(CollisionHelper ch, Vector3 dir){
		//add particel to collision
		Transform fp = Instantiate(_footprint,ch._hitPoint,Quaternion.identity);
		//orientate
		fp.forward=-dir;
		//offset footprint
		float vertMult = _state==3?0.4f : 0.9f;
		fp.position+=dir*_footprintOffset.y*0.1f+Vector3.up*_size.y*vertMult;
	

		_starParts.Play();
		switch(_state){
			case 0:
			default:
			case 1://waddling
				ch.Sound(_waddleKnockVolume);
				_waddle.KnockBack(dir);
				break;
			case 2://hopping
				ch.Sound(_hopKnockVolume);
				_hop.KnockBack(dir);
				break;
			case 3://flying
				ch.Sound(_hopKnockVolume);
				_fly.KnockBack(dir);
				break;
		}
	}

	public void ShakeItOff(){
		_waddle.enabled=false;
		_hop.enabled=false;
		_fly.enabled=false;
		Debug.Log("Shaking it off");
		_anim.SetFloat("walkSpeed",0f);
		_shakeTimer=0;
		_state=5;
	}

	public void RevertToPreviousPosition(){
		//move back to previous frame, to prevent collider from getting stuck overlapping
		transform.position=_prevPos;
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.magenta;
		Gizmos.DrawWireSphere(transform.position,_triggerRadius);
	}
}
