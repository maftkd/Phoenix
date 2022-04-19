using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollisionHelper : MonoBehaviour
{
	[HideInInspector]
	public Vector3 _hitPoint;
	Vector3 _hitNormal;
	Vector3 _newPoint;
	Collider _col;
	public float _maxDotToKnock;

	[Header("audio")]
	public AudioClip [] _clips;
	public int _numSources;
	[Range(0,1)]
	public float _volume;

	MeshCollider _meshCol;
	bool _hasBoxCollider;
	bool _isCapsule;
	BoxCollider _box;
	public bool _supressHitFx;
	public bool _supressNpcKnockback;
	public bool _inverted;
	bool _inZone;
	Bird _player;

	//float _fudge = 1.1f;

	public UnityEvent _onBirdEnter;

	void Awake(){
		_col=GetComponent<Collider>();
		_meshCol=transform.GetComponent<MeshCollider>();
		_hasBoxCollider=transform.GetComponent<BoxCollider>()!=null;
		_isCapsule=transform.GetComponent<CapsuleCollider>()!=null;
		if(_hasBoxCollider)
			_box=GetComponent<BoxCollider>();
		_player=GameManager._player;
		_player._onFlight+=FlightMode;
		_player._onLand+=LandMode;
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

	/*
    // Update is called once per frame
    void Update()
    {
		if(_inverted&&_isCapsule&&!_inZone)
			HandleCollision(null);
    }
	*/

	void OnTriggerEnter(Collider other){
		if(other.transform==transform)
			return;
		if(!_inverted)
		{
			Bird b = other.GetComponent<Bird>();
			if(b._state<3)
				HandleCollision(other);
			else{
				_hitPoint=b.transform.position;
				b.KnockBack(this,-b.transform.forward.normalized,_supressHitFx,_supressNpcKnockback);
			}
		}
		_inZone=true;
	}
	void OnTriggerStay(Collider other){
		if(other.transform==transform)
			return;
		Bird b = other.GetComponent<Bird>();
		if(b!=null&&b._state<3&&!_inverted)
			HandleCollision(other);
	}

	void OnTriggerExit(Collider other){
		if(_inverted)
		{
			Debug.Log("uhm");
			HandleCollision(other);
		}
		_inZone=false;
	}

	void HandleCollision(Collider other){
		if(!enabled)
			return;
		Bird b;
		if(other==null)
			b=_player;
		else if(other.GetComponent<Bird>()!=null&&other.gameObject.tag=="Player")
			b=other.GetComponent<Bird>();
		else
			return;

		_onBirdEnter.Invoke();
		Vector3 curPos=b.transform.position;
		_hitPoint=_col.ClosestPoint(b.transform.position);
		_hitNormal=b.transform.position-_hitPoint;
		if(_isCapsule||_inverted)
			_hitNormal*=-1;
		_newPoint=_hitPoint+_hitNormal.normalized*b._hitRadius;
		if(b._state==1)
		{
			if(!_isCapsule)
				b.SnapToPos(_newPoint);
			else
				b.RevertToPreviousPosition();

		}
		Vector3 flatNormal=_hitNormal;
		flatNormal.y=0;
		flatNormal.Normalize();
		float dt = Vector3.Dot(flatNormal, b.transform.forward);
		if(dt>_maxDotToKnock){
			b.transform.position=curPos;
		}
		else
		{
			bool closeEnough=false;
			if(!closeEnough)
				b.KnockBack(this,_hitNormal.normalized,_supressHitFx,_supressNpcKnockback);
		}
	}

	public void Sound(float vol){
		if(_clips.Length==0)
			return;
		Sfx.PlayOneShot3DVol(_clips[Random.Range(0,_clips.Length)],_hitPoint,vol);
	}

	public void FlightMode(){
		if(_meshCol!=null){
			_meshCol.isTrigger=false;
			_meshCol.convex=false;
		}
	}

	public void LandMode(){
		if(_meshCol!=null){
			_meshCol.convex=true;
			_meshCol.isTrigger=true;
		}

	}

	void OnDrawGizmos(){
		if(_hitPoint!=null)
		{
			Gizmos.color=Color.green;
			Gizmos.DrawWireSphere(_hitPoint,0.05f);
			if(_hitNormal!=null)
			{
				Gizmos.color=Color.red;
				Gizmos.DrawRay(_hitPoint,_hitNormal);
			}
		}
		if(_newPoint!=null){
			Gizmos.color=Color.blue;
			Gizmos.DrawWireSphere(_newPoint,0.05f);

		}
	}
}
