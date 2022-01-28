using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Charger : MonoBehaviour
{
	public Circuit _input;
	public Circuit [] _output;
	bool _charging;
	public float _chargeDur;
	float _chargeTimer;
	Material _mat;
	bool _powered;

	void Awake(){
		_input._powerChange+=CheckCharger;
		_mat=GetComponent<Renderer>().material;
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if(_charging && _chargeTimer<_chargeDur){
			_chargeTimer+=Time.deltaTime;
			if(_chargeTimer>_chargeDur)
				_chargeTimer=_chargeDur;
			_mat.SetFloat("_FillAmount", _chargeTimer/_chargeDur);
		}
		else if(!_charging && _chargeTimer>0){
			_chargeTimer-=Time.deltaTime;
			if(_chargeTimer<0)
			{
				_chargeTimer=0;
				SetPower(false);
			}
			_mat.SetFloat("_FillAmount", _chargeTimer/_chargeDur);
		}
    }

	public void CheckCharger(){
		_charging=_input.Powered();
		if(_charging)
			SetPower(true);
	}

	void SetPower(bool b){
		foreach(Circuit c in _output)
			c.Power(b);
	}
}
