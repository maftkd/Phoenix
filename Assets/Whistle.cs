using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Whistle : MonoBehaviour
{
	ParticleSystem _parts;
	AudioSource _source;
	public int _sampleRate;
	public float _frequency;
	int _position;
	public float _scale;

	public float _attack;
	public AnimationCurve _attackCurve;
	IEnumerator _attackRoutine;

	public float _release;
	public AnimationCurve _releaseCurve;
	IEnumerator _releaseRoutine;

	void Awake(){
		_source=GetComponent<AudioSource>();
		_parts=GetComponent<ParticleSystem>();
		_source.clip=AudioClip.Create("Whistle",_sampleRate,1,_sampleRate,true,OnAudioRead,OnAudioSetPosition);
	}
    // Start is called before the first frame update
    void Start()
    {
        
    }

	public void Play(){
		_parts.Play();
		if(_releaseRoutine!=null)
			StopCoroutine(_releaseRoutine);
		_attackRoutine = Attack();
		StartCoroutine(_attackRoutine);
	}

	IEnumerator Attack(){
		float startVol=_source.volume;
		if(!_source.isPlaying)
			_source.Play();
		float timer=0;
		while(timer<_attack){
			timer+=Time.deltaTime;
			float frac=timer/_attack;
			_source.volume=Mathf.Lerp(startVol,1f,_attackCurve.Evaluate(frac));
			yield return null;
		}
		_source.volume=1f;
	}

	public void Stop(){
		_parts.Stop();
		if(_attackRoutine!=null)
			StopCoroutine(_attackRoutine);
		_releaseRoutine=Release();
		StartCoroutine(_releaseRoutine);
	}

	IEnumerator Release(){
		float timer=0;
		float startVol=_source.volume;
		while(timer<_release){
			timer+=Time.deltaTime;
			float frac=timer/_release;
			_source.volume=Mathf.Lerp(0,startVol,_releaseCurve.Evaluate(frac));
			yield return null;
		}
		_source.volume=0f;
		//yield return null;
		yield return new WaitForSeconds(0.1f);
		_source.Stop();
		//_source.Pause();
	}

	void OnAudioRead(float[] data){
		int count = 0;
        while (count < data.Length)
        {
			float t=(float)_position/_sampleRate;

			float val = Mathf.Sin(2 * Mathf.PI * (_frequency) * t);
            data[count] = val*_scale;

            _position++;
            count++;
			//_time+=1f/_sampleRate;
        }
	}

	void OnAudioSetPosition(int pos){
		_position=pos;
	}
}
