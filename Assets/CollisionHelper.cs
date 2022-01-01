using System.Collections;
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

	void Awake(){
		_col=GetComponent<Collider>();
		_sources = new AudioSource[_numSources];
		for(int i=0; i<_numSources; i++){
			GameObject foo = new GameObject("AudioSource");
			foo.transform.SetParent(transform);
			_sources[i]=foo.AddComponent<AudioSource>();
			_sources[i].spatialBlend=1f;
		}
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
		_hitPoint=_col.ClosestPoint(other.transform.position);
		//check the normal vector
		_hitNormal=other.transform.position-_hitPoint;
		_hitNormal.Normalize();
		float dt = Vector3.Dot(_hitNormal,Vector3.up);
		if(Mathf.Abs(dt)>_maxDotToKnock)
		{
			Debug.Log("ignoring collision due to up/down-ness of collision normal");
			return;
		}
		if(other.GetComponent<Bird>()!=null)
		{
			other.GetComponent<Bird>().KnockBack(this,_hitNormal);
			//Sound(_hitPoint);
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
