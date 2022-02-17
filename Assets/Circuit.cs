using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Circuit : MonoBehaviour
{
	Material _mat;

	public delegate void CircuitEvent();
	public event CircuitEvent _powerChange;

	bool _powered;
	public UnityEvent _onPowered;
	public UnityEvent _onPoweredOff;

	void Awake(){
		_mat=GetComponent<Renderer>().material;
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void Power(bool on){
		_powered=on;
		_mat.SetFloat("_Lerp", on? 1f : 0f);
		if(_powerChange!=null)
		{
			_powerChange.Invoke();
		}
		if(_powered)
			_onPowered.Invoke();
		else
			_onPoweredOff.Invoke();
	}

	public bool Powered(){
		return _powered;
	}
}
