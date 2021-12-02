using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footstep : MonoBehaviour
{
	public AudioClip [] _clips;
	AudioSource [] _sources;
    // Start is called before the first frame update
    void Start()
    {
		_sources = transform.GetComponentsInChildren<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void Sound(Vector3 pos){
		foreach(AudioSource s in _sources){
			if(!s.isPlaying){
				s.transform.position=pos;
				s.clip=_clips[Random.Range(0,_clips.Length)];
				s.Play();
				return;
			}
		}
	}
}
