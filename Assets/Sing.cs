using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sing : MonoBehaviour
{
	MInput _mIn;
	Animator _anim;
	int _state;
	Whistle _low;
	Whistle _mid;
	Whistle _high;

	void Awake(){
		_mIn=GameManager._mIn;
		_low=transform.Find("Low").GetComponent<Whistle>();
		_mid=transform.Find("Mid").GetComponent<Whistle>();
		_high=transform.Find("High").GetComponent<Whistle>();
		
		_anim=transform.GetComponentInParent<Animator>();

	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		switch(_state){
			case 0://not singing
				if(_mIn.GetLowDown()){
					_low.Play();
					_anim.SetFloat("pitch",0.333f);
					_state=1;
				}
				else if(_mIn.GetMidDown()){
					_mid.Play();
					_anim.SetFloat("pitch",0.666f);
					_state=2;
				}
				else if(_mIn.GetHighDown()){
					_high.Play();
					_anim.SetFloat("pitch",1f);
					_state=3;
				}
				break;
			case 1://sing low
				if(_mIn.GetMidDown()){
					_low.Stop();
					_mid.Play();
					_anim.SetFloat("pitch",0.666f);
					_state=2;
				}
				else if(_mIn.GetHighDown()){
					_low.Stop();
					_high.Play();
					_anim.SetFloat("pitch",1f);
					_state=3;
				}
				else if(_mIn.GetLowUp()){
					_low.Stop();
					_state=0;
					_anim.SetFloat("pitch",0f);
				}
				break;
			case 2://sing mid
				if(_mIn.GetLowDown()){
					_mid.Stop();
					_low.Play();
					_anim.SetFloat("pitch",0.333f);
					_state=1;
				}
				else if(_mIn.GetHighDown()){
					_mid.Stop();
					_high.Play();
					_anim.SetFloat("pitch",1f);
					_state=3;
				}
				else if(_mIn.GetMidUp()){
					_mid.Stop();
					_state=0;
					_anim.SetFloat("pitch",0f);
				}
				break;
			case 3://sing high
				if(_mIn.GetLowDown()){
					_high.Stop();
					_low.Play();
					_anim.SetFloat("pitch",0.333f);
					_state=1;
				}
				else if(_mIn.GetMidDown()){
					_high.Stop();
					_mid.Play();
					_anim.SetFloat("pitch",0.666f);
					_state=2;
				}
				else if(_mIn.GetHighUp()){
					_high.Stop();
					_state=0;
					_anim.SetFloat("pitch",0f);
				}
				break;
			default:
				break;
		}
    }
}
