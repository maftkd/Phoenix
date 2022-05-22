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
	public bool _listening;
	TreeBehaviour _tb;

	//public delegate void BirdEvent(bool foo);
	//public event BirdEvent _onTargetted;

	public AudioClip _rewardSound;

	void Awake(){
		_target=transform.Find("Target").gameObject;
		_sing=transform.GetComponentInChildren<Sing>();
		_targetMat=_target.GetComponent<Renderer>().material;
		_anim=GetComponent<Animator>();
		_audio=gameObject.AddComponent<AudioSource>();
		_tb=GetComponent<TreeBehaviour>();
	}
    // Start is called before the first frame update
    void Start()
    {
		//StartCoroutine(ToggleCamera());
    }

    // Update is called once per frame
    void Update()
    {
		if(_listening)
			return;
		if(_targeted&&!_target.activeSelf)
		{
			_target.SetActive(true);
			TipHud.ShowTip("Press E to Sing",transform,Vector3.up*0.5f);
			//_targetMat.SetFloat("_Fill", 0);
		}
		else if(!_targeted&&_target.activeSelf)
		{
			_target.SetActive(false);
			TipHud.ClearTip();
			//_targetMat.SetFloat("_Fill", 0);
			_targetTimer=0f;
		}
    }
        
	void LateUpdate(){
		_targeted=false;
	}

	public void Targeted(){
		_targeted=true;
	}

	public void StartListening(){
		_target.SetActive(true);
		_targetMat.SetFloat("_Fill", 0);
		_listening=true;
	}

	public void StopListening(){
		_targetMat.SetFloat("_Fill", 1);
		_listening=false;

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
		_tb.ScareIntoTree();
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
