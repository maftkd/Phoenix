using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcBird : MonoBehaviour
{
	Bird _player;
	public AudioClip [] _call;
	public Vector2 _callDelayRange;
	public AudioClip _song;
	public Transform _callEffects;
	public float _callbackDelay;
	public Vector2 _callPitchRange;
	Animator _anim;
	AudioSource _source;
	bool _inZone;
	Dialog _dialog;
	bool _targeted;
	GameObject _target;
	bool _tamed;
	public AudioClip _chalp;
	public AudioClip _nomNom;
	public float _flySpeed;
	public float _flapDur;
	public AudioClip _flapSound;
	public AnimationCurve _vertCurve;
	public float _vertAmount;
	int _state;

	void Awake(){
		_player=GameManager._player;
		_player._onCall+=Callback;
		_anim=GetComponent<Animator>();
		_dialog=GetComponent<Dialog>();
		_target=transform.Find("Target").gameObject;
	}
    // Start is called before the first frame update
    void Start()
    {
		StartCoroutine(Call());
    }

    // Update is called once per frame
    void Update()
    {
		if(_targeted&&!_target.activeSelf)
			_target.SetActive(true);
		else if(!_targeted&&_target.activeSelf)
			_target.SetActive(false);
    }

	void LateUpdate(){
		_targeted=false;
	}

	public void PlayerEnter(){
		_inZone=true;
		_dialog.Activate(true);

		//if player has seed
		//	eat it
		//	sing song
		//	if song not learned
		//		learn song
		//
		if(_player.DestroyIfHasSeed()){
			StartCoroutine(EatSeed());
			_tamed=true;

		}
	}

	public void PlayerExit(){
		_inZone=false;
		_dialog.Activate(false);
	}

	public void Callback(){
		if(!_tamed)
			return;
		if(_targeted){
			StartCoroutine(FlyTo(_player.transform.position));
		}
		else{
			Transform call = Instantiate(_callEffects,transform.position+Vector3.up*0.2f,Quaternion.identity);
			_source = call.GetComponent<AudioSource>();
			_source.clip=_song;
			_source.pitch=Random.Range(_callPitchRange.x,_callPitchRange.y);
			_source.Play();
			_anim.SetTrigger("sing");
		}
	}

	public void Targeted(){
		_targeted=true;
	}

	IEnumerator Call(){
		float delay = Random.Range(_callDelayRange.x,_callDelayRange.y);
		yield return new WaitForSeconds(delay);
		//Sfx.PlayOneShot3D(,transform.position);
		if(_source!=null&&_source.isPlaying || _state==1)
		{
			//don't chirp if already making sound
		}
		else
		{
			Transform call = Instantiate(_callEffects,transform.position+Vector3.up*0.15f,Quaternion.identity);
			_source = call.GetComponent<AudioSource>();
			_source.clip=_call[Random.Range(0,_call.Length)];
			_source.pitch=Random.Range(_callPitchRange.x,_callPitchRange.y);
			_source.Play();
			_anim.SetTrigger("sing");
		}
		StartCoroutine(Call());
	}

	IEnumerator EatSeed(){
		transform.LookAt(_player.transform);
		Vector3 eulers = transform.eulerAngles;
		eulers.x=0;
		transform.eulerAngles=eulers;
		_anim.SetTrigger("peck");
		Sfx.PlayOneShot2D(_chalp);
		yield return new WaitForSeconds(0.5f);
		Sfx.PlayOneShot2D(_nomNom);
		yield return new WaitForSeconds(1f);

		Transform call = Instantiate(_callEffects,transform.position+Vector3.up*0.2f,Quaternion.identity);
		_source = call.GetComponent<AudioSource>();
		_source.clip=_song;
		_source.pitch=Random.Range(_callPitchRange.x,_callPitchRange.y);
		_source.Play();
		_anim.SetTrigger("sing");
	}

	IEnumerator FlyTo(Vector3 target){
		_state=1;
		_anim.SetTrigger("flyLoop");

		//get target pos
		Vector3 startPos=transform.position;
		Vector3 endPos=target;
		Vector3 dir=(endPos-startPos).normalized;
		endPos-=dir*0.25f;
		RaycastHit hit;
		if(Physics.Raycast(endPos+Vector3.up*0.2f,Vector3.down, out hit, 0.4f,_player._collisionLayer)){
			endPos.y=hit.point.y;
		}

		//orient
		transform.LookAt(endPos);
		Vector3 eulerAngles=transform.eulerAngles;
		eulerAngles.x=0;
		transform.eulerAngles=eulerAngles;

		//calculate distance and duration
		float dist=(endPos-startPos).magnitude;
		float speed=_flySpeed;
		float dur=dist/speed;
		float vertAmount=dist*_vertAmount;

		//animate
		float timer=0f;
		float flapTimer=10f;
		while(timer<dur){
			timer+=Time.deltaTime;
			float frac=timer/dur;
			Vector3 pos = Vector3.Lerp(startPos,endPos,frac);
			pos.y+=Mathf.Lerp(0f,vertAmount,_vertCurve.Evaluate(frac));
			transform.position=pos;
			flapTimer+=Time.deltaTime;
			if(flapTimer>_flapDur){
				Sfx.PlayOneShot3D(_flapSound,transform.position,Random.Range(0.7f,1.3f));
				flapTimer=0f;
			}
			yield return null;
		}
		transform.position=endPos;
		_anim.SetTrigger("land");
		_state=0;
	}
}
