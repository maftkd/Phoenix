using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
	public Transform _boid;
	public int _numBoids;
	Transform[] _boids;
	Vector3 [] _vels;
	public Vector3 _minZone;
	public Vector3 _maxZone;
	public float _maxSpawnOffset;

	public float _centering;
	public float _visualRange;
	public float _minDistance;
	public float _avoidance;
	public float _matching;
	public float _speedLimit;
	public float _margin;
	public float _turnFactor;

	void Awake(){
		_boids = new Transform[_numBoids];
		_vels = new Vector3[_numBoids];
		Vector3 spawnCenter=new Vector3(Random.Range(_minZone.x,_maxZone.x),
				Random.Range(_minZone.y,_maxZone.y),
				Random.Range(_minZone.z,_maxZone.z));
		for(int i=0; i<_numBoids; i++){
			_boids[i]=Instantiate(_boid,transform);
			Vector3 offset=Random.insideUnitSphere*_maxSpawnOffset;
			Vector3 pos = spawnCenter+offset;
			pos.x=Mathf.Clamp(pos.x,_minZone.x,_maxZone.x);
			pos.y=Mathf.Clamp(pos.y,_minZone.y,_maxZone.y);
			pos.z=Mathf.Clamp(pos.z,_minZone.z,_maxZone.z);
			_boids[i].position=pos;
			_vels[i]=Vector3.zero;
		}
	}

    // Update is called once per frame
    void Update()
    {
		for(int i=0; i<_boids.Length; i++){
			//fly towards center
			FlyTowardsCenter(i);
			//avoid others
			AvoidOthers(i);
			//match velocity
			MatchVelocity(i);
			//limit speed
			LimitSpeed(i);
			//keep within bounds
			KeepWithinBounds(i);
			//add velocity
			_boids[i].position+=_vels[i]*Time.deltaTime;
			if(_vels[i]!=Vector3.zero)
				_boids[i].forward=_vels[i];
		}
        
    }

	void FlyTowardsCenter(int index){
		Vector3 center=Vector3.zero;
		int numNeighbors=0;

		for(int i=0; i<_boids.Length;i++){
			if((_boids[i].position-_boids[index].position).sqrMagnitude<_visualRange*_visualRange){
				center+=_boids[i].position;
				numNeighbors++;
			}
		}
		if(numNeighbors>0){
			center/=(float)numNeighbors;
			_vels[index]+=(center-_boids[index].position)*_centering;
		}
	}

	void AvoidOthers(int index){
		Vector3 move = Vector3.zero;
		for(int i=0; i<_boids.Length; i++){
			if(i!=index&&(_boids[i].position-_boids[index].position).sqrMagnitude<_minDistance*_minDistance){
				move+=_boids[index].position-_boids[i].position;
			}
		}

		_vels[index]+=move*_avoidance;
	}

	void MatchVelocity(int index){
		Vector3 avgDelta = Vector3.zero;
		int numNeighbors=0;

		for(int i=0; i<_boids.Length; i++){
			if((_boids[i].position-_boids[index].position).sqrMagnitude<_visualRange*_visualRange){
				avgDelta+=_vels[i];
				numNeighbors++;
			}
		}

		if(numNeighbors>0){
			avgDelta/=(float)numNeighbors;
			_vels[index]+=(avgDelta-_vels[index])*_matching;
		}
	}

	void LimitSpeed(int index){
		float speed = _vels[index].magnitude;
		if(speed>_speedLimit){
			_vels[index]=_vels[index].normalized*_speedLimit;
		}
	}

	void KeepWithinBounds(int index){
		Vector3 pos = _boids[index].position;
		if(pos.x<_minZone.x+_margin){
			_vels[index].x+=_turnFactor;
		}
		else if(pos.x>_maxZone.x-_margin){
			_vels[index].x-=_turnFactor;
		}
		if(pos.y<_minZone.y+_margin){
			_vels[index].y+=_turnFactor;
		}
		else if(pos.y>_maxZone.y-_margin){
			_vels[index].y-=_turnFactor;
		}
		if(pos.z<_minZone.z+_margin){
			_vels[index].z+=_turnFactor;
		}
		else if(pos.z>_maxZone.z-_margin){
			_vels[index].z-=_turnFactor;
		}
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.blue;
		Gizmos.DrawWireCube(Vector3.Lerp(_minZone,_maxZone,0.5f)
				,new Vector3(_maxZone.x-_minZone.x,_maxZone.y-_minZone.y,_maxZone.z-_minZone.z));
	}
}
