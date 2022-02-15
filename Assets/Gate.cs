using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Gate : MonoBehaviour
{
	public Circuit [] _inputs;
	public Circuit [] _outputs;
	public UnityEvent _onGateActivated;
	Material _mat;
	public Material _off, _on;

	public float _chargeDur;
	float _chargeTimer;
	int _chargeState;//0 = idle, 1 = chargeUp, 2 = chargeDown
	bool _powered;
	bool _inputsOn;

	public Transform _sparks;
	public AudioClip _sparkClip;

	public bool _inverter;

	void Awake(){
		foreach(Circuit c in _inputs){
			c._powerChange+=CheckGate;
		}
		_mat=GetComponent<Renderer>().material;
		_mat.SetFloat("_FillAmount",0);
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
						_chargeState=1;
					break;
				case 1://charge up
					if(!_inputsOn){
						_chargeState=2;
						break;
					}
					if(_chargeTimer<_chargeDur){
						_chargeTimer+=Time.deltaTime;
						if(_chargeTimer>_chargeDur)
						{
							_chargeTimer=_chargeDur;
							_powered=true;
							CheckGate();
							MakeSparks();
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
						_chargeTimer-=Time.deltaTime;
						if(_chargeTimer<0)
						{
							_chargeTimer=0;
							_powered=false;
							CheckGate();
						}
						_mat.SetFloat("_FillAmount", _chargeTimer/_chargeDur);
					}
					break;
				case 3://discharge
					break;
			}
		}
		/*
		//charge up
		if(_charging && _chargeTimer<_chargeDur){
			_chargeTimer+=Time.deltaTime;
			if(_chargeTimer>_chargeDur)
			{
				_chargeTimer=_chargeDur;
			}
			_mat.SetFloat("_FillAmount", _chargeTimer/_chargeDur);
		}

		//cool down
		else if(!_charging && _chargeTimer>0){
			_chargeTimer-=Time.deltaTime;
			if(_chargeTimer<0)
			{
				_chargeTimer=0;
				CheckGate();
			}
			_mat.SetFloat("_FillAmount", _chargeTimer/_chargeDur);
		}
		*/
    }

	void CheckGate(){
		bool powered=true;
		for(int i=0; i<_inputs.Length; i++){
			bool inputPowered=_inputs[i].Powered();
			if(_inverter)
				inputPowered=!inputPowered;
			if(!inputPowered)
				powered=false;
			//color the input arrows
			Transform inT = transform.GetChild(i);
			Renderer r = inT.GetComponent<Renderer>();
			Material[] mats = r.materials;
			mats[0]=inputPowered?_on :_off;
			r.materials=mats;
		}
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
		if(_chargeDur==0)
			_mat.SetFloat("_FillAmount",powered?1:0);
		foreach(Circuit c in _outputs){
			c.Power(powered);
		}
		if(powered){
			//charger gate stays active
			//default gate goes to sleep after power
			if(_chargeDur==0&&!_inverter)
			{
				enabled=false;
				MakeSparks();
			}
			_onGateActivated.Invoke();
		}
	}

	void MakeSparks(){
		Transform sparks = Instantiate(_sparks,transform);
		sparks.SetParent(null);
		Vector3 eulers=sparks.eulerAngles;
		eulers.z=0;
		sparks.eulerAngles=eulers;
		sparks.localScale=Vector3.one;
		Sfx.PlayOneShot3D(_sparkClip,transform.position);
	}
}
