using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchPlate : MonoBehaviour
{
	Bird _player;
	public AudioClip _sound;
	bool _isOn;
	Material _mat;
	public float _delay;
	public Transform _plateParts;


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
		_mat.SetFloat("_On",_isOn?1f:0f);
		Sfx.PlayOneShot3D(_sound,transform.position,Random.Range(0.5f,1.5f));
		Transform fx = Instantiate(_plateParts,_player.transform.position+_player.transform.forward*0.1f,Quaternion.identity);
		/*
		ParticleSystem parts=fx.GetComponent<ParticleSystem>();
		var main = parts.main;
		main.startColor=_mat.GetColor("_Color");
		*/
	}
}
