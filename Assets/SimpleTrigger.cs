using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SimpleTrigger : MonoBehaviour
{
	public float _activeRadius;
	public UnityEvent _onTriggerEnter;
	public UnityEvent _onTriggerExit;
	public UnityEvent _onTriggerStay;
	public UnityEvent _onCollisionEnter;
	MeshRenderer _mesh;
	Collider _col;
	Transform _mainCam;
	bool _active;
	public bool _ignoreMesh;
	public bool _ignoreCol;

	void Awake(){
		_mesh=GetComponent<MeshRenderer>();
		_col = GetComponent<Collider>();
		_mainCam=Camera.main.transform;
		if((_mainCam.position-transform.position).sqrMagnitude<=_activeRadius*_activeRadius){
			_active=true;
			if(!_ignoreMesh)
				_mesh.enabled=true;
			if(!_ignoreCol)
				_col.enabled=true;
		}
		else{
			_active=false;
			if(!_ignoreMesh)
				_mesh.enabled=false;
			if(!_ignoreCol)
				_col.enabled=false;

		}
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
			if(!_ignoreMesh)
				_mesh.enabled=true;
			if(!_ignoreCol)
				_col.enabled=true;
		}
		else if(_active&&(_mainCam.position-transform.position).sqrMagnitude>_activeRadius*_activeRadius){
			_active=false;
			if(!_ignoreMesh)
				_mesh.enabled=false;
			if(!_ignoreCol)
				_col.enabled=false;
		}
    }

	void OnTriggerEnter(Collider other){
		_onTriggerEnter.Invoke();
	}
	void OnTriggerExit(Collider other){
		_onTriggerExit.Invoke();
	}

	void OnTriggerStay(Collider other){
		_onTriggerStay.Invoke();
	}

	void OnCollisionEnter(Collision other){
		_onCollisionEnter.Invoke();
	}
}
