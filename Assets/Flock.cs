using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour
{
	public Transform _birdPrefab;
	public float _minScale;
	public float _maxScale;
	public int _numBirds;
	public float _radius;
	public Vector3 _velocity;
	float [] _phaseShifts;
	float [] _yOffsets;
	float [] _frequencies;
	float [] _amplitudes;
	float _timer;
	public float _freqMultiplier;
	//public float _minFreq;
	//public float _maxFreq;
	public float _minAmp;
	public float _maxAmp;
	[Header("limits")]
	public Vector3 _limits;
	public bool _repeat;
	Vector3 _startPos;

    // Start is called before the first frame update
    void Start()
    {
		_startPos=transform.position;
		_phaseShifts = new float[_numBirds];
		_yOffsets = new float[_numBirds];
		_frequencies = new float[_numBirds];
		_amplitudes = new float[_numBirds];
		for(int i=0;i<_numBirds;i++){
			Vector3 pos = Random.insideUnitCircle;
			Transform bird = Instantiate(_birdPrefab,transform);
			bird.localScale=Vector3.one*Random.Range(_minScale,_maxScale);
			bird.localPosition=pos*_radius;
			bird.eulerAngles=Vector3.back*45f;
			_phaseShifts[i]=Random.value*Mathf.PI*2f;
			_yOffsets[i]=bird.localPosition.y;
			//_frequencies[i]=Random.Range(_minFreq,_maxFreq);
			_frequencies[i]=bird.localScale.x*_freqMultiplier;
			_amplitudes[i]=Random.Range(_minAmp,_maxAmp);
		}
    }

    // Update is called once per frame
    void Update()
    {
		_timer+=Time.deltaTime;
		transform.position+=Vector3.right*Time.deltaTime*_velocity.x;
		for(int i=0; i<transform.childCount; i++){
			Transform bird = transform.GetChild(i);
			Vector3 pos = bird.localPosition;
			pos.y=_amplitudes[i]*Mathf.Sin((_timer*Mathf.PI*2f+_phaseShifts[i])*_frequencies[i])+_yOffsets[i];
			bird.localPosition=pos;
		}
		if(_velocity.x>0&&transform.position.x>_limits.x){
			if(_repeat){
				transform.position=_startPos;
			}
			else{
				Destroy(gameObject);
			}
		}
    }

	void OnDrawGizmos(){
		Gizmos.color=Color.blue;
		Gizmos.DrawWireSphere(transform.position,_radius);
	}
}
