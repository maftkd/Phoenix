using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour
{
	public AudioClip _clip;
	[Range(0,1)]
	public float _vol;
	public static float _audioTimer;
	public static Grass _timeKeeper;
	public float _minAudioPeriod;
	public Vector2 _pitchRange;

	void Awake(){
		if(_timeKeeper==null)
			_timeKeeper=this;
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if(this==_timeKeeper)
			_audioTimer+=Time.deltaTime;
    }

	public void Collide(){
		if(_audioTimer>_minAudioPeriod){
			Sfx.PlayOneShot3D(_clip,transform.position,Random.Range(_pitchRange.x,_pitchRange.y),_vol);
			_audioTimer=0;
		}
	}
}
