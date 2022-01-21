using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PressurePlate : MonoBehaviour
{
	bool _npcOnPlate;
	BoxCollider _col;
	MCamera _mCam;
	int _load;
	public int _requiredLoad;
	Vector3 _startPos;
	public float _moveAmount;
	public float _moveLerp;
	public AudioClip _pressAudio;
	public Transform _dustParts;
	Bird _player;
	Bird _mate;
	public UnityEvent _onActivated;
	bool _activated;

	void Awake(){
		_col=GetComponent<BoxCollider>();
		_mCam=Camera.main.GetComponent<MCamera>();
		_startPos=transform.position;
		_player=GameObject.FindGameObjectWithTag("Player").GetComponent<Bird>();
		_mate=_player._mate;
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		float pressed=_load/(float)_requiredLoad;
		Vector3 target=_startPos+Vector3.down*_moveAmount*pressed;
		transform.position=Vector3.Lerp(transform.position,target,_moveLerp*Time.deltaTime);
    }

	void OnTriggerEnter(Collider other){
		if(_activated)
			return;
		if(other.GetComponent<Bird>()!=null){
			Bird b = other.GetComponent<Bird>();
			if(!_npcOnPlate&&b==_mate)
				StartCoroutine(PutNpcOnPlate());
			else if(b._playerControlled){
				Debug.Log("Player hit plate");
				if(b.transform.position.y>=transform.position.y+transform.localScale.y*0.5f)
					Debug.Log("Player on top of plate!");
			}
		}
	}

	IEnumerator PutNpcOnPlate(){
		_npcOnPlate=true;
		Debug.Log("Putting npc on plate");
		_mate.StopRunningAway();
		_mate.transform.position-=_mate.transform.forward*0.1f;
		_mate.HopTo(transform.position+Vector3.up*transform.localScale.y*0.5f);
		while(!_mate.Arrived())
			yield return null;
		_mate.StopHopping();
		Debug.Log("Npc is on plate");
		_mate.transform.SetParent(transform);
		GroundBirdOnPlate(_mate);
		_load++;
		PlayFx();
		CheckLoad();
	}

	public void PlayerOnPlate(){
		if(_activated)
			return;
		if(_player.transform.parent==transform)
			return;
		_player.transform.SetParent(transform);
		_load++;
		PlayFx();
		GroundBirdOnPlate(_player);
		if(!_npcOnPlate){
			//ForceNpcToPlate();
		}
		CheckLoad();
	}

	public void PlayerOffPlate(){
		Debug.Log("Player is off plate");
		if(_player.transform.parent!=transform)
			return;
		_player.transform.SetParent(null);
		_load--;
		/*
		PlayFx();
		GroundBirdOnPlate(_player);
		if(!_npcOnPlate){
			ForceNpcToPlate();
		}
		CheckLoad();
		*/
	}

	void CheckLoad(){
		if(_activated)
			return;
		if(_load>=_requiredLoad){
			Debug.Log("Activating!");
			_onActivated.Invoke();
			StartCoroutine(UngroundAfterDelay());
		}
	}

	void PlayFx(){
		Sfx.PlayOneShot3D(_pressAudio,transform.position);
		Instantiate(_dustParts,transform.position,Quaternion.identity);
	}

	void ForceNpcToPlate(){
		Vector3 dir = transform.position-_mate.transform.position;
		dir.Normalize();
		_mate.transform.position=transform.position-dir;
		StartCoroutine(PutNpcOnPlate());
	}

	void GroundBirdOnPlate(Bird b){
		Vector3 localPos=b.transform.localPosition;
		localPos.y=0.5f;
		b.transform.localPosition=localPos;
	}

	public IEnumerator UngroundAfterDelay(){
		yield return new WaitForSeconds(1f);
		_player.transform.SetParent(null);
		_mate.transform.SetParent(null);
		_activated=true;
		enabled=false;
	}
}
