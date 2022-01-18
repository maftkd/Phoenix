using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MBillboard : MonoBehaviour
{
	Transform _cam;
	public bool _reverse;

	void Awake(){
		_cam=Camera.main.transform;
	}
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		transform.LookAt(_cam);
		if(_reverse)
			transform.forward=-transform.forward;
    }
}
