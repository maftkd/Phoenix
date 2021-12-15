using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
	protected AudioSource _audio;
	public AudioClip _chalp;
	public float _nurishment;
	ParticleSystem _parts;
    // Start is called before the first frame update
    protected virtual void Start()
    {
		_audio=GetComponent<AudioSource>();
		_parts=GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }

	public virtual void GetEaten(){
		_audio.clip=_chalp;
		_audio.pitch=1f;
		_audio.Play();
		_parts.Play();
		GetComponent<Collider>().enabled=false;
		GetComponent<MeshRenderer>().enabled=false;
		enabled=false;
	}

	void OnTriggerEnter(Collider other){
		if(other.GetComponent<Bird>()!=null)
		{
			other.GetComponent<Bird>().Eat(_nurishment);
			GetEaten();
		}
	}

	void OnCollisioniggerEnter(Collision other){
		if(other.transform.GetComponent<Bird>()!=null)
		{
			other.transform.GetComponent<Bird>().Eat(_nurishment);
			GetEaten();
		}
	}
}
