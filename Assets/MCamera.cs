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
	bool _flying;
	Temptress _temptress;
	public float _shotRange;
	bool _chill;
	Human _human;
    // Start is called before the first frame update
    void Start()
    {
		_audio=GetComponent<AudioSource>();
		_button=transform.GetChild(0);
		_shootTime=_audio.clip.length;
		_flash=transform.GetChild(1).GetComponent<CanvasGroup>();
		_flash.alpha=0;
		if(transform.parent.parent.GetComponent<Human>()!=null)
		{
			_temptress=transform.parent.parent.parent.GetComponentInChildren<Temptress>();
			_shotRange=transform.parent.GetChild(1).GetComponent<LineRenderer>().GetPosition(1).x;
		}
		_human=transform.GetComponentInParent<Human>();
    }

    // Update is called once per frame
    void Update()
    {
		if(_shootTimer<=0){
			if(_human!=null&&Input.GetButtonDown(GameManager._jumpButton)&&!_flying){
				Flash();
				if(_tutorial!=null)
					Destroy(_tutorial);
				if(_temptress!=null){
					if((_temptress.transform.position-transform.position).sqrMagnitude<_shotRange*_shotRange)
						_temptress.Startle();
				}
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

	void LateUpdate(){
		if(!_flying)
			transform.rotation=Quaternion.identity;
	}

	public void FlashAndChilll(){
		if(_chill)
			return;
		_chill=true;
		Flash();
	}

	public void ResetChill(){
		_chill=false;
	}

	public void Flash(){
		_audio.Play();
		_shootTimer=_shootTime;
	}

	public void GoFlying(){
		StartCoroutine(FlyAwayR());
	}

	IEnumerator FlyAwayR(){
		_flying=true;
		Rigidbody rb = gameObject.AddComponent<Rigidbody>();
		rb.AddForce(new Vector3(Random.value,1f,0)*_flyForce);
		rb.AddTorque(Vector3.forward*Random.value*_flyForce);
		yield return new WaitForSeconds(5f);
		Destroy(gameObject);
	}
}
