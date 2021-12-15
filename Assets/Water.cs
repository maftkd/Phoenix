using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
	Transform _bird;
	AudioSource _splash;
	AudioSource _bubbles;
	AudioSource _sloosh;
	public float _splashEnterPitch;
	public float _splashExitPitch;
	ParticleSystem _bubbleParts;
    // Start is called before the first frame update
    void Start()
    {
		_splash=transform.Find("Splash").GetComponent<AudioSource>();
		_bubbles=transform.Find("Bubbles").GetComponent<AudioSource>();
		_sloosh=transform.Find("Sloosh").GetComponent<AudioSource>();
		_bubbleParts=transform.Find("Bubbles").GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
		if(_bird!=null)
			_bubbles.transform.position=_bird.position;
    }

	public void Splash(Transform t){
		_bird=t;
		_splash.transform.position=_bird.position;
		_splash.pitch=_splashEnterPitch;
		_splash.Play();
		_bubbles.Play();
		_bubbleParts.Play();
	}

	public void Unsplash(){
		_splash.transform.position=_bird.position;
		_splash.pitch=_splashExitPitch;
		_splash.Play();
		_bird=null;
		_bubbles.Stop();
		_bubbleParts.Stop();
	}

	public void Sloosh(){
		if(!_sloosh.isPlaying)
			_sloosh.Play();
	}
}
