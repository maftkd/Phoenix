using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Greeter : MonoBehaviour
{
	Hop _hop;
	Peck _peck;
	Animator _anim;
	int _state;
	AudioSource _audio;
	public AudioClip _clonk;
	public AudioClip _sing;
	public AudioClip _nomNom;
	public AudioClip _chalp;
	float _peckTime;
	float _peckTimer=0;
	public int _pecksToWake;
	int _peckCount;
	float _singTimer;
	float _singTime;
	float _singAnimTime;
	int _singCount;
	public float _scootBackAmount;
	Dialog _dialog;
	int _dialogCounter;
	public bool _skipFirstSong;
	public float _chatRange;
	Transform _mainCam;
	[Header("Eating")]
	public float _eyeHeight;
	public float _turnTime;
	bool _doneTurning;
	public Transform _biteParent;
	public float _biteDistance;
	float _eatTimer;
	int _eatCount;
	Transform _seed;
	public float _chuckTime;

    // Start is called before the first frame update
    void Start()
    {
		_anim = GetComponent<Animator>();
		AnimationClip[] clips = _anim.runtimeAnimatorController.animationClips;
        foreach(AnimationClip clip in clips)
        {
            switch(clip.name)
            {
                case "sing":
					_singAnimTime=clip.length;
                    break;
                case "peck":
                    _peckTime=clip.length;
                    break;
				default:
					break;
            }
        }
		_singTime=_sing.length;
		_audio = GetComponent<AudioSource>();
		_anim.SetTrigger("peck");
		_dialog=transform.GetComponentInChildren<Dialog>();
		_hop = Hop._instance;
		_hop.enabled=false;
		_peck = Peck._instance;
		_mainCam=Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
		switch(_state){
			case 0:
			default:
				_peckTimer+=Time.deltaTime;
				if(_peckTimer>_peckTime){
					_peckCount++;
					if(_peckCount>=_pecksToWake){
						_state=1;
						_anim.SetTrigger("sing");
						_audio.clip=_sing;
						_audio.Play();
						_singCount++;
					}
					else{
						_peckTimer=0;
						_anim.SetTrigger("peck");
						_audio.clip=_clonk;
						_audio.Play();
					}
				}
				CheckForWake();
				break;
			case 1:
				_singTimer+=Time.deltaTime;
				if(_singTimer>=_singTime){
					_peckTimer=0;
					_state=0;
					_peckCount=0;
					_singTimer=0;
				}
				CheckForWake();
				break;
			case 2:
				//hello friend
				if(Input.anyKeyDown){
					_dialogCounter++;
					switch(_dialogCounter){
						case 0:
						default:
							break;
						case 1:
							_dialog.ShowText("[insert story text here]");
							break;
						case 2:
							_dialog.ShowText("I guess you're not used to your new legs yet");
							break;
						case 3:
							_dialog.ShowText("Try walking over yonder, and bring me back a tasty morsel");
							break;
						case 4:
							_hop.enabled=true;
							_state=3;
							break;
					}
				}
				break;
			case 3:
				//wait for seed
				if((_mainCam.position-transform.position).sqrMagnitude<_chatRange*_chatRange){
					if(_mainCam.GetComponentInChildren<Seed>()!=null&&!_hop.Hopping()){
						_state=4;
						_peck.TurnTo(transform.position+Vector3.up*_eyeHeight);
						Vector3 lookAt = _mainCam.position;
						lookAt.y=transform.position.y;
						TurnTo(lookAt);
						_dialog.HideText();
					}
				}
				break;
			case 4:
				//look at eachother
				if(_doneTurning && _peck._doneTurning){
					Debug.Log("Embrace for impact");
					_peckTimer=0;
					_anim.SetTrigger("peck");
					_state=5;
				}
				break;
			case 5:
				//take seed
				_peckTimer+=Time.deltaTime;
				if(_peckTimer>=_peckTime){
					_audio.clip=_clonk;
					_audio.Play();
					_seed = _peck._holding;
					_seed.SetParent(_biteParent);
					_seed.localPosition=Vector3.zero;
					_peck._holding=null;
					_state=6;
					_eatTimer=0;
					_anim.SetTrigger("sing");
					_audio.clip=_nomNom;
					_audio.Play();
					//throw shellA in the air
					Transform shell = _seed.GetChild(1);
					StartCoroutine(ChuckR(shell));
				}
				break;
			case 6:
				//eating
				_eatTimer+=Time.deltaTime;
				if(_eatTimer>=_singAnimTime)
				{
					_eatCount++;
					if(_eatCount==1){
						//play another nom
						_audio.Play();
						_eatTimer=0;
						_anim.SetTrigger("sing");
						//throw shellB in the air
						Transform shell = _seed.GetChild(0);
						StartCoroutine(ChuckR(shell));
					}
					else if(_eatCount==2){
						_audio.clip= _chalp;
						_audio.Play();
						_eatTimer=0;
						_anim.SetTrigger("sing");
						//eat seed
						StartCoroutine(EatSeedR());
					}
					else{
						_state=7;
					}
				}
				break;
			case 7:
				//done eating
				break;
		}
    }

	void CheckForWake(){
		if(_singCount>=1 || _skipFirstSong){
			if(Input.anyKeyDown)
			{
				//check for wake up
				_hop.enabled=true;
				_hop.ResetPosRot();
				_hop.enabled=false;
				transform.position+=Vector3.forward*_scootBackAmount;
				_state=2;
				_dialog.ShowText("Hello Friend, that was quite a fall you took there");
			}
		}
	}

	void TurnTo(Vector3 v){
		StartCoroutine(TurnToR(v));
	}

	IEnumerator TurnToR(Vector3 target){
		_doneTurning=false;
		Quaternion startRot=transform.rotation;
		transform.LookAt(target);
		Quaternion endRot=transform.rotation;
		Vector3 startPos=transform.position;
		Vector3 endPos = target+(startPos-target).normalized*_biteDistance;
		float timer = 0;
		while(timer<_turnTime){
			timer+=Time.deltaTime;
			transform.rotation=Quaternion.Slerp(startRot,endRot,timer/_turnTime);
			transform.position=Vector3.Lerp(startPos,endPos,timer/_turnTime);
			yield return null;
		}
		_doneTurning=true;
	}

	IEnumerator ChuckR(Transform t){
		t.SetParent(null);
		Rigidbody rb = t.gameObject.AddComponent<Rigidbody>();
		t.gameObject.AddComponent<SphereCollider>();
		rb.AddForce(Random.onUnitSphere);
		rb.AddTorque(Random.onUnitSphere);
		yield return new WaitForSeconds(_chuckTime);
		Destroy(t.gameObject);
	}

	IEnumerator EatSeedR(){
		_seed.GetComponent<MeshRenderer>().enabled=false;
		yield return new WaitForSeconds(_chuckTime);
		Destroy(_seed.gameObject);
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.red;
		Gizmos.DrawWireSphere(transform.position,_chatRange);
	}
}
