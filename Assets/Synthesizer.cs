using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Synthesizer : MonoBehaviour
{
	public float _frequency;
	public float _duration;
	public static int _sampleRate = 44100;
	[HideInInspector]
	public AudioClip _myClip;
	public UnityEvent _onGenerate;
	public Transform _musicParts;

	void Awake(){
		int numSamples=Mathf.FloorToInt(_duration*_sampleRate);
		float [] samples = new float[numSamples];
		for(int i=0;i<numSamples; i++){
			float t01 = i/(float)numSamples;
			float t = t01*_duration;
			samples[i]=Mathf.Sin(t*_frequency*Mathf.PI*2);
		}
		_myClip = AudioClip.Create("synth",numSamples,1,_sampleRate,false);
		_myClip.SetData(samples,0);
		_onGenerate.Invoke();
	}

	public static AudioClip GenerateSineWave(float frequency, float dur, float noise, float noiseFreq=0,float minNoise=0){
		int numSamples=Mathf.FloorToInt(dur*_sampleRate);
		float [] samples = new float[numSamples];
		for(int i=0;i<numSamples; i++){
			float t01 = i/(float)numSamples;
			float t = t01*dur;
			float n = Mathf.Lerp(minNoise,noise,Mathf.Abs(Mathf.Cos(t*noiseFreq*Mathf.PI*2)));
			n=2*n*Random.value-1f;
			//n=2*n-1f;
			samples[i]=Mathf.Sin(t*frequency*Mathf.PI*2)+n;
		}
		AudioClip foo = AudioClip.Create("synth",numSamples,1,_sampleRate,false);
		foo.SetData(samples,0);
		return foo;
	}

	public static AudioClip GenerateSquareWave(float frequency, float dur, float noise){
		int numSamples=Mathf.FloorToInt(dur*_sampleRate);
		float [] samples = new float[numSamples];
		for(int i=0;i<numSamples; i++){
			float t01 = i/(float)numSamples;
			float t = t01*dur;
			samples[i]=Mathf.Sin(t*frequency*Mathf.PI*2)+(Random.value*2-1)*noise;
			if(samples[i]>0)
				samples[i]=1f;
			else
				samples[i]=-1f;
		}
		AudioClip foo = AudioClip.Create("synth",numSamples,1,_sampleRate,false);
		foo.SetData(samples,0);
		return foo;
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void PlayParticles(){
		Transform parts = Instantiate(_musicParts,transform.position,Quaternion.identity);
		ParticleSystem ps = parts.GetComponent<ParticleSystem>();
		var main = ps.main;
		main.startColor=GetComponent<MeshRenderer>().material.color;
	}
}
