using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurroundCamHelper : MonoBehaviour
{
	public float _outerRadius;
	public float _innerRadius;
	Transform _player;
	bool _playerInZone;
	MCamera _mCam;

	void Awake(){
		_player=GameObject.FindGameObjectWithTag("Player").transform;
		_mCam=Camera.main.GetComponent<MCamera>();
	}

	void OnDisable(){
		_mCam.DefaultCam();
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		float sqrMag = (_player.position-transform.position).sqrMagnitude;
		if(!_playerInZone){
			if(sqrMag<_outerRadius*_outerRadius){
				_playerInZone=true;
				_mCam.Surround(transform);
			}
		}
		else{
			if(sqrMag>_outerRadius*_outerRadius || sqrMag<_innerRadius*_innerRadius){
				_playerInZone=false;
				_mCam.DefaultCam();
			}
		}
        
    }

	void OnDrawGizmos(){
		if(!enabled)
			return;
		Gizmos.color=Color.green;
		Gizmos.DrawWireSphere(transform.position,_outerRadius);
		Gizmos.color=Color.blue;
		Gizmos.DrawWireSphere(transform.position,_innerRadius);
	}
}
