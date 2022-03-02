using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class River : MonoBehaviour
{
	MeshRenderer _meshR;
	MeshFilter _meshF;
	List<Vector3> _points;
	List<Vector3> _normals;

	void Start(){
		AudioSource s = transform.GetComponentInChildren<AudioSource>();
		s.pitch=Random.Range(0.9f,1.1f);
		//s.Play((ulong)(Random.value*(s.clip.samples/s.clip.length)));
		s.PlayDelayed(Random.value);
	}

	public void Setup(List<Vector3> points,List<Vector3> normals,Material mat,float width,float recess,AudioClip clip){
		Debug.Log("Setting up a river with: "+points.Count+" points");
		_points=new List<Vector3>(points);
		_normals=new List<Vector3>(normals);
		_meshR=gameObject.AddComponent<MeshRenderer>();
		_meshR.material=mat;
		_meshR.shadowCastingMode=ShadowCastingMode.Off;
		_meshF=gameObject.AddComponent<MeshFilter>();
		if(_points.Count<2){
			Debug.Log("Cannot create river with less than 2 points");
			return;
		}

		GameObject audioGo = new GameObject("RiverAudio");
		audioGo.transform.SetParent(transform);
		AudioSource audio = audioGo.AddComponent<AudioSource>();
		audio.clip=clip;
		audio.spatialBlend=1f;
		audio.loop=true;
		audio.volume=0.1f;
		audio.playOnAwake=false;
		audio.maxDistance=10f;
		audioGo.transform.position=_points[_points.Count/2];

		Mesh m = new Mesh();
		int numCenters=_points.Count;
		Vector3[] vertices = new Vector3[numCenters*2];
		int[] tris = new int[(numCenters-1)*6];
		Vector3[] norms = new Vector3[vertices.Length];
		Vector2[] uvs = new Vector2[vertices.Length];

		//vertices
		int vertexCounter=0;
		Vector3 forward=Vector3.zero;
		for(int i=0;i<numCenters; i++){
			float t01 = i/(float)(numCenters-1);
			if(i<numCenters-1)
				forward=(_points[i+1]-_points[i]).normalized;
			//else assume previous forward
			Vector3 right = Vector3.Cross(_normals[i],forward);
			Vector3 p1 = _points[i]-right*width*0.5f;
			Vector3 p2 = _points[i]+right*width*0.5f;
			vertices[vertexCounter]=p1-Vector3.up*recess;
			vertices[vertexCounter+1]=p2-Vector3.up*recess;
			//vertices[vertexCounter]=p1-_normals[i]*recess;
			//vertices[vertexCounter+1]=p2-_normals[i]*recess;
			/*
			float recessMult=i==numCenters-1? -1f : 1f;
			recessMult*=0.1f;
			vertices[vertexCounter]=p1-_normals[i]*recess*recessMult;
			vertices[vertexCounter+1]=p2-_normals[i]*recess*recessMult;
			*/
			vertexCounter+=2;
		}

		//triangles
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

		//normals
		for(int i=0; i<vertices.Length; i++){
			int centerIndex=i/2;
			norms[i]=_normals[centerIndex];
			uvs[i]=new Vector2(i%2,centerIndex/(float)numCenters);
		}

		//uvs
		float dist=0;
		for(int i=0; i<numCenters; i++){
			if(i>0)
				dist+=(_points[i]-_points[i-1]).magnitude;
			uvs[2*i]=new Vector2(0,dist);
			uvs[2*i+1]=new Vector2(1,dist);
		}

		m.vertices=vertices;
		m.triangles=tris;
		m.normals = norms;
		m.uv=uvs;
		m.RecalculateBounds();
		_meshF.sharedMesh=m;
		/*
		if(_mc!=null)
			_mc.sharedMesh=m;
			*/

	}

	void OnDrawGizmos(){
		/*
		if(_points!=null&&_normals!=null){
			for(int i=0; i<_points.Count; i++){
				Gizmos.color=Color.blue;
				Gizmos.DrawSphere(_points[i],0.2f);
				Gizmos.color=Color.red;
				Gizmos.DrawRay(_points[i],_normals[i]);
			}
		}
		*/
	}
}
