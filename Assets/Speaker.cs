using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speaker : MonoBehaviour
{
	public AudioClip [] _clips;
	AudioSource _audio;
	AudioSource _static;
	AudioClip _staticClip;
	public static Speaker _instance;

	public class SpeakerEventArgs : System.EventArgs{
		public string name;
		public float dur;
		public Vector3 pos;
	}

	public delegate void EventHandler(SpeakerEventArgs args);
	public event EventHandler OnSpeakerPlay;

	void Awake(){
		_instance=this;
	}
    // Start is called before the first frame update
    void Start()
    {
		_audio=transform.GetChild(0).GetComponent<AudioSource>();
		_static=transform.GetChild(1).GetComponent<AudioSource>();
		GenerateStatic();
    }

	void GenerateStatic(){
		float _dur=1f;
		float _noiseMult=1f;
		int sampleRate = 44100;
		int numSamples = Mathf.RoundToInt(sampleRate*_dur);
		float[] samples = new float[numSamples];
		float t=0;
		float n=0;
		for(int i=0; i<numSamples; i++){
			n =(i/(float)numSamples); 
			t=n*_dur;

			//noise
			samples[i]+=(Random.value-0.5f)*2f*_noiseMult;
		}
		_staticClip = AudioClip.Create("static", numSamples, 1, sampleRate, false);
		_staticClip.SetData(samples,0);
		_static.clip=_staticClip;
	}

    // Update is called once per frame
    void Update()
    {
		if(Input.GetKeyDown(KeyCode.Alpha1)){
			StartCoroutine(PlaybackClipR(_clips[0]));
			SpeakerEventArgs args = new SpeakerEventArgs();
			args.name=_audio.clip.name;
			args.dur=_audio.clip.length;
			args.pos=transform.position;
			if(OnSpeakerPlay!=null)
				OnSpeakerPlay.Invoke(args);
		}
    }

	IEnumerator PlaybackClipR(AudioClip clip){
		_audio.clip=clip;
		_audio.Play();
		_static.Play();
		yield return new WaitForSeconds(clip.length);
		_static.Stop();
	}
}
