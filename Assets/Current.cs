using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Current : MonoBehaviour
{
	public int _numPoints;
	//public Transform _streamNode;
	public float _dist;
	public float _dip;
	public float _minDistToNode;
	public float _windLerpStrength;
	public float _startT;
	Vector3 [] _points;
	Quaternion _prevRot;
	Vector3 _prevPos;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if((_points==null || _points.Length!=_numPoints)&&_numPoints>1)
		{
			Debug.Log("Gotta regen points");
			UpdatePoints();
		}
		else if(transform.position!=_prevPos||transform.rotation!=_prevRot){
			UpdatePoints();
		}
		_prevPos=transform.position;
		_prevRot=transform.rotation;
    }

	void UpdatePoints(){
		_points = new Vector3[_numPoints];
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
			//Transform point = Instantiate(_streamNode,transform);
			//point.localPosition=
			Vector3 local = new Vector3(0,y-yOffset,_dist*tRaw);
			_points[i]=transform.TransformPoint(local);
		}
	}

	void OnValidate(){
		UpdatePoints();
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.green;
		Gizmos.DrawWireSphere(transform.position,0.15f);
		Gizmos.color = Color.blue;
		if(_points!=null){
			foreach(Vector3 p in _points){
				Gizmos.DrawWireSphere(p,0.1f);
			}
		}
		else{
			Debug.Log("sadge");
		}
	}
}
