using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TouchPlate : MonoBehaviour
{
	Bird _player;
	public AudioClip _sound;
	public bool _isOn;
	Material _mat;
	public float _delay;
	public float _animDur;
	public Transform _plateParts;
	public UnityEvent _onPowerOn;
	public UnityEvent _onPowerOff;
	BirdHouse _bh;

	void Awake(){
		_player=GameManager._player;
		_mat=GetComponent<Renderer>().material;
		_mat.SetFloat("_Interactable",0);
		_mat.SetFloat("_On",0);
		_bh=transform.GetComponentInParent<BirdHouse>();
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	void OnTriggerEnter(Collider other){
		_player.NearTouchPlate(this);
		_mat.SetFloat("_Interactable",1);
		//Toggle();
	}

	void OnTriggerExit(Collider other){
		_player.LeaveTouchPlate(this);
		_mat.SetFloat("_Interactable",0);
	}

	public void Toggle(){
		_isOn=!_isOn;
		StartCoroutine(ToggleR());
		if(_bh!=null){
			_bh.CheckPlates();
		}
	}

	IEnumerator ToggleR(){
		/*
		yield return new WaitForSeconds(_delay);
		Transform fx = Instantiate(_plateParts,_player.transform.position+_player.transform.forward*0.1f,Quaternion.identity);
		*/
		float pitch=_isOn? Random.Range(1f,1.3f) : Random.Range(0.7f,1f);
		float vol = _isOn? 1f : 0.3f;
		Sfx.PlayOneShot3D(_sound,transform.position,pitch,vol);
		if(_isOn)
			_onPowerOn.Invoke();
		else
			_onPowerOff.Invoke();
		float timer=0;
		float dur=_animDur;
		while(timer<dur){
			timer+=Time.deltaTime;
			float frac=timer/dur;
			_mat.SetFloat("_On",_isOn?frac:1-frac);
			yield return null;
		}
		_mat.SetFloat("_On",_isOn?1f:0f);
	}
}
