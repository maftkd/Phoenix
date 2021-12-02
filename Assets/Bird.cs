using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour
{
	Animator _anim;
	int _state;
	Transform _flyTarget;
	public float _flySpeed;
	public float _landSpeed;
	Vector3 _dir;//#temp
	Vector3 _pos;//#temp - maybe prevDir and prevPos;
	Quaternion _prevRot;
	Quaternion _targetRot;
	float _peckTimer;
	float _peckCheckTimer;
	public float _peckCheckTime;
	public float _peckChance;
    // Start is called before the first frame update
    void Start()
    {
		_anim=GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
		switch(_state){
			case 0://idle
			default:
				//#temp
				//GoToFood
				GameObject [] feeders = GameObject.FindGameObjectsWithTag("Feeder");
				GameObject feeder = feeders[Random.Range(0,feeders.Length)];
				Transform feederPerch = feeder.transform.GetChild(
						Random.Range(0,feeder.transform.childCount));
				_flyTarget=feederPerch;
				_anim.SetBool("flying",true);
				transform.LookAt(_flyTarget);
				_state = 1;
				break;
			case 1://flying
				transform.position+=transform.forward*Time.deltaTime*_flySpeed;
				//if distance to target <.3
				//	landing
				if((transform.position-_flyTarget.position).sqrMagnitude<0.09f){
					_state=2;
					_anim.SetBool("flying",false);
					_dir = _flyTarget.position-transform.position;
					_pos = transform.position;
					_prevRot = transform.rotation;
					_targetRot = _flyTarget.rotation;
				}
				break;
			case 2://landing
				transform.position+=_dir.normalized*Time.deltaTime*_landSpeed;
				float tx = (transform.position.x-_pos.x)/_dir.x;
				float tz = (transform.position.z-_pos.z)/_dir.z;
				float ty = (transform.position.y-_pos.y)/_dir.y;
				float t = Mathf.Max(Mathf.Max(tx,tz),ty);
				transform.rotation = Quaternion.Slerp(_prevRot,_targetRot,t);
				if(t>=1f){
					transform.position=_flyTarget.position;
					_state=3;
					if(_flyTarget.parent!=null){
						Vector3 looky=_flyTarget.parent.position;
						looky.y=_flyTarget.position.y;
						transform.LookAt(looky);
					}
				}
				break;
			case 3://feeding
				if(_peckTimer<=0){
					if(_peckCheckTimer<=0){
						if(Random.value<_peckChance){
							_anim.SetTrigger("peck");
							_peckTimer=1f;
						}
						_peckCheckTimer=_peckCheckTime;
					}
					else
						_peckCheckTimer-=Time.deltaTime;
				}
				else
					_peckTimer-=Time.deltaTime;
				//if not pecking
				//	wait 1 sec
				//		//check for new peck
				//		//reset wait timer
				break;
		}
    }

	void OnDrawGizmos(){
		Gizmos.color=Color.blue;
		if(_flyTarget!=null)
			Gizmos.DrawSphere(_flyTarget.position,0.01f);
	}
}
