using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPB : MonoBehaviour
{
	bool _targeted;
	GameObject _target;
	[HideInInspector]
	public Sing _sing;//sing
	float _targetTime=1f;
	float _targetTimer;
	Material _targetMat;
	Animator _anim;
	AudioSource _audio;
	public bool _scanned;
	TreeBehaviour _tb;
	BirdSpawner _bs;
	Fader _fader;
	Bird _player;
	public string _species;
	public string _speciesLatin;
	int _state;
	Text _debugText;

	//public delegate void BirdEvent(bool foo);
	//public event BirdEvent _onTargetted;

	public AudioClip[] _rewardSounds;
	public AudioClip _rewardBirdCount;
	public AudioClip _patternFail;

	void Awake(){
		_target=transform.Find("Target").gameObject;
		_sing=transform.GetComponentInChildren<Sing>();
		_targetMat=_target.GetComponent<Renderer>().material;
		_target.SetActive(false);
		_anim=GetComponent<Animator>();
		_audio=gameObject.AddComponent<AudioSource>();
		_tb=GetComponent<TreeBehaviour>();
		_bs=transform.parent.GetComponent<BirdSpawner>();
		_fader=GetComponent<Fader>();
		AudioSource audio = GetComponent<AudioSource>();
		audio.clip=Synthesizer.GenerateSimpleWave(440f,1f,0.05f);
		_player=GameManager._player;
		_debugText=transform.GetComponentInChildren<Text>();
	}
    // Start is called before the first frame update
    void Start()
    {
		//StartCoroutine(ToggleCamera());
    }

    // Update is called once per frame
    void Update()
    {
		switch(_state){
			case 0:
				if(_targeted){
					_target.SetActive(true);
					_targetTimer=0f;
					_state=1;
				}
				break;
			case 1:
				if(!_fader.IsOn())
					_fader.Play();
				_targetTimer+=Time.deltaTime;
				float frac=_targetTimer/_targetTime;
				_targetMat.SetFloat("_Fill",frac);
				if(_targetTimer>=_targetTime){
					//scan complete
					_player.ScanBird();
					GetScanned();
					_state=2;
				}
				if(!_targeted){
					//scan broken
					_state=0;
					_fader.Stop();
					_targetMat.SetFloat("_Fill",0);
					_target.SetActive(false);
				}
				break;
			case 2:
				//scanned
				break;
			case 3:
				//post scan
				if(!_targeted){
					_state=0;
					_targetMat.SetFloat("_Fill",0);
					_target.SetActive(false);
				}
				break;
		}
		_debugText.text="state: "+_state.ToString();
    }

	public void Targeted(bool t){
		_targeted=t;
	}

	/*
	public void StartListening(){
		_target.SetActive(true);
		//_targetMat.SetFloat("_Fill", 0);
		_scanned=true;
		int index = _sing.SingSong();
		//Sfx.PlayOneShot3D(_rewardSounds[index],transform.position);
	}
	*/

	/*
	public void StopListening(){
		//_targetMat.SetFloat("_Fill", 1);
		_listening=false;
	}
	*/

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
		//Sfx.PlayOneShot3D(_rewardSounds[i],transform.position);
		Sfx.PlayOneShot3D(_rewardBirdCount,transform.position);
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

	public void GetScanned(){
		if(_fader.IsOn())
		{
			_fader.Stop();
			_targetMat.SetFloat("_Fill",1f);
			//_scanned=true;
			//IncBirdCount();
		}
		_scanned=true;
		_sing.Engage();
	}

	public void EndScan(){
		//_scanned=false;
		_sing.Free();
		_state=3;
		_scanned=false;
		//some temp code here
		//we needa code up a song list / song menu
		//we needa know if the most recent bird scanned
		//if its a new bird we gotta add it to the song list
		//and show the tip hud
		//but for temp, since we just testing tip hud, we show that every time
		TipHud.ShowTip("New Song Learned!",_species,"Press 'E' to select song");
	}
}
