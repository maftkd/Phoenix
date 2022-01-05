﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionHelper : MonoBehaviour
{
	[HideInInspector]
	public Vector3 _hitPoint;
	Vector3 _hitNormal;
	Collider _col;
	public float _maxDotToKnock;

	[Header("audio")]
	public AudioClip [] _clips;
	AudioSource [] _sources;
	public int _numSources;
	[Range(0,1)]
	public float _volume;

	bool _hasMeshCollider;

	void Awake(){
		_col=GetComponent<Collider>();
		_sources = new AudioSource[_numSources];
		for(int i=0; i<_numSources; i++){
			GameObject foo = new GameObject("AudioSource");
			foo.transform.SetParent(transform);
			_sources[i]=foo.AddComponent<AudioSource>();
			_sources[i].spatialBlend=1f;
		}
		_hasMeshCollider=transform.GetComponent<MeshCollider>()!=null;
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
		if(other.GetComponent<Bird>()!=null)
		{
			if(_hasMeshCollider)
			{
				_hitPoint=other.transform.position;
			}
			else
			{
				_hitPoint=_col.ClosestPoint(other.transform.position);
			}
			_hitNormal=other.transform.position-_hitPoint;
			if(_hitNormal.sqrMagnitude==0)
				_hitNormal=-other.transform.forward;
			Bird b = other.GetComponent<Bird>();
			Vector3 curPos=other.transform.position;
			b.RevertToPreviousPosition();
			_hitNormal.y=0;
			_hitNormal.Normalize();
			float dt = Vector3.Dot(_hitNormal, other.transform.forward);
			if(dt>_maxDotToKnock){
				other.transform.position=curPos;
			}
			else
				other.GetComponent<Bird>().KnockBack(this,_hitNormal);
		}
	}

	public void Sound(float vol){
		if(_sources==null||_clips.Length==0)
			return;
		foreach(AudioSource s in _sources){
			if(!s.isPlaying){
				s.volume=vol;
				s.transform.position=_hitPoint;
				s.clip=_clips[Random.Range(0,_clips.Length)];
				s.Play();
				return;
			}
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
	}
}