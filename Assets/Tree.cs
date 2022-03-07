using System.Collections;
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
	}

	IEnumerator DestroyNextFrame(GameObject go){
		yield return null;
		DestroyImmediate(go);
	}
}
