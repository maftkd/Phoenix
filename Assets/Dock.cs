using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dock : MonoBehaviour
{
	[Header("Prefabs")]
	public Transform _post;
	public Transform _plank;

	[Header("Planks")]
	public int _plankSeed;
	public int _numPlanks;
	public float _plankSpacing;
	public Vector3 _minPlankScale;
	public Vector3 _maxPlankScale;
	public float _maxPlankRotation;

	[Header("Posts")]
	public int _postFreq;
	public Vector2 _postWidthRange;
	public float _postHeight;
	public Vector2 _postOffsetRange;
	public float _postSpacing;

	[Header("Supports")]
	public float _supportSpacing;
	public Vector3 _supportScale;
	public float _supportSink;

	[Header("Generate")]
	public bool _regen;
	public bool _autoRegen;

	void OnValidate(){
		if(_regen||_autoRegen){
			Regen();
			_regen=false;
		}
	}

	void Regen(){
		//clear old
		StartCoroutine(Clear());

		//setup planks
		Transform planks = new GameObject("Planks").transform;
		planks.SetParent(transform);
		planks.localPosition=Vector3.zero;
		planks.localEulerAngles=Vector3.zero;
		
		//setup posts
		Transform posts = new GameObject("Posts").transform;
		posts.SetParent(transform);
		posts.localPosition=Vector3.zero;
		posts.localEulerAngles=Vector3.zero;
		
		//setup supports
		Transform supports = new GameObject("Supports").transform;
		supports.SetParent(transform);
		supports.localPosition=Vector3.zero;
		supports.localEulerAngles=Vector3.zero;

		Random.InitState(_plankSeed);
		for(int i=0;i<_numPlanks; i++){
			Transform plank = Instantiate(_plank,planks);
			plank.localEulerAngles=Vector3.zero;
			plank.localPosition=Vector3.forward*i*_plankSpacing;
			float xScale = Mathf.Lerp(_minPlankScale.x,_maxPlankScale.x,Random.value);
			float yScale = Mathf.Lerp(_minPlankScale.y,_maxPlankScale.y,Random.value);
			float zScale = Mathf.Lerp(_minPlankScale.z,_maxPlankScale.z,Random.value);
			plank.localScale= new Vector3(xScale,yScale,zScale);

			Quaternion randRot = Random.rotation;
			plank.rotation = Quaternion.Slerp(plank.rotation,randRot,Random.value*_maxPlankRotation);

			//post
			if(i%_postFreq!=0)
				continue;
			for(int p=0; p<2; p++){
				Transform postA = Instantiate(_post,posts);
				postA.localEulerAngles=Vector3.zero;
				Vector3 postScale=Vector3.one;
				float width= Random.Range(_postWidthRange.x,_postWidthRange.y);
				postScale.x=width;
				postScale.z=width;
				postScale.y=_postHeight*0.5f;//posts are 2 high by default
				postA.localScale=postScale;
				float right=p%2==0?1f : -1f;
				right*=_postSpacing;
				postA.localPosition=plank.localPosition+Vector3.down*postA.localScale.y;
				postA.position+=plank.right*plank.localScale.x*right;
				float vertOffset=Random.Range(_postOffsetRange.x,_postOffsetRange.y);
				postA.position+=vertOffset*Vector3.up;
			}
		}

		//supports
		for(int i=0;i<2; i++){
			Transform support = Instantiate(_plank,supports);
			support.localEulerAngles=Vector3.zero;
			support.localPosition=Vector3.forward*_numPlanks*0.5f*_plankSpacing;
			float right = i%2==0? 1f : -1f;
			right*=_supportSpacing;
			support.position+=support.right*right;
			support.position+=Vector3.down*_supportSink;
			Vector3 scale=Vector3.one;
			scale.z=_numPlanks*_plankSpacing*_supportScale.z;
			scale.y=_supportScale.y;
			scale.x=_supportScale.x;
			support.localScale=scale;
		}
	}

	IEnumerator Clear(){
		int numChildren=transform.childCount;
		Transform [] children = new Transform[numChildren];
		for(int i=0;i<numChildren; i++){
			children[i]=transform.GetChild(i);
		}

		yield return null;

		for(int i=0; i<numChildren; i++){
			DestroyImmediate(children[i].gameObject);
		}
	}
}
