using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideWhistle : MonoBehaviour
{
	public float _clipDur;
	public float _startFreq;
	public float _endFreq;
	AudioSource _audio;
	int _sampleRate = 44100;
	public Vector3 _velocity;
	public AudioClip _crash;
    // Start is called before the first frame update
    void Start()
    {
		_audio=GetComponent<AudioSource>();
		GenerateClip();
		_audio.Play();
		StartCoroutine(NextLevelAfterAudio());
    }

	void GenerateClip(){
		int numSamples=Mathf.RoundToInt(_sampleRate*_clipDur);
		float[] samples = new float[numSamples];
		for(int i=0;i<numSamples; i++){
			float t = i/(float)numSamples;
			t*=_clipDur;
			samples[i]=Mathf.Sin(t*Mathf.PI*2f*_startFreq);
		}
		AudioClip clip = AudioClip.Create("Foo",numSamples,1,_sampleRate,false);
		clip.SetData(samples,0);
		_audio.clip=clip;
	}

    // Update is called once per frame
    void Update()
    {
		transform.position+=_velocity*Time.deltaTime;
    }
	
	IEnumerator NextLevelAfterAudio(){
		yield return new WaitForSeconds(_clipDur);
		_audio.clip=_crash;
		_audio.volume=1;
		_audio.Play();
		_audio.spatialBlend=0;
		yield return new WaitForSeconds(_audio.clip.length);
		GameManager._instance.NextLevel();
	}
}
