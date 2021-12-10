using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindStream : MonoBehaviour
{
	public int _numPoints;
	public Transform _streamNode;
	public float _dist;
	public float _dip;
	public float _minDistToNode;
	public float _windLerpStrength;
	public float _startT;
	Fly _fly;
    // Start is called before the first frame update
    void Start()
    {
		if(_numPoints<2)
			return;
		float h = _dist*0.5f;
		float k = -_dip;
		float a = -k/(h*h);
		float yOffset=0;
		for(int i=0;i<_numPoints; i++){
			float tRaw = (float)i/(_numPoints-1);
			float t = Mathf.Lerp(_startT,1f,tRaw);
			float x = t*_dist;
			float y = a*(x-h)*(x-h)+k;
			if(i==0)
				yOffset=y;
			Transform point = Instantiate(_streamNode,transform);
			point.localPosition=new Vector3(0,y-yOffset,_dist*tRaw);
		}
		_fly = Fly._instance;
    }

    // Update is called once per frame
    void Update()
    {
		if(!_fly.enabled)
			return;
		float minSqrDist=_minDistToNode*_minDistToNode;
		int closestIndex=-1;
		for(int i=0; i<transform.childCount; i++){
			Transform t = transform.GetChild(i);
			float sqrDist = (t.position-_fly.transform.position).sqrMagnitude;
			if(sqrDist<minSqrDist){
				minSqrDist=sqrDist;
				closestIndex=i;
			}
		}
		if(closestIndex>-1&&closestIndex<transform.childCount-1)
			_fly.FlyTowards(transform.GetChild(closestIndex+1).position,_windLerpStrength*Time.deltaTime);
    }

	void OnDrawGizmos(){
		Gizmos.color=Color.green;
		foreach(Transform t in transform){
			Gizmos.DrawWireSphere(t.position,_minDistToNode);
		}
		Gizmos.DrawSphere(transform.position,_minDistToNode);
	}
}
