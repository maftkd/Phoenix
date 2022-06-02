using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Magnetics : MonoBehaviour
{
	CubicBezierPath _path;
	List<Vector3> _points;
	Dictionary<float,Vector3> _centers;
	public float _vertexSpacing;
	public int _vertsPerCenter;
	public float _radius;

	public bool _autoGen;
	public bool _generate;

	void OnValidate(){
		if(_generate||_autoGen){
			_generate=false;
			GenerateMesh();
		}
	}

	void GenerateMesh(){
		_points = new List<Vector3>();
		for(int i=0;i<transform.childCount; i++){
			Transform t =transform.GetChild(i); 
			_points.Add(t.position);
		}
		if(_points.Count<=1){
			Debug.Log("Need at least two points to regen");
			return;
		}
		_path = new CubicBezierPath(_points.ToArray());

		Dictionary<float,Vector3> pointsRaw = new Dictionary<float,Vector3>();
		float inc=0.02f;
		float last=0;
		for(float i=0; i<_points.Count-1;i+=inc){
			Vector3 pos = _path.GetPoint(i);
			pointsRaw.Add(i,pos);
			last=i;
		}

		//loop through raw points catching points at least a certain distance
		_centers = new Dictionary<float,Vector3>();
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
				float ang=t01*Mathf.PI*2;
				Vector3 pos=centerPos;
				pos+=right*_radius*Mathf.Cos(ang);
				pos+=up*_radius*Mathf.Sin(ang);
				vertices[vertexCounter]=transform.InverseTransformPoint(pos);
				norms[vertexCounter]=pos-centerPos;
				vertexCounter++;
			}
		}
		StartCoroutine(DestroyNextFrame(temp.gameObject));
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
		GetComponent<MeshFilter>().sharedMesh=m;
		//_meshF.sharedMesh=m;
	}

	IEnumerator DestroyNextFrame(GameObject go){
		yield return null;
		DestroyImmediate(go);
	}

	void Awake(){
		GenerateMesh();
	}

    // Start is called before the first frame update
    void Start()
    {
		foreach(Transform t in transform)
			t.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
