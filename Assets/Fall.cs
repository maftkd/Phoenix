using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fall : MonoBehaviour
{
	float _velocity;
	public float _initialVel;
	public float _gravity;
	public float _fallTime;
	float _fallTimer;
    // Start is called before the first frame update
    void Start()
    {
		_velocity=_initialVel;
    }

    // Update is called once per frame
    void Update()
    {
		_velocity+=_gravity*Time.deltaTime;
		/*
		foreach(Transform t in transform){
			t.position+=Vector3.down*_velocity*Time.deltaTime;
		}
		*/
		transform.position+=Vector3.down*_velocity*Time.deltaTime;
		_fallTimer+=Time.deltaTime;
		if(_fallTimer>=_fallTime){
			GameManager._instance.NextLevel();
		}
    }
}
