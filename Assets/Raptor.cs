using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raptor : MonoBehaviour
{
	public float _height;
	public float _radius;
	public float _angVel;
	Vector3 _center;
	public bool _clockwise;

	void Awake(){
		_center=transform.position;
		_center.y=_height;
		float theta=Random.value*Mathf.PI*2f;
		Vector3 pos=_center;
		pos.x+=Mathf.Cos(theta)*_radius;
		pos.z+=Mathf.Sin(theta)*_radius;
		transform.position=pos;
		Vector3 offset=pos-_center;
		//if(_clockwise)
			transform.forward=Quaternion.Euler(0,90,0)*offset;

	}
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		transform.RotateAround(_center,Vector3.up,_angVel*Time.deltaTime);
        
    }

	void OnDrawGizmos(){
		Gizmos.color=Color.red;
		Gizmos.DrawWireSphere(_center,_radius);
	}
}
