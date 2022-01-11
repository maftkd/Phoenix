using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
	Bird _bird;
	Bird _mate;
	public float _checkInterval;
	public float _targetOffset;
	public float _followThreshold;
	float _checkTimer;
	public float _followSpeed;

	void Awake(){
		_bird=GetComponent<Bird>();
		_mate=_bird._mate;
	}

	void OnEnable(){

	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		_checkTimer+=Time.deltaTime;
		if(_checkTimer>=_checkInterval){
			Vector3 diff=transform.position-_mate.transform.position;
			if(diff.sqrMagnitude>_followThreshold*_followThreshold){
				//_bird.WaddleTo(_mate.transform.position+diff.normalized*_followDistance,1f);
				_bird.WaddleTo(_mate.transform.position-_mate.transform.forward*_targetOffset,_followSpeed);
			}
			else
				_bird.StopWaddling();
			_checkTimer=0;
		}
    }

	public void StartFollowingMate(){
		enabled=true;
	}

	public void StopFollowingMate(){
		_bird.StopWaddling();
		enabled=false;
		Debug.Log("say whaaa?");
	}
}
