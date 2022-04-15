using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindTunnel : MonoBehaviour
{
	Bird _player;
	public float _boostAmount;
	public float _minDotToThrust;
	public float _boostDur;

	void Awake(){
		_player=GameManager._player;
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	void OnTriggerEnter(Collider other){
		float dot=Vector3.Dot(_player.transform.forward,-transform.up);
		if(dot>_minDotToThrust){
			_player.BoostSpeed(_boostAmount,_boostDur);
		}
	}
}
