using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunAway : MonoBehaviour
{
	public Transform [] _paths;
	Transform[] _spots;
	int _pathIndex;
	int _curSpot;
	Bird _bird;
	int _state;
	float _ruffleTimer;
	float _ruffleDelay;

	void Awake(){
		foreach(Transform t in _paths){
			foreach(Transform s in t){
				if(s.GetComponent<MeshRenderer>()!=null)
					s.GetComponent<MeshRenderer>().enabled=false;
			}
		}
		_bird=GetComponent<Bird>();
	}

	void OnEnable(){
		if(_pathIndex>=_paths.Length)
		{
			enabled=false;
			return;
		}
		_spots = new Transform[_paths[_pathIndex].childCount];
		for(int i=0; i<_spots.Length; i++){
			_spots[i]=_paths[_pathIndex].GetChild(i);
		}
		_curSpot=0;
		_state=0;
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
			default://idle
				if(_bird.IsPlayerClose()){
					if(_curSpot<_spots.Length){
						if(_curSpot==0){
							//ruffle for first spot
							_state=1;
							_ruffleDelay = _bird.Ruffle();
							_ruffleTimer=0;
						}
						else{
							//otherwise just start walking
							Transform target=_spots[_curSpot];
							_bird.WaddleTo(target.position,1f);
							_state=2;
						}
					}
					else{
						//no more spots
						enabled=false;
					}
				}
				break;
			case 1://wait
				_ruffleTimer+=Time.deltaTime;
				if(_ruffleTimer>=_ruffleDelay){
					Transform target=_spots[_curSpot];
					//_bird.HopTo(target.position);
					_bird.WaddleTo(target.position,1f);
					_state=2;
				}
				//maybe we wait for some effects to register
				//
				break;
			case 2://hopping
				if(_bird.ArrivedW()){
					_curSpot++;
					_state=0;
					_bird.StopWaddling();
				}
				break;
		}
    }

	public void RunAwayOnPath(int pathIndex){
		_pathIndex=pathIndex;
		enabled=true;
	}

	public void RunAwayNextPath(){
		RunAwayOnPath(_pathIndex+1);
	}

	void OnDrawGizmos(){
		if(_bird==null)
			return;
		Gizmos.color=Color.blue;
		foreach(Transform t in _paths){
			foreach(Transform s in t){
				Gizmos.DrawWireSphere(s.position,_bird._triggerRadius);
			}
		}
	}
}
