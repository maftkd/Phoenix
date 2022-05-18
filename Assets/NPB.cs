using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPB : MonoBehaviour
{
	bool _targeted;
	GameObject _target;
	Sing _sing;
	float _targetTime=5f;
	float _targetTimer;
	Material _targetMat;
	Animator _anim;
	AudioSource _audio;

	void Awake(){
		_target=transform.Find("Target").gameObject;
		_sing=transform.GetComponentInChildren<Sing>();
		_targetMat=_target.GetComponent<Renderer>().material;
		_anim=GetComponent<Animator>();
		_audio=gameObject.AddComponent<AudioSource>();
	}
    // Start is called before the first frame update
    void Start()
    {
		StartCoroutine(ToggleCamera());
    }

    // Update is called once per frame
    void Update()
    {
		if(_targeted&&!_target.activeSelf)
		{
			_target.SetActive(true);
		}
		else if(!_targeted&&_target.activeSelf)
		{
			_target.SetActive(false);
			_targetMat.SetFloat("_Fill", 0);
			_targetTimer=0f;
		}
		if(_targeted&&_targetTimer<_targetTime){
			//inc target timer
			_targetTimer+=Time.deltaTime;
			_targetMat.SetFloat("_Fill",_targetTimer/_targetTime);
			if(_targetTimer>=_targetTime){
				_targetTimer=0;
				_sing.SingSong();
			}
		}
    }
        
	void LateUpdate(){
		_targeted=false;
	}

	public void Targeted(){
		_targeted=true;
	}

	IEnumerator ToggleCamera(){
		Camera cam = transform.GetComponentInChildren<Camera>();
		yield return null;
		cam.enabled=true;
		yield return null;
		cam.enabled=false;
	}
}
