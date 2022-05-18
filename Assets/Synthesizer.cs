using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Synthesizer : MonoBehaviour
{
	public static int _sampleRate = 44100;

	void Awake(){
	}

	public static AudioClip GenerateSimpleWave(float frequency, float dur, float noise){
		int numSamples=Mathf.FloorToInt(dur*_sampleRate);
		float [] samples = new float[numSamples];
		for(int i=0;i<numSamples; i++){
			float t01 = i/(float)numSamples;
			float t = t01*dur;
			//float n = Mathf.Lerp(minNoise,noise,Mathf.Abs(Mathf.Cos(t*noiseFreq*Mathf.PI*2)));
			//n=2*n*Random.value-1f;
			//n=2*n-1f;
			float n = (Random.value*2-1f)*noise;
			samples[i]=Mathf.Sin(t*frequency*Mathf.PI*2)+n;
		}
		AudioClip foo = AudioClip.Create("synth",numSamples,1,_sampleRate,false);
		foo.SetData(samples,0);
		return foo;
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

	public static AudioClip GenerateSomethingHarmonic(float frequency, float dur, float noise, int harmonics=0,float harmonicFactor=1f){
		int numSamples=Mathf.FloorToInt(dur*_sampleRate);
		float [] samples = new float[numSamples];
		float max=0;
		for(int i=0;i<numSamples; i++){
			float t01 = i/(float)numSamples;
			float t = t01*dur;
			float n=noise*(2*Random.value-1f);
			samples[i]=Mathf.Sin(t*frequency*Mathf.PI*2);//+n;
			for(int j=1;j<=harmonics;j++){
				float amount=harmonicFactor;
				//samples[i]+=Mathf.Sin(t*(frequency+j*100)*Mathf.PI*2)*amount;//+n;
				samples[i]+=Mathf.Sin(t*(frequency*(j+1))*Mathf.PI*2)*amount;//+n;
			}
			samples[i]+=n;
			if(Mathf.Abs(samples[i])>max)
				max=Mathf.Abs(samples[i]);
		}
		//normalize
		if(max>1f){
			float factor=1f/max;
			for(int i=0;i<numSamples; i++)
				samples[i]*=factor;
		}

		AudioClip foo = AudioClip.Create("synth",numSamples,1,_sampleRate,false);
		foo.SetData(samples,0);
		return foo;
	}

	public static AudioClip GenerateSomethingAm(float frequency,float ampFrequency, float dur, float noise){
		int numSamples=Mathf.FloorToInt(dur*_sampleRate);
		float [] samples = new float[numSamples];
		float max=0;
		for(int i=0;i<numSamples; i++){
			float t01 = i/(float)numSamples;
			float t = t01*dur;
			float n=noise*(2*Random.value-1f);
			float ampMod=Mathf.Cos(t*ampFrequency*Mathf.PI*2);
			samples[i]=Mathf.Sin(t*frequency*Mathf.PI*2)*ampMod;//+n;

			samples[i]+=n;
			if(Mathf.Abs(samples[i])>max)
				max=Mathf.Abs(samples[i]);
		}
		//normalize
		if(max>1f){
			float factor=1f/max;
			for(int i=0;i<numSamples; i++)
				samples[i]*=factor;
		}

		AudioClip foo = AudioClip.Create("synth",numSamples,1,_sampleRate,false);
		foo.SetData(samples,0);
		return foo;
	}

	public static AudioClip GenerateSomethingFm(float frequency,float frequencyB, float dur, float noise){
		int numSamples=Mathf.FloorToInt(dur*_sampleRate);
		float [] samples = new float[numSamples];
		float max=0;
		for(int i=0;i<numSamples; i++){
			float t01 = i/(float)numSamples;
			float t = t01*dur;
			float n=noise*(2*Random.value-1f);
			float freqMod=Mathf.Cos(t*frequencyB*Mathf.PI*2);
			samples[i]=Mathf.Sin(t*frequency*freqMod*Mathf.PI*2);//+n;

			samples[i]+=n;
			if(Mathf.Abs(samples[i])>max)
				max=Mathf.Abs(samples[i]);
		}
		//normalize
		if(max>1f){
			float factor=1f/max;
			for(int i=0;i<numSamples; i++)
				samples[i]*=factor;
		}

		AudioClip foo = AudioClip.Create("synth",numSamples,1,_sampleRate,false);
		foo.SetData(samples,0);
		return foo;
	}

	public static AudioClip GenerateSomethingDetune(float frequency, float dur, float noise, int voices=0,float detune=0){
		int numSamples=Mathf.FloorToInt(dur*_sampleRate);
		float [] samples = new float[numSamples];
		float max=0;
		float [] voiceFreqs = new float[voices];
		for(int i=0;i<voices; i++){
			voiceFreqs[i]=frequency+(Random.value*2f-1)*detune;
		}
		for(int i=0;i<numSamples; i++){
			float t01 = i/(float)numSamples;
			float t = t01*dur;
			float n=noise*(2*Random.value-1f);
			samples[i]=Mathf.Sin(t*frequency*Mathf.PI*2);//+n;
			for(int j=0;j<voices;j++){
				float amount=0.5f;
				//float amount=1f/(j+2);
				samples[i]+=Mathf.Sin(t*voiceFreqs[j]*Mathf.PI*2)*amount;//+n;
			}
			//average voices
			//samples[i]/=(voices+1);
			samples[i]+=n;
			if(Mathf.Abs(samples[i])>max)
				max=Mathf.Abs(samples[i]);
		}
		//normalize
		if(max>1f){
			float factor=1f/max;
			for(int i=0;i<numSamples; i++)
				samples[i]*=factor;
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
}
