using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wind : MonoBehaviour
{
	public Vector3 _extents;
	Transform _windDirT;
	Vector3 _windDir;
	Bird _player;
	public float _windStrength;
	ParticleSystem _parts;

	void Awake(){
		BoxCollider box=GetComponent<BoxCollider>();
		box.size=_extents;
		_player = GameManager._player;
		if(_windDirT==null)
			_windDirT=transform.Find("WindDir");
		if(_windDirT==null)
		{
			Debug.Log("Error, could not find wind dir. Destroying wind zone: "+name);
			Destroy(gameObject,0.1f);
		}
		_windDir=_windDirT.forward;
		_parts=GetComponent<ParticleSystem>();
		var sh = _parts.shape;
		sh.scale=_extents;
		var vol = _parts.velocityOverLifetime;
		Vector3 vel=_windDir*_windStrength;
		vol.xMultiplier=vel.x;
		vol.yMultiplier=vel.y;
		vol.zMultiplier=vel.z;
		var main = _parts.main;
		Vector3 forward = _windDirT.forward;
		float r = (forward.x+1)*0.5f;
		float g = (forward.y+1)*0.5f;
		float b = (forward.z+1)*0.5f;
		Color arrowColor=new Color(r,g,b);
		main.startColor=arrowColor;
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

	void OnTriggerEnter(Collider other){
		//_player.SetMaxVel(_maxVel);
	}

	void OnTriggerStay(Collider other){
		_player.AddForce(_windDir*_windStrength);
	}

	void OnTriggerExit(Collider other){
		//_player.SetMaxVel(-1f);
	}

	void OnDrawGizmos(){
		if(_extents==null)
			return;
		if(_windDirT==null)
			_windDirT=transform.Find("WindDir");
		if(_windDirT==null)
			return;
		Vector3 forward = _windDirT.forward;
		float r = (forward.x+1)*0.5f;
		float g = (forward.y+1)*0.5f;
		float b = (forward.z+1)*0.5f;
		Color arrowColor=new Color(r,g,b);
		Gizmos.color=arrowColor;
		Gizmos.matrix = _windDirT.localToWorldMatrix;
		Gizmos.DrawFrustum(Vector3.zero,40f,-1f,0.01f,1f);
		Gizmos.DrawLine(Vector3.back*2f,Vector3.zero);

		Gizmos.color=Color.black;
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.DrawWireCube(Vector3.zero, _extents);

	}
}
