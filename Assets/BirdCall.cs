using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdCall : MonoBehaviour
{
	public Vector2 _delayRange;
	public Vector2 _innerDelayRange;
	public AudioClip _sound;
	public float _interruptDur;
	public int _repsMin;
	public int _repsMax;
	float _interruptTimer;
	float _timer;
	float _delay;
	BirdCall[] _birdCalls;
	int _state;
	int _repetitions;
	int _reps;
	public float _territoryRange;

    // Start is called before the first frame update
    void Start()
    {
		_delay = Random.Range(_delayRange.x,_delayRange.y);
		_birdCalls=FindObjectsOfType<BirdCall>();
		_timer=Random.Range(0,_delay);
    }

    // Update is called once per frame
    void Update()
    {
		if(_interruptTimer>0){
			//make sure not interrupted
			_interruptTimer-=Time.deltaTime;
		}
		else{
			switch(_state){
				case 0://waiting to sing
					_timer+=Time.deltaTime;
					if(_timer>_delay){
						Sfx.PlayOneShot3D(_sound,transform.position);
						_timer=0;
						foreach(BirdCall bc in _birdCalls)
							bc.SetInterrupt(this,_interruptDur);
						if(_repsMax>0){
							_state=1;
							_repetitions=Random.Range(_repsMin,_repsMax+1);
							_reps=1;//start reps at 1 cuz already played one time
							_delay = Random.Range(_innerDelayRange.x,_innerDelayRange.y);
						}
						else{
							//some birds don't repeat, and go right back to state 0
							//particularly the pace-keeping birds chips of a cardinal or sparrow
							_delay = Random.Range(_delayRange.x,_delayRange.y);
						}
					}
					break;
				case 1://singing / repetitions
					_timer+=Time.deltaTime;
					if(_timer>_delay){
						Sfx.PlayOneShot3D(_sound,transform.position);
						foreach(BirdCall bc in _birdCalls)
							bc.SetInterrupt(this,_interruptDur);
						_timer=0;
						_reps++;
						if(_reps>=_repetitions){
							_state=0;
							_delay = Random.Range(_delayRange.x,_delayRange.y);
						}
						else{
							_delay = Random.Range(_innerDelayRange.x,_innerDelayRange.y);
						}

					}
					break;
				default:
					break;
			}
		}
    }

	public void SetInterrupt(BirdCall bc, float f){
		if(bc!=this)
			_interruptTimer=f;
	}

	void OnTriggerEnter(Collider other){
		Debug.Log("Entered "+name);
		_timer=_delay;
	}

	void OnTriggerExit(Collider other){
		Debug.Log("Exit "+name);
		_timer=0;
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.red;
		Gizmos.DrawWireSphere(transform.position,_territoryRange);
	}
}
