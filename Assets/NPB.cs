using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPB : MonoBehaviour
{
	bool _targeted;
	GameObject _target;
	[HideInInspector]
	public Sing _sing;
	float _targetTime=1f;
	float _targetTimer;
	Material _targetMat;
	Animator _anim;
	AudioSource _audio;
	public bool _listening;
	TreeBehaviour _tb;
	bool _scanned;
	BirdSpawner _bs;
	Fader _fader;

	//public delegate void BirdEvent(bool foo);
	//public event BirdEvent _onTargetted;

	public AudioClip[] _rewardSounds;
	public AudioClip _rewardBirdCount;
	public AudioClip _patternFail;

	void Awake(){
		_target=transform.Find("Target").gameObject;
		_sing=transform.GetComponentInChildren<Sing>();
		_targetMat=_target.GetComponent<Renderer>().material;
		_anim=GetComponent<Animator>();
		_audio=gameObject.AddComponent<AudioSource>();
		_tb=GetComponent<TreeBehaviour>();
		_bs=transform.parent.GetComponent<BirdSpawner>();
		_fader=GetComponent<Fader>();
		AudioSource audio = GetComponent<AudioSource>();
		audio.clip=Synthesizer.GenerateSimpleWave(440f,1f,0.05f);
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
			_targetTimer=0f;
		}
		else if(!_targeted&&_target.activeSelf)
		{
			_target.SetActive(false);
			TipHud.ClearTip();
			if(!_scanned){
				_targetMat.SetFloat("_Fill",0);
			}
			if(_fader.IsOn())
				_fader.Stop();
			_targetTimer=0f;
		}
		if(!_scanned&&_targeted){
			if(!_fader.IsOn())
				_fader.Play();
			_targetTimer+=Time.deltaTime;
			float frac=_targetTimer/_targetTime;
			_targetMat.SetFloat("_Fill",frac);
			if(_targetTimer>=_targetTime){
				IncBirdCount();
				_scanned=true;
				_fader.Stop();
			}
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

	public void FullPatternSuccess(int i){
		Sfx.PlayOneShot3D(_rewardSounds[i],transform.position);
		StartCoroutine(Success(Color.green));
		_tb.ScareIntoTree();
	}

	public void IncBirdCount(){
		Sfx.PlayOneShot3D(_rewardBirdCount,transform.position);
		_bs.IncBirdCount(_sing._name,_sing._male);
	}

	IEnumerator Success(Color c){
		_targetMat.SetFloat("_Success",1f);
		_targetMat.SetColor("_SuccessColor", c);
		yield return new WaitForSeconds(1f);
		float success=1f;
		while(success>0){
			success-=Time.deltaTime;
			_targetMat.SetFloat("_Success",success);
			yield return null;
		}
	}

	public void PatternFail(){
		StartCoroutine(Success(Color.red));
		Sfx.PlayOneShot3D(_patternFail,transform.position);
	}

	public void Hush(){
		if(_fader.IsOn())
			_fader.Stop();
	}
}
