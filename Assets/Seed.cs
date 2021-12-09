using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seed : MonoBehaviour
{
	public float _range;
	Transform _mainCam;
	bool _carried;
	bool _inRange;
	SphereCollider _collider;
    // Start is called before the first frame update
    void Start()
    {
		_mainCam=Camera.main.transform;
		_collider=GetComponent<SphereCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	void OnMouseEnter(){
		if(_carried)
			return;
		if((transform.position-_mainCam.position).sqrMagnitude<_range*_range)
		{
			Crosshair._instance.SetOverItem(name);
			_inRange=true;
		}
		else
			_inRange=false;
	}

	void OnMouseExit(){
		if(_carried)
			return;
		Crosshair._instance.ClearOverItem(name);

	}

	void OnMouseDown(){
		if(_carried||!_inRange)
			return;
		Peck._instance.Pickup(transform);
		Crosshair._instance.ClearOverItem(name);
		_carried=true;
		_collider.enabled=false;
	}

	public void Reset(){
		_collider.enabled=true;
		_carried=false;
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.red;
		Gizmos.DrawWireSphere(transform.position,_range);
	}
}
