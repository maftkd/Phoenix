using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hawk : MonoBehaviour
{
	Vector3 _center;
	public float _radius;
	public float _turnSpeed;

	void Awake(){
		float theta = Random.value*Mathf.PI*2f;
		Vector3 pos = new Vector3(Mathf.Cos(theta),0,Mathf.Sin(theta));
		transform.position=_center+pos*_radius;
		transform.forward=Quaternion.Euler(0,90,0)*pos;
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		transform.RotateAround(_center,Vector3.up,_turnSpeed*Time.deltaTime);
    }
}
