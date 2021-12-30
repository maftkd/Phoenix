using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunAway : MonoBehaviour
{
	public Transform[] _spots;
	int _curSpot;
	Bird _bird;
	Hop _hop;
	int _state;
	float _ruffleTimer;
	float _ruffleDelay;

	void Awake(){
		foreach(Transform t in _spots){
			if(t.GetComponent<MeshRenderer>()!=null)
				t.GetComponent<MeshRenderer>().enabled=false;
		}
		_bird=GetComponent<Bird>();
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
						_state=1;
						_ruffleDelay = _bird.Ruffle();
						_ruffleTimer=0;
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
					_bird.HopTo(target.position);
					_state=2;
				}
				//maybe we wait for some effects to register
				//
				break;
			case 2://hopping
				if(!_bird.IsHopping()&&_bird.Arrived()){
					_curSpot++;
					_state=0;
					_bird.StopHopping();
				}
				break;
		}

    }
}
