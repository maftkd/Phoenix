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

	bool _hasMeshCollider;
	bool _hasBoxCollider;
	BoxCollider _box;
	public bool _supressHitFx;
	public bool _supressNpcKnockback;
	public bool _inverted;

	//float _fudge = 1.1f;

	public UnityEvent _onBirdEnter;

	void Awake(){
		_col=GetComponent<Collider>();
		_hasMeshCollider=transform.GetComponent<MeshCollider>()!=null;
		_hasBoxCollider=transform.GetComponent<BoxCollider>()!=null;
		if(_hasBoxCollider)
			_box=GetComponent<BoxCollider>();
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
		if(!_inverted)
			HandleCollision(other);
	}
	void OnTriggerStay(Collider other){
		Bird b = other.GetComponent<Bird>();
		if(b!=null&&b._state<3&&!_inverted)
			HandleCollision(other);
	}

	void OnTriggerExit(Collider other){
		if(_inverted)
			HandleCollision(other);
	}

	void HandleCollision(Collider other){
		if(!enabled)
			return;
		if(other.GetComponent<Bird>()!=null&&other.gameObject.tag=="Player")
		{
			_onBirdEnter.Invoke();
			Bird b = other.GetComponent<Bird>();
			Vector3 curPos=other.transform.position;
			_hitPoint=_col.ClosestPoint(other.transform.position);
			_hitNormal=other.transform.position-_hitPoint;
			_newPoint=_hitPoint+_hitNormal.normalized*b._hitRadius;
			if(b._state==1)
			{
				b.SnapToPos(_newPoint);
			}
			Vector3 flatNormal=_hitNormal;
			flatNormal.y=0;
			flatNormal.Normalize();
			float dt = Vector3.Dot(flatNormal, other.transform.forward);
			if(dt>_maxDotToKnock){
				other.transform.position=curPos;
			}
			else
			{
				bool closeEnough=false;
				if(!closeEnough)
					b.KnockBack(this,_hitNormal.normalized,_supressHitFx,_supressNpcKnockback);
			}
		}

	}

	public void Sound(float vol){
		if(_clips.Length==0)
			return;
		Sfx.PlayOneShot3DVol(_clips[Random.Range(0,_clips.Length)],_hitPoint,vol);
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
