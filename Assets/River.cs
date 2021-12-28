using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class River : MonoBehaviour
{

	CubicBezierPath _path;
	MeshFilter _meshF;
	[HideInInspector]
	public MeshRenderer _meshR;
	List<Vector3> _points; //used to check if mesh needs updating
	List<Vector3> _scales;
	List<Quaternion> _rots;
	MeshCollider _mc;
	bool _init;
	Transform _riverPoints;
	public int _resolution;
	public Transform _sfxPrefab;
	Transform _riverAudio;

	//debugging
	List<Vector3> _centers;

	[ContextMenu("Reset")]
	public void Reset(){
		Init();
	}

	void Init(){
		_mc = GetComponent<MeshCollider>();
		_meshR = GetComponent<MeshRenderer>();
		_points = new List<Vector3>();
		_scales = new List<Vector3>();
		_rots = new List<Quaternion>();
		_riverPoints=transform.Find("RiverPoints");
		_riverAudio = transform.Find("Audio");
		_centers = new List<Vector3>();

		_init=true;
	}

    // Start is called before the first frame update
    void Start()
    {
		foreach(Transform t in _riverPoints){
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
		if(_points.Count!=_riverPoints.childCount)
		{
			needsRegen=true;
		}
		else{
			for(int i=0;i<_riverPoints.childCount;i++){
				Transform t = _riverPoints.GetChild(i);
				if(t.position!=_points[i]||t.localScale!=_scales[i]||t.rotation!=_rots[i])
				{
					needsRegen=true;
					//_points[i]=_riverPoints.GetChild(i).position;
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
			_path = new CubicBezierPath(_points.ToArray());
			Mesh m = new Mesh();
			int numCenters=(_points.Count-1)*_resolution+1;
			Vector3[] vertices = new Vector3[numCenters*2];
			int[] tris = new int[(numCenters-1)*6];
			Vector3[] norms = new Vector3[vertices.Length];
			Vector2[] uvs = new Vector2[vertices.Length];
			int vertexCounter=0;
			_centers.Clear();
			for(int i=0;i<numCenters; i++){
				float t01 = i/(float)(numCenters-1);
				float t = t01*(_points.Count-1);
				Vector3 centerPos = _path.GetPoint(t);
				Vector3 tan = _path.GetTangent(t);
				_centers.Add(centerPos);
				int transformIndex = i/_resolution;
				float width = _riverPoints.GetChild(transformIndex).localScale.x;
				Vector3 right = Vector3.Cross(Vector3.up,tan);
				Vector3 p1 = centerPos-right*width*0.5f;
				Vector3 p2 = centerPos+right*width*0.5f;
				vertices[vertexCounter]=p1;
				vertices[vertexCounter+1]=p2;
				vertexCounter+=2;
			}
			int triCounter=0;
			for(int i=0;i<numCenters-1;i++){
				//fist tri
				tris[triCounter]=2*i;
				tris[triCounter+2]=2*i+1;
				tris[triCounter+1]=2*i+2;
				//second tri
				tris[triCounter+3]=2*i+2;
				tris[triCounter+5]=2*i+1;
				tris[triCounter+4]=2*i+3;
				triCounter+=6;
			}
			for(int i=0; i<vertices.Length; i++){
				int centerIndex=i/2;
				int transformIndex=centerIndex/_resolution;
				norms[i]=_riverPoints.GetChild(transformIndex).up;
				uvs[i]=new Vector2(i%2,centerIndex/(float)numCenters);
			}
			m.vertices=vertices;
			m.triangles=tris;
			m.normals = norms;
			m.uv=uvs;
			m.RecalculateBounds();
			_meshF = GetComponent<MeshFilter>();
			_meshF.sharedMesh=m;
			if(_mc!=null)
				_mc.sharedMesh=m;
		}
    }

	void UpdatePoints(){
		_points.Clear();
		_scales.Clear();
		_rots.Clear();
		for(int i=0;i<_riverPoints.childCount; i++){
			Transform t =_riverPoints.GetChild(i); 
			_points.Add(t.position);
			_scales.Add(t.localScale);
			_rots.Add(t.rotation);
		}
		for(int i=_riverAudio.childCount-1;i>=0;i--)
			DestroyImmediate(_riverAudio.GetChild(i).gameObject);
		//just put a source near the middle
		Instantiate(_sfxPrefab,_points[_points.Count/2],Quaternion.identity,_riverAudio);
	}

	void OnValidate(){
		Reset();
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.blue;
		if(_centers!=null){
			foreach(Vector3 c in _centers){
				Gizmos.DrawSphere(c,0.25f);
			}
		}
	}

}
