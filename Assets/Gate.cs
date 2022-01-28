using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Gate : MonoBehaviour
{
	public Circuit [] _inputs;
	public Circuit _output;
	public UnityEvent _onGateActivated;
	Material _mat;

	void Awake(){
		foreach(Circuit c in _inputs){
			c._powerChange+=CheckGate;
		}
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

	void CheckGate(){
		if(!enabled)
			return;
		int powered=0;
		foreach(Circuit c in _inputs){
			if(c.Powered())
				powered++;
		}
		_mat.SetFloat("_FillAmount",powered/(float)(_inputs.Length));
		if(powered>=_inputs.Length){
			Debug.Log("We got power");
			_output.Power(true);
			enabled=false;
			_onGateActivated.Invoke();
		}
	}
}
