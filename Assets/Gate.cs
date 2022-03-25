using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Gate : MonoBehaviour
{
	public Circuit [] _inputs;
	public Circuit [] _outputs;
	public UnityEvent _onGateActivated;
	public UnityEvent _onGateDeactivated;
	Material _mat;

	public float _chargeDur;
	public float _chargeRate;
	public float _decayRate;
	float _chargeTimer;
	int _chargeState;//0 = idle, 1 = chargeUp, 2 = chargeDown
	bool _powered;
	bool _inputsOn;

	public Transform _sparks;
	public AudioClip _sparkClip;

	public bool _inverter;
	public bool _window;
	bool _windowOpen;
	public bool _windowIsDoor;
	Transform _ring;
	public bool _disableOnPower;
	public bool _node;

	AudioSource _source;
	[Header ("Charger audio settings")]
	public float _maxVolume;
	public float _minPitch;
	public float _maxPitch;
	public float _toneFrequency;
	public float _toneNoise;
	public float _decayVolMult;

	[Header ("Window stuff")]
	public float _doorOpenDelay;
	public float _doorOpenTime;
	public float _doorOpenAngle;
	public AudioClip _doorOpenSound;
	public AudioClip _doorCloseSound;
	public int _seedCount;
	int _seedCounter;
	public Transform _seedPrefab;
	public AudioClip _dispenseSound;
	public Vector2 _ejectForceRange;
	public Vector3 _ejectForce;
	public AnimationCurve _windowOpenCurve;
	Quaternion _doorLeftClosed;
	Quaternion _doorRightClosed;
	Quaternion _doorLeftOpened;
	Quaternion _doorRightOpened;

	bool _init;

	void Awake(){
		if(!_init)
			Init();
	}

	void Init(){
		foreach(Circuit c in _inputs){
			c._powerChange+=CheckGate;
		}
		if(!_window)
		{
			_mat=GetComponent<Renderer>().material;
			_mat.SetFloat("_FillAmount",0);
		}
		else{
			Transform right=transform.Find("WindowRight");
			Transform left=transform.Find("WindowLeft");
			_doorLeftClosed=left.rotation;
			_doorRightClosed=right.rotation;
			left.Rotate(Vector3.up*_doorOpenAngle);
			_doorLeftOpened=left.rotation;
			right.Rotate(-Vector3.up*_doorOpenAngle);
			_doorRightOpened=right.rotation;
			left.rotation=_doorLeftClosed;
			right.rotation=_doorRightClosed;
		}

		//setup charger
		if(_chargeDur>0)
		{
			_source=gameObject.AddComponent<AudioSource>();
			//_source.clip=Synthesizer.GenerateSineWave(880,1f,0.05f);
			_source.clip=Synthesizer.GenerateSquareWave(_toneFrequency,1f,_toneNoise);
			_source.spatialBlend=1f;
			_source.loop=true;
			_source.volume=0;
		}

	}
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if(_chargeDur>0){
			switch(_chargeState){
				case 0://idle
				default:
					if(_inputsOn)
					{
						_chargeState=1;
						_source.Play();

					}
					break;
				case 1://charge up
					if(!_inputsOn){
						_chargeState=2;
						_source.Play();
						break;
					}
					if(_chargeTimer<_chargeDur){
						_chargeTimer+=Time.deltaTime*_chargeRate;
						_source.volume=Mathf.Lerp(0,_maxVolume,_chargeTimer/_chargeDur);
						_source.pitch=Mathf.Lerp(_minPitch,_maxPitch,_chargeTimer/_chargeDur);
						if(_chargeTimer>_chargeDur)
						{
							_chargeTimer=_chargeDur;
							_powered=true;
							CheckGate();
							MakeSparks();
							_source.Stop();
						}
						_mat.SetFloat("_FillAmount", _chargeTimer/_chargeDur);
					}
					break;
				case 2://charge down
					if(_inputsOn){
						_chargeState=1;
						break;
					}
					if(_chargeTimer>0){
						_chargeTimer-=Time.deltaTime*_decayRate;
						_source.volume=Mathf.Lerp(0,_maxVolume*_decayVolMult,_chargeTimer/_chargeDur);
						_source.pitch=Mathf.Lerp(_minPitch,_maxPitch,_chargeTimer/_chargeDur);
						if(_chargeTimer<0)
						{
							_chargeTimer=0;
							_powered=false;
							CheckGate();
							_source.Stop();
							_chargeState=0;
						}
						_mat.SetFloat("_FillAmount", _chargeTimer/_chargeDur);
					}
					break;
				case 3://discharge
					break;
			}
		}
    }

	void CheckGate(){
		bool powered=true;
		for(int i=0; i<_inputs.Length; i++){
			bool inputPowered=_inputs[i].Powered();
			if(_inverter)
				inputPowered=!inputPowered;
			if(!inputPowered)
				powered=false;
		}
		Power(powered);
	}

	public void Power(bool powered){
		_inputsOn=powered;

		if(_inverter){
			_mat.SetFloat("_Invert",powered? 1 : 0);
		}
		//_charging=powered;
		//#temp forcing chargers to not be powered
		if(_chargeDur>0)
			powered=_powered;
			//powered=powered || _chargeTimer>0;//note: charge timer = 0 the instant it's activated
		//this may be temp
		//for now we want the inputs to be modifiable after power
		//but the main gate output is stuck once it powers on
		if(!enabled)
			return;
		if(_chargeDur==0&&_mat!=null)
			_mat.SetFloat("_FillAmount",powered?1:0);
		foreach(Circuit c in _outputs){
			c.Power(powered);
		}
		if(powered){
			//charger gate stays active
			//default gate goes to sleep after power
			if(_chargeDur==0&&!_inverter)
			{
				if(_disableOnPower)
					enabled=false;
				MakeSparks();
				if(_window&&!_windowOpen){
					StopAllCoroutines();
					StartCoroutine(OpenDoors(1f));
				}
				if(_mat!=null)
					_mat.SetFloat("_Lerp",powered?1:0);
			}
			_onGateActivated.Invoke();
		}
		else{
			if(_window&&_windowOpen){
				StopAllCoroutines();
				StartCoroutine(OpenDoors(-1f));
			}
			_onGateDeactivated.Invoke();
		}
	}

	void MakeSparks(){
		if(_sparks==null)
			return;
		Transform sparks = Instantiate(_sparks,transform);
		sparks.SetParent(null);
		Vector3 eulers=sparks.eulerAngles;
		eulers.z=0;
		sparks.eulerAngles=eulers;
		sparks.localScale=Vector3.one;
		Sfx.PlayOneShot3D(_sparkClip,transform.position);
	}

	IEnumerator OpenDoors(float dir){
		_windowOpen=dir>0;
		Transform right=transform.Find("WindowRight");
		Transform left=transform.Find("WindowLeft");
		Material doorRight=right.GetComponent<Renderer>().material;
		Material doorLeft=left.GetComponent<Renderer>().material;
		doorRight.SetFloat("_Lerp",dir>0?1:0);
		doorLeft.SetFloat("_Lerp",dir>0?1:0);

		yield return null;
		if(_seedCounter<_seedCount||_windowIsDoor){
			if(dir>0)
				Sfx.PlayOneShot3D(_doorOpenSound,left.position);
			else
				Sfx.PlayOneShot3D(_doorCloseSound,left.position);

			if(dir>0)
			{
				if(_seedCounter<_seedCount){
					DropSeed();
				}
			}

			yield return new WaitForSeconds(_doorOpenDelay);
			float timer=0;
			float dur=_doorOpenTime;
			//float rotateRate=_doorOpenAngle/dur;
			
			Quaternion leftStartRot=left.rotation;
			Quaternion leftEndRot=_windowOpen? _doorLeftOpened : _doorLeftClosed;
			Quaternion rightStartRot=right.rotation;
			Quaternion rightEndRot=_windowOpen? _doorRightOpened : _doorRightClosed;

			while(timer<dur){
				timer+=Time.deltaTime;
				float frac=_windowOpenCurve.Evaluate(timer/dur);
				//left.Rotate(Vector3.up*Time.deltaTime*rotateRate*dir);
				//right.Rotate(-Vector3.up*Time.deltaTime*rotateRate*dir);
				left.rotation=Quaternion.Slerp(leftStartRot,leftEndRot,frac);
				right.rotation=Quaternion.Slerp(rightStartRot,rightEndRot,frac);
				yield return null;
			}
		}
	}

	void DropSeed(){
		Transform hole = transform.Find("WindowHole");
		_seedCounter++;
	}

	public void Activate(bool active){
		if(!_init)
			Init();
		if(_window){
			Transform right=transform.Find("WindowRight");
			Transform left=transform.Find("WindowLeft");
			Material doorRight=right.GetComponent<Renderer>().material;
			Material doorLeft=left.GetComponent<Renderer>().material;
			doorRight.SetFloat("_OutlineThickness",active?0.044f : 0.5f);
			doorLeft.SetFloat("_OutlineThickness",active?0.044f : 0.5f);
		}
	}
}
