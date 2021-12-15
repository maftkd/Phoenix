using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fruit : Food
{
	[Header("Rolling")]
	public AudioClip _roll;
	public float _rollPitchMult;
	public float _minRollSpeed;
	public bool _rolling;
	Rigidbody _rb;
    // Start is called before the first frame update
    protected override void Start()
    {
		base.Start();
		if(_rolling)
		{
			_audio.clip=_roll;
			_audio.Play();
			_rb=GetComponent<Rigidbody>();
		}

    }

    // Update is called once per frame
    protected override void Update()
    {
		if(_rolling){
			if(!_audio.isPlaying){
				//_audio.pitch=Random.Range(_minRollPitch,_maxRollPitch);
				_audio.pitch=_rb.velocity.sqrMagnitude*_rollPitchMult;//MatRandom.Range(_minRollPitch,_maxRollPitch);

				_audio.Play();
				if(_rb.velocity.sqrMagnitude<_minRollSpeed*_minRollSpeed)
					_rolling=false;
			}
		}
    }

	public override void GetEaten(){
		_rolling=false;
		if(_rb!=null)
		{
			_rb.isKinematic=true;
		}
		base.GetEaten();
	}
}
