using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sfx : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public static void PlayOneShot2D(AudioClip clip){
		GameObject foo = new GameObject("one-shot audio");
		AudioSource audio = foo.AddComponent<AudioSource>();
		audio.clip=clip;
		audio.Play();
		Destroy(foo,clip.length);
	}

	public static void PlayOneShot2D(AudioClip clip, float pitch){
		GameObject foo = new GameObject("one-shot audio");
		AudioSource audio = foo.AddComponent<AudioSource>();
		audio.pitch=pitch;
		audio.clip=clip;
		audio.Play();
		Destroy(foo,clip.length);
	}

	public static void PlayOneShot3D(AudioClip clip,Vector3 pos){
		GameObject foo = new GameObject("one-shot audio");
		foo.transform.position=pos;
		AudioSource audio = foo.AddComponent<AudioSource>();
		audio.spatialBlend=1f;
		audio.clip=clip;
		audio.Play();
		Destroy(foo,clip.length);
	}
}
