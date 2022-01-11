﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SimpleTrigger : MonoBehaviour
{
	public float _activeRadius;
	public UnityEvent _onTriggerEnter;
	public UnityEvent _onTriggerExit;
	public UnityEvent _onCollisionEnter;
	MeshRenderer _mesh;
	Collider _col;
	Transform _mainCam;
	bool _active;

	void Awake(){
		_mesh=GetComponent<MeshRenderer>();
		_col = GetComponent<Collider>();
		_mainCam=Camera.main.transform;
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if(!_active&&(_mainCam.position-transform.position).sqrMagnitude<=_activeRadius*_activeRadius){
			_active=true;
			_mesh.enabled=true;
			_col.enabled=true;
		}
		else if(_active&&(_mainCam.position-transform.position).sqrMagnitude>_activeRadius*_activeRadius){
			_active=false;
			_mesh.enabled=false;
			_col.enabled=false;
		}
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
