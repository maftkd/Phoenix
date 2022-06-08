using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Sfx : MonoBehaviour
{
	public static Sfx _instance;
	public AudioMixer _mixer;
	public AnimationCurve _fallOff;

	void Awake(){
		_instance=this;
	}
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
		Destroy(foo,clip.length/pitch);
	}

	public static void PlayOneShot2DVol(AudioClip clip, float volume){
		GameObject foo = new GameObject("one-shot audio");
		AudioSource audio = foo.AddComponent<AudioSource>();
		audio.volume=volume;
		audio.clip=clip;
		audio.Play();
		Destroy(foo,clip.length);
	}

	public static void PlayOneShot2D(AudioClip clip, float pitch, float vol){
		GameObject foo = new GameObject("one-shot audio");
		AudioSource audio = foo.AddComponent<AudioSource>();
		audio.pitch=pitch;
		audio.volume=vol;
		audio.clip=clip;
		audio.Play();
		Destroy(foo,clip.length/pitch);
	}

	public static AudioSource PlayOneShot3D(AudioClip clip,Vector3 pos){
		GameObject foo = new GameObject("one-shot audio");
		foo.transform.position=pos;
		AudioSource audio = foo.AddComponent<AudioSource>();
		audio.spatialBlend=1f;
		//audio.maxDistance=100f;
		audio.clip=clip;
		audio.Play();
		Destroy(foo,clip.length);
		return audio;
	}
	public static AudioSource PlayOneShot3D(AudioClip clip,Vector3 pos,float pitch){
		GameObject foo = new GameObject("one-shot audio");
		foo.transform.position=pos;
		AudioSource audio = foo.AddComponent<AudioSource>();
		audio.spatialBlend=1f;
		audio.pitch=pitch;
		audio.clip=clip;
		audio.Play();
		//audio.rolloffMode=AudioRolloffMode.Linear;
		//audio.maxDistance=15f;
		audio.rolloffMode=AudioRolloffMode.Custom;
		audio.SetCustomCurve(AudioSourceCurveType.CustomRolloff,_instance._fallOff);
		audio.maxDistance=25f;

		Destroy(foo,clip.length/pitch);
		return audio;
	}

	public static AudioSource PlayOneShot3D(AudioClip clip,Vector3 pos,float pitch, string mixerGroupName=""){
		GameObject foo = new GameObject("one-shot audio");
		foo.transform.position=pos;
		AudioSource audio = foo.AddComponent<AudioSource>();
		audio.spatialBlend=1f;
		audio.pitch=pitch;
		audio.clip=clip;
		audio.Play();
		if(mixerGroupName!="")
			audio.outputAudioMixerGroup = _instance._mixer.FindMatchingGroups(mixerGroupName)[0];
		//audio.rolloffMode=AudioRolloffMode.Linear;
		//audio.maxDistance=10f;

		Destroy(foo,clip.length/pitch);
		return audio;
	}

	public static void PlayOneShot3DVol(AudioClip clip,Vector3 pos,float volume){
		GameObject foo = new GameObject("one-shot audio");
		foo.transform.position=pos;
		AudioSource audio = foo.AddComponent<AudioSource>();
		audio.spatialBlend=1f;
		audio.volume=volume;
		audio.clip=clip;
		audio.Play();
		Destroy(foo,clip.length);
	}

	public static void PlayOneShot3D(AudioClip clip,Vector3 pos,float pitch, float vol,string mixerGroupName=""){
		GameObject foo = new GameObject("one-shot audio");
		foo.transform.position=pos;
		AudioSource audio = foo.AddComponent<AudioSource>();
		audio.spatialBlend=1f;
		audio.pitch=pitch;
		audio.volume=vol;
		audio.clip=clip;
		audio.Play();
		if(mixerGroupName!="")
			audio.outputAudioMixerGroup = _instance._mixer.FindMatchingGroups(mixerGroupName)[0];
		Destroy(foo,clip.length/pitch);
	}

	public static void PauseBg(){
		_instance.transform.GetChild(2).GetComponent<AudioSource>().Pause();
	}

	public static void PlayBg(){
		_instance.transform.GetChild(2).GetComponent<AudioSource>().Play();
	}

	public static void SetFloat(string n, float f){
		_instance._mixer.SetFloat(n,f);
	}
}
