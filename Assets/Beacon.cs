using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beacon : MonoBehaviour
{
	AudioSource _hum;
	public AudioClip _smallSwitch;
	public AudioClip _bigSwitch;
	public MeshRenderer _domeGlass;
	public MeshRenderer _domeLight;
	public float _flipPeriod;

	void Awake(){
		_hum=GetComponent<AudioSource>();
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void Activate(){
		StartCoroutine(ActivateR());
	}

	IEnumerator ActivateR(){
		Sfx.PlayOneShot2D(_smallSwitch);
		yield return new WaitForSeconds(_smallSwitch.length);
		Sfx.PlayOneShot2D(_bigSwitch);
		float dur = _bigSwitch.length;
		float timer=0;
		Color c = Color.black;
		float flipTimer=0;
		while(timer<dur){
			timer+=Time.deltaTime;
			flipTimer+=Time.deltaTime;
			if(flipTimer>_flipPeriod){
				_domeGlass.enabled=!_domeGlass.enabled;
				_domeLight.enabled=!_domeLight.enabled;
				flipTimer=0;
			}
			yield return null;
		}
		_domeGlass.enabled=false;
		_domeLight.enabled=true;
		_hum.Play();
		Debug.Log("Time to activate the beam");
	}
}
