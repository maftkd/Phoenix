using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Fireplace : MonoBehaviour
{
	ParticleSystem _parts;
	AudioSource _audio;
	public Material _thermoMat;
	public float _fillRate;
	public float _fill;
	bool _lit;
	public UnityEvent _onHeat;

	void Awake()
	{
		_parts=transform.GetComponentInChildren<ParticleSystem>();
		_audio=_parts.GetComponent<AudioSource>();
		_thermoMat.SetFloat("_FillAmount",_fill);
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if(_lit&&_fill<1){
			_fill+=Time.deltaTime*_fillRate;
			if(_fill>1){
				_fill=1;
			}
			_onHeat.Invoke();
			_thermoMat.SetFloat("_FillAmount",_fill);
		}
		else if(!_lit&&_fill>0){
			_fill-=Time.deltaTime*_fillRate;
			if(_fill<0){
				_fill=0;
			}
			_onHeat.Invoke();
			_thermoMat.SetFloat("_FillAmount",_fill);
		}
    }

	public void Light(bool lit){
		if(lit)
		{
			_parts.Play();
			_audio.Play();
		}
		else
		{
			_parts.Stop();
			_audio.Stop();
		}
		_lit=lit;
	}
}
