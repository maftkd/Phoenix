using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Worm : MonoBehaviour
{
	public Transform _zone;
	Vector3 _velocity;
	public float _crawlSpeed;

	void OnDisable(){
		GetComponent<AudioSource>().Stop();
	}
    // Start is called before the first frame update
    void Start()
    {
		Vector3 rand = Random.insideUnitSphere;
		//Vector3 pos = _zone.position+_zone.localScale.x*rand.x+_zone.localScale.z*rand.z;
		Vector3 pos = _zone.position;
		pos.x+=_zone.localScale.x*0.5f*rand.x;
		pos.z+=_zone.localScale.z*0.5f*rand.z;
		transform.position=pos;
		_velocity = Random.onUnitSphere;
		_velocity.y=0;
		WormBed._instance.RegisterWorm(this);
    }

    // Update is called once per frame
    void Update()
    {
		transform.position+=_velocity*Time.deltaTime*_crawlSpeed;
		float xDiff = transform.position.x-_zone.position.x;
		if(transform.position.x>_zone.position.x+_zone.localScale.x*0.5f ||
				transform.position.x<_zone.position.x-_zone.localScale.x*0.5f){
			_velocity.x*=-1f;
		}
		if(transform.position.z>_zone.position.z+_zone.localScale.z*0.5f ||
				transform.position.z<_zone.position.z-_zone.localScale.z*0.5f){
			_velocity.z*=-1f;
		}
	}
}
