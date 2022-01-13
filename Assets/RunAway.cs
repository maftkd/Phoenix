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
	MCamera _mCam;
	float _chirpTimer;
	float _chirpTime;
	public Vector2 _chirpTimeRange;

	void Awake(){
		foreach(Transform t in _paths){
			foreach(Transform s in t){
				if(s.GetComponent<MeshRenderer>()!=null)
					s.GetComponent<MeshRenderer>().enabled=false;
			}
		}
		_bird=GetComponent<Bird>();
		_mCam=Camera.main.transform.parent.GetComponent<MCamera>();
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
		_chirpTimer=0;
		_chirpTime=Random.Range(_chirpTimeRange.x,_chirpTimeRange.y);
	}

	void OnDisable(){
		if(_mCam!=null)
			_mCam.DefaultCam();
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
				if(_bird.IsPlayerClose(_bird._mate)){
					if(_curSpot<_spots.Length){
						/*
						if(_curSpot==0){
							//ruffle for first spot
							_state=1;
							_ruffleDelay = _bird.Ruffle();
							_ruffleTimer=0;
						}
						else{
						*/
							//otherwise just start walking
						Transform target=_spots[_curSpot];
						_bird.WaddleTo(target.position,1f);
						_state=2;
						//}
					}
					else{
						//no more spots
						enabled=false;
					}
				}
				else{
					_chirpTimer+=Time.deltaTime;
					if(_chirpTimer>_chirpTime)
					{
						_bird.CallToMate();
						_chirpTimer=0;
						_chirpTime=Random.Range(_chirpTimeRange.x,_chirpTimeRange.y);
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
					_bird.CallToMate();
				}
				break;
		}
    }

	public void RunAwayOnPath(int pathIndex){
		_pathIndex=pathIndex;
		_mCam.Surround(transform);
		enabled=true;
	}

	public void RunAwayNextPath(){
		RunAwayOnPath(_pathIndex+1);
	}

	public void FinishPath(int index){
		Vector3 target=_paths[index].GetChild(_paths[index].childCount-1).position;
		transform.position=target;
		_pathIndex=index;
		_state=0;
		enabled=false;
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
