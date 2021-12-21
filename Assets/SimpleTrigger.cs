using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SimpleTrigger : MonoBehaviour
{
	public UnityEvent _onTriggerEnter;
	public UnityEvent _onTriggerExit;
	public UnityEvent _onCollisionEnter;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	void OnTriggerEnter(Collider other){
		_onTriggerEnter.Invoke();
	}
	void OnTriggerExit(Collider other){
		_onTriggerExit.Invoke();
	}

	void OnCollisionEnter(Collision other){
		_onCollisionEnter.Invoke();
	}
}
