using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MCamera : MonoBehaviour
{
	AudioSource _audio;
	Transform _button;
	float _shootTime;
	float _shootTimer;
	CanvasGroup _flash;
	public GameObject _tutorial;
	public float _flyForce;
    // Start is called before the first frame update
    void Start()
    {
		_audio=GetComponent<AudioSource>();
		_button=transform.GetChild(0);
		_shootTime=_audio.clip.length;
		_flash=transform.GetChild(1).GetComponent<CanvasGroup>();
		_flash.alpha=0;
    }

    // Update is called once per frame
    void Update()
    {
		if(_shootTimer<=0){
			if(Input.GetKeyDown(KeyCode.Space)){
				_audio.Play();
				_shootTimer=_shootTime;
				if(_tutorial!=null)
					Destroy(_tutorial);
			}
		}
		else{
			_shootTimer-=Time.deltaTime;
			float t = 1f-_shootTimer/_shootTime;
			_button.localPosition=Vector3.down*Mathf.Sin(t*Mathf.PI)*0.2f;
			_flash.alpha=1-t;
			if(_shootTimer<=0){
				_button.localPosition=Vector3.zero;
				_flash.alpha=0;
			}
		}
    }

	public void GoFlying(){
		StartCoroutine(FlyAwayR());
	}

	IEnumerator FlyAwayR(){
		Rigidbody rb = gameObject.AddComponent<Rigidbody>();
		rb.AddForce(new Vector3(Random.value,1f,0)*_flyForce);
		rb.AddTorque(Vector3.forward*Random.value*_flyForce);
		yield return new WaitForSeconds(5f);
		Destroy(gameObject);
	}
}
