using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPB : MonoBehaviour
{
	bool _targeted;
	GameObject _target;
	Sing _sing;
	float _targetTime=1f;
	float _targetTimer;
	Material _targetMat;
	Animator _anim;
	AudioSource _audio;

	public delegate void BirdEvent(bool foo);
	public event BirdEvent _onTargetted;

	public AudioClip _rewardSound;

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
		//StartCoroutine(ToggleCamera());
    }

    // Update is called once per frame
    void Update()
    {
		if(_targeted&&!_target.activeSelf)
		{
			_target.SetActive(true);
			if(_onTargetted!=null)
				_onTargetted.Invoke(true);
		}
		else if(!_targeted&&_target.activeSelf)
		{
			_target.SetActive(false);
			_targetMat.SetFloat("_Fill", 0);
			_targetTimer=0f;
			if(_onTargetted!=null)
				_onTargetted.Invoke(false);
		}
		if(_targeted&&_targetTimer<_targetTime){
			//inc target timer
			_targetTimer+=Time.deltaTime;
			_targetMat.SetFloat("_Fill",_targetTimer/_targetTime);
			if(_targetTimer>=_targetTime){
				//_targetTimer=0;
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
		if(cam!=null){
			cam.enabled=true;
			yield return null;
			cam.enabled=false;
		}
	}

	public void FullPatternSuccess(){
		Sfx.PlayOneShot3D(_rewardSound,transform.position,Random.Range(0.95f,1.05f));
		StartCoroutine(Success());
	}

	IEnumerator Success(){
		_targetMat.SetFloat("_Success",1f);
		yield return new WaitForSeconds(1f);
		float success=1f;
		while(success>0){
			success-=Time.deltaTime;
			_targetMat.SetFloat("_Success",success);
			yield return null;
		}
	}
}
