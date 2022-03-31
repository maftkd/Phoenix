using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TouchPlate : MonoBehaviour
{
	Bird _player;
	public AudioClip _sound;
	bool _isOn;
	Material _mat;
	public float _delay;
	public float _animDur;
	public Transform _plateParts;
	public UnityEvent _onPowerOn;
	public UnityEvent _onPowerOff;

	void Awake(){
		_player=GameManager._player;
		_mat=GetComponent<Renderer>().material;
		_mat.SetFloat("_Interactable",0);
		_mat.SetFloat("_On",0);
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
		Debug.Log("Bird in");
		_player.NearTouchPlate(this);
		_mat.SetFloat("_Interactable",1);
	}

	void OnTriggerExit(Collider other){
		Debug.Log("Bird out");
		_player.LeaveTouchPlate(this);
		_mat.SetFloat("_Interactable",0);
	}

	public void Toggle(){
		_player._anim.SetTrigger("peck");
		_isOn=!_isOn;
		StartCoroutine(ToggleR());
		//mat set emission: 
		//_mat.SetColor("_EmissionColor",_isOn?_emissionColor:Color.black);
	}

	IEnumerator ToggleR(){
		yield return new WaitForSeconds(_delay);
		Sfx.PlayOneShot3D(_sound,transform.position,Random.Range(0.7f,1.3f));
		Transform fx = Instantiate(_plateParts,_player.transform.position+_player.transform.forward*0.1f,Quaternion.identity);
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
