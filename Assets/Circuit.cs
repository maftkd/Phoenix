using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circuit : MonoBehaviour
{
	Material _mat;

	public delegate void CircuitEvent();
	public event CircuitEvent _powerChange;

	bool _powered;

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
	}

	public bool Powered(){
		return _powered;
	}
}
