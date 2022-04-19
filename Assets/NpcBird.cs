using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcBird : MonoBehaviour
{
	Bird _player;
	public AudioClip _call;
	public Transform _callEffects;
	public float _callbackDelay;
	public Vector2 _callPitchRange;
	Animator _anim;
	AudioSource _source;
	bool _inZone;
	Dialog _dialog;

	void Awake(){
		_player=GameManager._player;
		_player._onCall+=Callback;
		_anim=GetComponent<Animator>();
		_dialog=GetComponent<Dialog>();
	}
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void PlayerEnter(){
		_inZone=true;
		_dialog.Activate(true);
	}

	public void PlayerExit(){
		_inZone=false;
		_dialog.Activate(false);
	}

	public void Callback(){
		if(_source!=null&&_source.isPlaying)
			return;
		if(!_inZone)
			return;
		StartCoroutine(CallbackR());
	}

	IEnumerator CallbackR(){
		yield return new WaitForSeconds(_callbackDelay);
		Transform call = Instantiate(_callEffects,transform.position+Vector3.up*0.2f,Quaternion.identity);
		_source = call.GetComponent<AudioSource>();
		_source.clip=_call;
		_source.pitch=Random.Range(_callPitchRange.x,_callPitchRange.y);
		_source.Play();
		_anim.SetTrigger("sing");

	}
}
