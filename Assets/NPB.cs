using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPB : MonoBehaviour
{
	public AudioClip _call;
	float _callDelay;
	public Vector2 _callDelayRange;
	float _callTimer;
	public int _minCalls;
	public int _maxCalls;
	int _callCounter;
	public Vector2 _chillRange;
	int _state;
	public bool _scanned;
	bool _targetted;

	void Awake(){
		_callDelay=Random.Range(_callDelayRange.x,_callDelayRange.y);
		_callCounter=Random.Range(_minCalls,_maxCalls+1);
	}
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
		switch(_state){
			case 0:
				_callTimer+=Time.deltaTime;
				if(_callTimer>=_callDelay){
					if(_call!=null)
						Sfx.PlayOneShot3D(_call,transform.position,Random.Range(0.95f,1.05f));
					_callTimer=0f;
					_callCounter--;
					if(_callCounter<=0){
						_state=1;
						_callDelay=Random.Range(_chillRange.x,_chillRange.y);
					}
					else
						_callDelay=Random.Range(_callDelayRange.x,_callDelayRange.y);
				}
				break;
			case 1:
				_callTimer+=Time.deltaTime;
				if(_callTimer>=_callDelay){
					_callDelay=Random.Range(_chillRange.x,_chillRange.y);
					_callCounter=Random.Range(_minCalls,_maxCalls+1);
					_state=0;
				}
				break;
		}
    }

	public void Targeted(bool foo){
		_targetted=foo;
	}

	void OnTriggerEnter(Collider other){
		Debug.Log("yoyoyoyo");
		//tree view
		//game logic somewhere
		//animations and stuff
	}
}
