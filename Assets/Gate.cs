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

	bool _charging;
	public float _chargeDur;
	float _chargeTimer;

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
    }

	void CheckGate(){
		bool powered=true;
		for(int i=0; i<_inputs.Length; i++){
			bool inputPowered=_inputs[i].Powered();
			if(!inputPowered)
				powered=false;
			//color the input arrows
			Transform inT = transform.GetChild(i);
			Renderer r = inT.GetComponent<Renderer>();
			Material[] mats = r.materials;
			mats[0]=inputPowered?_on :_off;
			r.materials=mats;
		}
		_charging=powered;
		if(_chargeDur>0)
			powered=powered || _chargeTimer>0;//note: charge timer = 0 the instant it's activated
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
			if(_chargeDur==0)
				enabled=false;
			_onGateActivated.Invoke();
		}
	}
}
