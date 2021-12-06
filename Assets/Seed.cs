using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seed : MonoBehaviour
{
	public float _range;
	Transform _mainCam;
    // Start is called before the first frame update
    void Start()
    {
		_mainCam=Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	void OnMouseEnter(){
		if((transform.position-_mainCam.position).sqrMagnitude<_range*_range)
			Crosshair._instance.SetOverItem(name);
	}

	void OnMouseExit(){
		Crosshair._instance.ClearOverItem(name);

	}

	void OnMouseDown(){
		Peck._instance.Pickup(transform);
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.red;
		Gizmos.DrawWireSphere(transform.position,_range);
	}
}
