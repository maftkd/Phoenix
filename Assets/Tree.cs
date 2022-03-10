﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Tree : MonoBehaviour
{
	MeshFilter _meshF;
	MeshRenderer _meshR;
	MeshCollider _meshC;
	public enum TreeType {PALM, NONE};
	public TreeType _treeType;
	public int _trunkSeed;
	public int _vertsPerRing;
	public int _numRings;
	public Vector2 _heightRange;
	public Vector2 _trunkRadiusRange;
	public AnimationCurve _trunkRadiusCurve;
	public float _trunkNoiseScale;
	[Range(0,1)]
	public float _trunkNoiseAmplitude;
	Vector3 _treeTop;

	[Header("Leaves")]
	public int _leafSeed;
	public int _numLeaves;
	public int _leafSegments;
	public int _leafRes;
	public Vector2 _leafSizeRange;
	public Vector2 _leafWidthRange;
	public Vector2 _leafHeightRange;

	[Header("Palm leaves")]
	public Material _palmLeafMat;
	public AnimationCurve _palmLeafSpineCurve;
	public AnimationCurve _palmLeafWidthCurve;
	public AnimationCurve _palmVertCurve;
	public Vector2 _palmPitchRange;
	public Vector2 _palmSpinRange;

	[Header("Buttons")]
	public bool _genTree;
	public bool _autoGen;

	void OnValidate(){
		if(_genTree || _autoGen){
			GenTree();
			_genTree=false;
		}
	}

	void GenTree(){
		if(_numRings<2)
		{
			Debug.Log("Cannot create tree with <2 rings");
			return;
		}
		if(_vertsPerRing<3){
			Debug.Log("Cannot create rings with <3 verts per ring");
			return;
		}
		if(_heightRange.y<=0){
			Debug.Log("Cannot create tree with max height range < 0");
			return;
		}
		if(_meshR==null||_meshF==null||_meshC==null)
		{
			_meshR = GetComponent<MeshRenderer>();
			_meshF = GetComponent<MeshFilter>();
			_meshC = GetComponent<MeshCollider>();
		}
		switch(_treeType){
			case TreeType.PALM:
				GenPalmTree();
				break;
			default:
				break;
		}
	}

	void GenPalmTree(){
		Mesh m = new Mesh();
		Random.InitState(_trunkSeed);

		//allocate some mem
		Vector3[] vertices = new Vector3[(_vertsPerRing+1)*_numRings];
		int[] tris = new int[(_numRings-1)*(_vertsPerRing)*6];
		Vector3[] norms = new Vector3[vertices.Length];
		Vector2[] uvs = new Vector2[vertices.Length];

		//use a dumby transform cuz I'm a dumby
		Transform ringCenter = new GameObject("ring center").transform;
		ringCenter.position=Vector3.zero;
		ringCenter.rotation=transform.rotation;

		//some initial calculations
		float trunkHeight = Random.Range(_heightRange.x,_heightRange.y);
		float ringSpacing=trunkHeight/(_numRings-1);
		Vector2 offsetDir = Random.insideUnitCircle.normalized;
		float offset=Random.value;

		int vertexCounter=0;
		for(int r=0; r<_numRings; r++){
			float r01=r/(float)(_numRings-1);
			if(r>0){
				Vector3 prevPos=ringCenter.position;
				ringCenter.position+=ringCenter.up*ringSpacing;
				float noise=Mathf.PerlinNoise(0,(r01+offset)*_trunkNoiseScale);
				noise=noise*2f-1f;
				noise*=_trunkNoiseAmplitude;
				ringCenter.position+=new Vector3(offsetDir.x,0,offsetDir.y)*noise;
				ringCenter.up=ringCenter.position-prevPos;
				if(r==_numRings-1)
					_treeTop=ringCenter.position;
			}
			//radius as a function of height
			float radius = Mathf.Lerp(_trunkRadiusRange.x,_trunkRadiusRange.y,_trunkRadiusCurve.Evaluate(r01));
			for(int i=0; i<=_vertsPerRing; i++){
				float t01 = i/(float)_vertsPerRing;
				float ang=t01*Mathf.PI*2f;
				Vector3 pos=ringCenter.position;
				pos+=ringCenter.right*radius*Mathf.Cos(ang);
				pos+=ringCenter.forward*radius*Mathf.Sin(ang);
				vertices[vertexCounter]=pos;
				norms[vertexCounter]=pos-ringCenter.position;
				uvs[vertexCounter] = new Vector2(t01,r01);
				vertexCounter++;
			}
		}

		int triCounter=0;
		for(int i=0;i<_numRings-1;i++){
			int baseV=(_vertsPerRing+1)*i;//trust
			for(int j=0;j<_vertsPerRing;j++){//trust
				//fist tri
				tris[triCounter]=baseV;
				tris[triCounter+2]=baseV+1;
				tris[triCounter+1]=baseV+(_vertsPerRing+1);
				//second tri
				tris[triCounter+3]=baseV+(_vertsPerRing+1);
				tris[triCounter+5]=baseV+1;
				tris[triCounter+4]=baseV+(_vertsPerRing+1)+1;
				triCounter+=6;
				baseV++;
			}
		}

		m.vertices=vertices;
		m.triangles=tris;
		m.normals = norms;
		m.uv=uvs;
		m.RecalculateBounds();
		_meshF.sharedMesh=m;
		_meshC.sharedMesh=m;

		StartCoroutine(DestroyNextFrame(ringCenter.gameObject));

		Transform [] leaves = new Transform[transform.childCount];
		for(int i=0; i<transform.childCount; i++){
			leaves[i]=transform.GetChild(i);
		}
		StartCoroutine(DestroyNextFrame(leaves));

		Random.InitState(_leafSeed);
		for(int i=0; i<_numLeaves; i++){
			GeneratePalmLeaf(i);
		}
	}

	void GeneratePalmLeaf(int index){
		if(_leafSegments<2)
		{
			Debug.Log("Cannot create leaf with < 2 leaf segments");
			return;
		}
		if(_leafRes<2){
			Debug.Log("Cannot create leaf with < 2 resolution");
			return;
		}

		//gen game object
		GameObject leafGo = new GameObject("Leaf");
		Transform leafT = leafGo.transform;
		leafT.SetParent(transform);
		MeshFilter meshF = leafGo.AddComponent<MeshFilter>();
		MeshRenderer meshR = leafGo.AddComponent<MeshRenderer>();
		meshR.material=_palmLeafMat;
		Mesh m = new Mesh();

		//gen centers
		List<Vector3> centers = new List<Vector3>();
		List<Vector3> centerNormals = new List<Vector3>();
		float length = Random.Range(_leafSizeRange.x,_leafSizeRange.y);
		float maxWidth = Random.Range(_leafWidthRange.x,_leafWidthRange.y);
		float maxHeight = Random.Range(_leafHeightRange.x,_leafHeightRange.y);
		for(int i=0; i<_leafSegments; i++){
			float t01 = i/(float)(_leafSegments-1);
			Vector3 pos = Vector3.forward*length*t01;
			pos+=Vector3.down*_palmLeafSpineCurve.Evaluate(t01);
			centers.Add(pos);
			if(i==0)
				centerNormals.Add(Vector3.up);
			else{
				Vector3 spineVec=centers[i]-centers[i-1];
				centerNormals.Add(Vector3.Cross(spineVec,Vector3.right));
			}
		}

		//allocate some mem
		Vector3[] vertices = new Vector3[_leafRes*_leafSegments];
		int[] tris = new int[(_leafSegments-1)*(_leafRes)*6];
		Vector3[] norms = new Vector3[vertices.Length];
		Vector2[] uvs = new Vector2[vertices.Length];

		int vIndex=0;
		//set up verts
		for(int i=0; i<_leafSegments; i++){
			float i01 = i/(float)(_leafSegments-1);
			float width01 = _palmLeafWidthCurve.Evaluate(i01);
			float width = width01*maxWidth;
			for(int j=0;j<_leafRes; j++){
				float j01 = j/(float)(_leafRes-1);
				float jLeftRight=j01*2f-1f;
				Vector3 pos = centers[i];
				//position horizontally
				pos+=Vector3.right*width*jLeftRight;
				//at some point we will want to add some vertical offset
				float vert = _palmVertCurve.Evaluate(Mathf.Abs(jLeftRight))*width01;
				pos+=Vector3.up*vert*maxHeight;
				//we will also have to modify the normals to account for the vertical offset
				if(Mathf.Abs(jLeftRight)<0.0001f)
					norms[vIndex]=centerNormals[i];
				else{
					Vector3 vec = pos-centers[i];
					norms[vIndex]=Vector3.Cross(vec.normalized,Vector3.forward);
				}

				vertices[vIndex]=pos;
				//#temp - just take the spine normal
				uvs[vIndex]=new Vector2(j01,i01);
				vIndex++;
			}
		}

		//set up tris
		int tIndex=0;
		for(int i=0;i<_leafSegments-1; i++){
			for(int j=0;j<_leafRes-1; j++){
				vIndex=i*_leafRes+j;
				
				//first tri
				tris[tIndex]=vIndex;
				tris[tIndex+2]=vIndex+1;
				tris[tIndex+1]=vIndex+_leafRes;

				//second tri
				tris[tIndex+3]=vIndex+_leafRes;
				tris[tIndex+5]=vIndex+1;
				tris[tIndex+4]=vIndex+_leafRes+1;

				tIndex+=6;
			}
		}

		m.vertices=vertices;
		m.triangles=tris;
		m.normals = norms;
		m.uv=uvs;
		m.RecalculateBounds();
		meshF.sharedMesh=m;
		//_meshC.sharedMesh=m;

		leafT.localPosition=_treeTop;
		leafT.Rotate(Vector3.up*index*Random.Range(_palmSpinRange.x,_palmSpinRange.y));

		leafT.Rotate(Vector3.right*Random.Range(_palmPitchRange.x,_palmPitchRange.y));

	}

	IEnumerator DestroyNextFrame(GameObject go){
		yield return null;
		DestroyImmediate(go);
	}

	IEnumerator DestroyNextFrame(Transform[] ts){
		yield return null;
		foreach(Transform t in ts)
			DestroyImmediate(t.gameObject);
	}
}
