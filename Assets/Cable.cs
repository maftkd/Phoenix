using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Cable : MonoBehaviour
{
	CubicBezierPath _path;
	MeshFilter _meshF;
	[HideInInspector]
	public MeshRenderer _meshR;
	List<Vector3> _points; //used to check if mesh needs updating
	List<Quaternion> _rots;
	List<Vector3> _groundPoints;
	bool _init;
	Transform _controlPoints;
	public int _vertsPerCenter;
	public float _vertexSpacing;
	public float _radius;
	public Terrain _terrain;

	Dictionary<float,Vector3> _centers;

	void Awake(){
		Init();
	}

	[ContextMenu("Reset")]
	public void Reset(){
		Init();
	}

	void Init(){
		_meshR = GetComponent<MeshRenderer>();
		_points = new List<Vector3>();
		_groundPoints = new List<Vector3>();
		_rots = new List<Quaternion>();
		_controlPoints=transform.Find("ControlPoints");
		_centers = new Dictionary<float,Vector3>();

		_init=true;
	}

    // Start is called before the first frame update
    void Start()
    {
		foreach(Transform t in _controlPoints){
			t.GetComponent<MeshRenderer>().enabled=false;
		}
    }

    // Update is called once per frame
    void Update()
    {
		if(!_init)
			Init();
		//check to regen
		//if we want to disable logic at runtime - pre ship or whatever
		//we can check if Application.isPlaying before doing this logic
		bool needsRegen=false;
		if(_points.Count!=_controlPoints.childCount)
		{
			needsRegen=true;
		}
		else{
			for(int i=0;i<_controlPoints.childCount;i++){
				Transform t = _controlPoints.GetChild(i);
				if(t.position!=_points[i]||t.rotation!=_rots[i])
				{
					needsRegen=true;
				}
			}
		}
		if(needsRegen){
			UpdatePoints();
			Debug.Log("regenerating "+name);
			if(_points.Count<=1){
				Debug.Log("Need at least two points to regen");
				return;
			}

			//cast points to ground points
			_groundPoints.Clear();
			foreach(Vector3 p in _points){
				Vector3 pos=p;
				pos.y=_terrain.SampleHeight(pos);
				_groundPoints.Add(pos);
			}
			_path = new CubicBezierPath(_groundPoints.ToArray());

			//get raw points at very small increments
			Dictionary<float,Vector3> pointsRaw = new Dictionary<float,Vector3>();
			float inc=0.02f;
			float last=0;
			for(float i=0; i<_groundPoints.Count-1;i+=inc){
				Vector3 pos = _path.GetPoint(i);
				float y = _terrain.SampleHeight(pos);
				pos.y=y;
				pointsRaw.Add(i,pos);
				last=i;
			}

			//loop through raw points catching points at least a certain distance
			_centers.Clear();
			Vector3 prevPoint=pointsRaw[0];
			_centers.Add(0,prevPoint);
			float dist=0;
			foreach(float t in pointsRaw.Keys){
				Vector3 curPoint=pointsRaw[t];
				float curDist=(curPoint-prevPoint).magnitude;
				dist+=curDist;
				if(dist>=_vertexSpacing)
				{
					_centers.Add(t,curPoint);
					dist=0;
				}
				prevPoint=curPoint;
			}
			if(dist>0)
			{
				_centers.Add(last,pointsRaw[last]);
			}

			//make da mesh
			Mesh m = new Mesh();
			int numCenters=_centers.Count;
			Vector3[] vertices = new Vector3[numCenters*_vertsPerCenter];
			int[] tris = new int[(numCenters-1)*(_vertsPerCenter-1)*6];
			Vector3[] norms = new Vector3[vertices.Length];
			Vector2[] uvs = new Vector2[vertices.Length];
			int vertexCounter=0;
			
			Transform temp = new GameObject("temp").transform;
			//get vertex positions
			foreach(float t in _centers.Keys){
				Vector3 centerPos = _centers[t];
				Vector3 tan = _path.GetTangent(t).normalized;
				temp.forward=tan;
				Vector3 right = temp.right;
				Vector3 up = temp.up;
				for(int i=0;i<_vertsPerCenter;i++){
					float t01 = i/(float)(_vertsPerCenter-1);
					float ang=t01*Mathf.PI;
					Vector3 pos=centerPos;
					pos+=right*_radius*Mathf.Cos(ang);
					pos+=up*_radius*Mathf.Sin(ang);
					vertices[vertexCounter]=pos;
					norms[vertexCounter]=pos-centerPos;
					vertexCounter++;
				}
			}
			DestroyImmediate(temp.gameObject);
			int triCounter=0;
			for(int i=0;i<numCenters-1;i++){
				int baseV=_vertsPerCenter*i;
				for(int j=0;j<_vertsPerCenter-1;j++){
					//fist tri
					tris[triCounter]=baseV;
					tris[triCounter+1]=baseV+1;
					tris[triCounter+2]=baseV+_vertsPerCenter;
					//second tri
					tris[triCounter+3]=baseV+_vertsPerCenter;
					tris[triCounter+4]=baseV+1;
					tris[triCounter+5]=baseV+_vertsPerCenter+1;
					triCounter+=6;
					baseV++;
				}
			}
			for(int i=0; i<vertices.Length; i++){
				int centerIndex=i/_vertsPerCenter;
				uvs[i]=new Vector2(i%_vertsPerCenter/(float)(_vertsPerCenter-1),centerIndex/(float)numCenters);
				//uvs[i]=Vector2.right;
			}
			m.vertices=vertices;
			m.triangles=tris;
			m.normals = norms;
			m.uv=uvs;
			m.RecalculateBounds();
			_meshF = GetComponent<MeshFilter>();
			_meshF.sharedMesh=m;
		}
    }

	void UpdatePoints(){
		_points.Clear();
		_rots.Clear();
		for(int i=0;i<_controlPoints.childCount; i++){
			Transform t =_controlPoints.GetChild(i); 
			_points.Add(t.position);
			_rots.Add(t.rotation);
		}
	}

	void OnValidate(){
		Reset();
	}

	void OnDrawGizmos(){
		/*
		if(_centers!=null){
			Gizmos.color=Color.red;
			foreach(Vector3 c in _centers.Values){
				Gizmos.DrawSphere(c,0.25f);
			}
		}
		if(_points!=null){
			Gizmos.color=Color.blue;
			foreach(Vector3 c in _points){
				Gizmos.DrawSphere(c,0.25f);
			}
		}
		*/
	}
}
