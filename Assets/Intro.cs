using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intro : MonoBehaviour
{
	public enum Mode {DEFAULT,FAST,SKIP,COVER_ART};
	public Mode _mode;
	MCamera _mCam;
	MInput _mIn;

	[Header("Intro bird")]
	public Bird _introBird;
	public float _birdStartDistance;
	Vector3 _platePos;

	[Header("Intro text")]
	public Material _titleMat;
	public CanvasGroup _startPrompt;
	public float _blinkDur;
	public float _powerDur;
	public AudioClip _powerSound;

	[Header("FlightSequence")]
	public float _letterBoxAmount;
	public Camera _introCam;
	public Camera _overheadCam;
	public Bird _overheadBird;
	public float _flightDist;
	public float _dollyDur;
	public float _truckDur;
	Dolly _overheadDolly;
	public Camera _truckCam;
	Truck _overheadTruck;
	Bird _player;
	public Camera _landingCam;
	Pan _landingPan;
	public Transform _landingParts;
	public float _landPartsDelay;
	public float _orbitDur;
	public float _letterBoxFadeDur;
	public GameObject _coverArtIsland;

	[Header("Cover art mode")]
	public GameObject _rock;

	void Awake(){
		//get refs
		_mCam=GameManager._mCam;
		_mIn = GameManager._mIn;
		_overheadDolly=_overheadCam.GetComponent<Dolly>();
		_overheadTruck=_truckCam.GetComponent<Truck>();
		_player=GameManager._player;
		_landingPan = _landingCam.GetComponent<Pan>();

		if(_mode==Mode.FAST){
			_dollyDur*=0.1f;
			_truckDur*=0.1f;
			_orbitDur*=0.1f;
		}

		switch(_mode){
			case Mode.DEFAULT:
			case Mode.FAST:
			default:
				_platePos=_introBird.transform.position;
				_introBird.transform.position+=Vector3.up*_birdStartDistance;
				_introBird.transform.position-=_introBird.transform.forward*_birdStartDistance;
				_titleMat.SetFloat("_Power",0);
				_startPrompt.alpha=0f;
				_mCam.SnapToCamera(_introCam);
				_mIn.LockInput(true);
				break;
			case Mode.COVER_ART:
				//PowerTitleSign(true);
				_startPrompt.gameObject.SetActive(false);
				_mCam.SnapToCamera(_introCam);
				_rock.SetActive(false);
				_titleMat.SetFloat("_Power",0);
				break;
			case Mode.SKIP:
				GameManager._instance.Play();
				_coverArtIsland.SetActive(false);
				enabled=false;
				_mCam.SnapToCamera(_player._waddleCam);
				break;
		}
	}

	public void PowerTitleSign(bool p){
		StartCoroutine(PowerTitleSignR(p?1f:0.1f));
		if(p)
			Sfx.PlayOneShot2D(_powerSound);
	}

	IEnumerator PowerTitleSignR(float target){
		float timer=0;
		float dur = _powerDur;
		float tStart=_titleMat.GetFloat("_Power");
		float power=0;
		while(timer<dur){
			timer+=Time.deltaTime;
			power=Mathf.Lerp(tStart,target,timer/dur);
			_titleMat.SetFloat("_Power",power);
			yield return null;
		}
	}

    // Start is called before the first frame update
    void Start()
    {
		switch(_mode){
			case Mode.DEFAULT:
			case Mode.FAST:
			default:
				_introBird._onDoneFlying+=Alighted;
				_introBird.FlyTo(_platePos);
				break;
			case Mode.COVER_ART:
				break;
		}
    }

	public void Alighted(){
		Debug.Log("Alighted!");
		StartCoroutine(BlinkPrompt());
	}

	IEnumerator BlinkPrompt(){
		float timer=0;
		while(timer<_blinkDur){
			timer+=Time.deltaTime;
			_startPrompt.alpha=(Mathf.Sin(Mathf.PI*2f*timer/_blinkDur)+1)*0.5f;
			yield return null;
			if(Input.anyKeyDown){
				timer=101f;
			}
		}
		if(timer>100f){
			_startPrompt.gameObject.SetActive(false);
			//bird flies behind cam
			Vector3 flightTarget=_introCam.transform.position;
			flightTarget+=Vector3.up*0.2f;
			_introBird._onDoneFlying-=Alighted;
			_introBird._onDoneFlying+=BirdBehindCam;
			_introBird.FlyTo(flightTarget);
		}
		else
		{
			//keep blinking
			StartCoroutine(BlinkPrompt());
		}
	}

	public void BirdBehindCam(){
		_mCam.Transition(_overheadCam,MCamera.Transitions.FADE,_letterBoxAmount);
		Vector3 start=_overheadBird.transform.position;
		Vector3 target=start+_overheadBird.transform.forward*_flightDist;
		_overheadBird.FlyTo(target,0.2f);
		_overheadDolly.StartTracking(_overheadBird.transform);
		_overheadTruck.StartTracking(_overheadBird.transform);

		StartCoroutine(FlightCapture());
	}

	IEnumerator FlightCapture(){
		//overhead cam already set up
		yield return new WaitForSeconds(_dollyDur);
		//transition to truck cam
		_mCam.Transition(_truckCam,MCamera.Transitions.FADE,_letterBoxAmount);
		yield return new WaitForSeconds(_truckDur);
		_overheadDolly.enabled=false;
		//player flies in
		Vector3 startPos=_player.transform.position;
		Vector3 target=startPos-_player.transform.forward*12f;
		target+=Vector3.up*3f;
		_player.transform.position=target;
		_player.FlyTo(startPos,0.8f);
		_player._onDoneFlying+=PlayerArrived;
		_landingPan.StartTracking(_player.transform);
		//transition to track cam
		_mCam.Transition(_landingCam,MCamera.Transitions.FADE,_letterBoxAmount);
		yield return new WaitForSeconds(_landPartsDelay);
		_overheadTruck.enabled=false;
		Instantiate(_landingParts,startPos,Quaternion.identity);
	}

	public void PlayerArrived(){
		Debug.Log("Player arrived!");
		//disable intro cams
		_landingPan.enabled=false;
		_overheadTruck.enabled=false;
		_player.Ruffle();
		StartCoroutine(GivePlayerControl());
	}

	IEnumerator GivePlayerControl(){
		//mCam. transition to (bird cam, orbit transition)
		_mCam.Transition(_player._waddleCam,MCamera.Transitions.ORBIT,_letterBoxAmount,_player.transform,_orbitDur);
		yield return new WaitForSeconds(_orbitDur);
		_mCam.LerpLetterBox(0,_letterBoxFadeDur);
		yield return new WaitForSeconds(_letterBoxFadeDur);
		//free input, etc.
		GameManager._instance.Play();
		_coverArtIsland.SetActive(false);
		enabled=false;
	}
}
