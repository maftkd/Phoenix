using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hut : MonoBehaviour
{

	public enum HutType {BASIC, TWO_STORY, NONE};
	public HutType _hutType;

	[Header("Prefabs")]
	public Transform _post;
	public Transform _plank;
	public Transform _door;
	public Transform _window;
	public Transform _roof;

	[Header("Posts")]
	public float _riserHeight;
	public float _buildingHeight;
	public float _secondaryHeight;
	public float _postSpacing;
	public float _postWidth;

	[Header("Floor")]
	public float _floorThickness;

	[Header("Wall")]
	public int _sideWallPlanks;
	public float _plankSpacing;
	public Vector3 _plankSize;
	public int _sideLoftPlanks;
	public float _maxPlankRotation;
	public Vector2 _plankWidthRange;

	[Header("Door")]
	public Vector3 _doorSize;

	[Header("Balcony")]
	public float _balconyOffset;

	[Header("Windows")]
	public float _windowHeight;
	public Vector3 _windowSize;
	public float _loftWindowHeight;

	[Header("Roof")]
	public float _roofHeight;
	public float _roofHeightFudge;
	public float _roofOverhang;

	[Header("Generate")]
	public int _seed;
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

		switch(_hutType){
			case HutType.BASIC:
				GenBasic();
				break;
			case HutType.TWO_STORY:
				GenTwoStory();
				break;
			case HutType.NONE:
			default:
				break;
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

	public void GenTwoStory(){
		Random.InitState(_seed);

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
		
		//setup doors
		Transform doors = new GameObject("Doors").transform;
		doors.SetParent(transform);
		doors.localPosition=Vector3.zero;
		doors.localEulerAngles=Vector3.zero;
		
		//setup windows
		Transform windows = new GameObject("Windows").transform;
		windows.SetParent(transform);
		windows.localPosition=Vector3.zero;
		windows.localEulerAngles=Vector3.zero;

		//setup roof
		Transform roofs = new GameObject("Roofs").transform;
		roofs.SetParent(transform);
		roofs.localPosition=Vector3.zero;
		roofs.localEulerAngles=Vector3.zero;

		//some calculations
		float r = Mathf.Sqrt(2)*_postSpacing*0.5f;
		Vector3 center=transform.position;
		float baseHeight=center.y-_riserHeight;
		float totalHeight=_riserHeight+_buildingHeight;
		float secondaryHeight=_riserHeight+_secondaryHeight;

		Vector3 scale=Vector3.zero;

		//generate primary posts
		for(int i=0; i<4; i++){
			float t01 = i*0.25f+0.125f;
			float theta=t01*Mathf.PI*2f;
			Vector3 pos=center+transform.forward*Mathf.Sin(theta)*r+
				transform.right*Mathf.Cos(theta)*r;
			Transform post = Instantiate(_post,posts);
			post.localEulerAngles=Vector3.zero;
			scale=Vector3.one;
			scale.x=_postWidth;
			scale.z=_postWidth;
			scale.y=totalHeight*0.5f;//times 0.5 cuz cylinders are 2 high by default
			post.localScale=scale;
			pos.y=baseHeight+totalHeight*0.5f;
			post.position=pos;
		}

		//generate secondary posts
		for(int i=0; i<2; i++){
			float zOffset=_postSpacing*1.5f;
			Vector3 pos=center;
			pos.y=baseHeight+secondaryHeight*0.5f;
			pos-=transform.forward*zOffset;
			float right = i%2==0?1f:-1f;
			pos+=transform.right*_postSpacing*0.5f*right;
			Transform post = Instantiate(_post,posts);
			post.position=pos;
			scale=Vector3.one;
			scale.x=_postWidth;
			scale.z=_postWidth;
			scale.y=secondaryHeight*0.5f;//times 0.5 cuz cylinders are 2 high by default
			post.localScale=scale;
		}

		//generate floor
		Transform floor=Instantiate(_plank,planks);
		floor.position=center-transform.forward*_postSpacing*0.5f;
		scale = new Vector3(_postSpacing,_floorThickness,_postSpacing*2f);
		floor.localScale=scale;

		//side base walls
		for(int i=0; i<_sideWallPlanks;i++){
			for(int w=0;w<2;w++){
				Transform plank = Instantiate(_plank,planks);
				scale = Vector3.one;
				float plankRandom=Random.Range(_plankWidthRange.x,_plankWidthRange.y);
				scale.z=_postSpacing*2f*plankRandom;
				scale.x=_plankSize.x;
				scale.y=_plankSize.y;
				plank.localScale=scale;

				Vector3 pos=center+Vector3.up*i*_plankSpacing;
				pos-=transform.forward*_postSpacing*0.5f;
				float right=w%2==0?1f:-1f;
				pos+=transform.right*right*_postSpacing*0.5f;
				plank.position=pos;

				Quaternion randRot = Random.rotation;
				plank.rotation = Quaternion.Slerp(plank.rotation,randRot,Random.value*_maxPlankRotation);
			}
		}

		/*
		//generate secondary room ceiling
		Transform shortRoof=Instantiate(_plank,planks);
		shortRoof.position=center-transform.forward*_postSpacing+Vector3.up*_secondaryHeight;
		scale = new Vector3(_postSpacing,_floorThickness,_postSpacing);
		shortRoof.localScale=scale;
		*/

		//side loft walls
		for(int i=0; i<_sideLoftPlanks;i++){
			for(int w=0;w<2;w++){
				Transform plank = Instantiate(_plank,planks);
				scale = Vector3.one;
				float plankRandom=Random.Range(_plankWidthRange.x,_plankWidthRange.y);
				scale.z=_postSpacing*plankRandom;
				scale.x=_plankSize.x;
				scale.y=_plankSize.y;
				plank.localScale=scale;

				Vector3 pos=center+Vector3.up*(i*_plankSpacing+_secondaryHeight);
				float right=w%2==0?1f:-1f;
				pos+=transform.right*right*_postSpacing*0.5f;
				plank.position=pos;

				Quaternion randRot = Random.rotation;
				plank.rotation = Quaternion.Slerp(plank.rotation,randRot,Random.value*_maxPlankRotation);
			}
		}
		
		//generate front and back planks
		for(int i=0; i<_sideWallPlanks;i++){
			for(int f=0; f<2; f++){
				Transform plank = Instantiate(_plank,planks);
				scale = Vector3.one;
				scale.z=_plankSize.x;
				float plankRandom=Random.Range(_plankWidthRange.x,_plankWidthRange.y);
				scale.x=_postSpacing*plankRandom;
				scale.y=_plankSize.y;
				plank.localScale=scale;

				Vector3 pos=center+Vector3.up*i*_plankSpacing-transform.forward*_postSpacing*0.5f;
				float front=f%2==0?1f:-1f;
				pos+=transform.forward*_postSpacing*front;
				plank.position=pos;

				Quaternion randRot = Random.rotation;
				plank.rotation = Quaternion.Slerp(plank.rotation,randRot,Random.value*_maxPlankRotation);
			}
		}

		//generate loft front and back
		for(int i=0; i<_sideLoftPlanks;i++){
			for(int w=0;w<2;w++){
				Transform plank = Instantiate(_plank,planks);
				scale = Vector3.one;
				float plankRandom=Random.Range(_plankWidthRange.x,_plankWidthRange.y);
				scale.x=_postSpacing*plankRandom;
				scale.z=_plankSize.x;
				scale.y=_plankSize.y;
				plank.localScale=scale;

				Vector3 pos=center+Vector3.up*(i*_plankSpacing+_secondaryHeight);
				float front=w%2==0?1f:-1f;
				pos+=transform.forward*front*_postSpacing*0.5f;
				plank.position=pos;

				Quaternion randRot = Random.rotation;
				plank.rotation = Quaternion.Slerp(plank.rotation,randRot,Random.value*_maxPlankRotation);
			}
		}

		//front door
		Transform frontDoor=Instantiate(_door,doors);
		frontDoor.position=center+transform.forward*_postSpacing*0.5f+Vector3.up*_doorSize.y*0.5f;
		scale=Vector3.one;
		scale.y=_doorSize.y;
		scale.z=_doorSize.z;
		scale.x=_doorSize.x;
		frontDoor.localScale=scale;

		//loft door
		Transform loftDoor=Instantiate(_door,doors);
		loftDoor.position=center+transform.forward*_postSpacing*0.5f+Vector3.up*(_doorSize.y*0.5f+_secondaryHeight+_balconyOffset);
		loftDoor.localScale=scale;

		//side base windows
		for(int i=0;i<2;i++){
			Transform window = Instantiate(_window,windows);
			scale = Vector3.one;
			scale.z=_windowSize.x;
			scale.x=_windowSize.z;
			scale.y=_windowSize.y;
			window.localScale=scale;

			Vector3 pos=center+Vector3.up*_windowHeight;
			float right=i%2==0?1f:-1f;
			pos+=transform.right*_postSpacing*0.5f*right;
			window.position=pos;
		}

		//side loft windows
		for(int i=0;i<2;i++){
			Transform window = Instantiate(_window,windows);
			scale = Vector3.one;
			scale.z=_windowSize.x;
			scale.x=_windowSize.z;
			scale.y=_windowSize.y;
			window.localScale=scale;

			Vector3 pos=center+Vector3.up*_loftWindowHeight;
			float right=i%2==0?1f:-1f;
			pos+=transform.right*_postSpacing*0.5f*right;
			window.position=pos;
		}

		//side secondary windows
		for(int i=0;i<2;i++){
			Transform window = Instantiate(_window,windows);
			scale = Vector3.one;
			scale.z=_windowSize.x;
			scale.x=_windowSize.z;
			scale.y=_windowSize.y;
			window.localScale=scale;

			Vector3 pos=center+Vector3.up*_windowHeight;
			float right=i%2==0?1f:-1f;
			pos+=transform.right*_postSpacing*0.5f*right;
			pos-=transform.forward*_postSpacing;
			window.position=pos;
		}

		//generate roof
		Transform roof = Instantiate(_roof,roofs);
		roof.position=center+Vector3.up*_buildingHeight+Vector3.up*_roofHeightFudge;
		//roof.position+=transform.forward
		scale=Vector3.one;
		scale.y=_roofHeight;
		scale.x=_postSpacing;
		scale.z=_postSpacing;
		roof.localScale=scale;

		roof = Instantiate(_roof,roofs);
		roof.position=center+Vector3.up*_secondaryHeight-transform.forward*_postSpacing+Vector3.up*_roofHeightFudge;
		scale=Vector3.one;
		scale.y=_roofHeight;
		scale.x=_postSpacing;
		scale.z=_postSpacing;
		roof.localScale=scale;
		roof.Rotate(Vector3.up*180f);
	}

	public void GenBasic(){
		Random.InitState(_seed);

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
		
		//setup doors
		Transform doors = new GameObject("Doors").transform;
		doors.SetParent(transform);
		doors.localPosition=Vector3.zero;
		doors.localEulerAngles=Vector3.zero;
		
		//setup windows
		Transform windows = new GameObject("Windows").transform;
		windows.SetParent(transform);
		windows.localPosition=Vector3.zero;
		windows.localEulerAngles=Vector3.zero;

		//setup roof
		Transform roofs = new GameObject("Roofs").transform;
		roofs.SetParent(transform);
		roofs.localPosition=Vector3.zero;
		roofs.localEulerAngles=Vector3.zero;

		//some calculations
		float r = Mathf.Sqrt(2)*_postSpacing*0.5f;
		Vector3 center=transform.position;
		float baseHeight=center.y-_riserHeight;
		float totalHeight=_riserHeight+_secondaryHeight;

		Vector3 scale=Vector3.zero;

		//generate primary posts
		for(int i=0; i<4; i++){
			float t01 = i*0.25f+0.125f;
			float theta=t01*Mathf.PI*2f;
			Vector3 pos=center+transform.forward*Mathf.Sin(theta)*r+
				transform.right*Mathf.Cos(theta)*r;
			Transform post = Instantiate(_post,posts);
			post.localEulerAngles=Vector3.zero;
			scale=Vector3.one;
			scale.x=_postWidth;
			scale.z=_postWidth;
			scale.y=totalHeight*0.5f;//times 0.5 cuz cylinders are 2 high by default
			post.localScale=scale;
			pos.y=baseHeight+totalHeight*0.5f;
			post.position=pos;
		}

		//generate floor
		Transform floor=Instantiate(_plank,planks);
		floor.position=center;
		scale = new Vector3(_postSpacing,_floorThickness,_postSpacing);
		floor.localScale=scale;

		//side base walls
		for(int i=0; i<_sideWallPlanks;i++){
			for(int w=0;w<2;w++){
				Transform plank = Instantiate(_plank,planks);
				scale = Vector3.one;
				float plankRandom=Random.Range(_plankWidthRange.x,_plankWidthRange.y);
				scale.z=_postSpacing*plankRandom;
				scale.x=_plankSize.x;
				scale.y=_plankSize.y;
				plank.localScale=scale;

				Vector3 pos=center+Vector3.up*i*_plankSpacing;
				float right=w%2==0?1f:-1f;
				pos+=transform.right*right*_postSpacing*0.5f;
				plank.position=pos;

				Quaternion randRot = Random.rotation;
				plank.rotation = Quaternion.Slerp(plank.rotation,randRot,Random.value*_maxPlankRotation);
			}
		}

		//generate front and back planks
		for(int i=0; i<_sideWallPlanks;i++){
			for(int f=0; f<2; f++){
				Transform plank = Instantiate(_plank,planks);
				scale = Vector3.one;
				scale.z=_plankSize.x;
				float plankRandom=Random.Range(_plankWidthRange.x,_plankWidthRange.y);
				scale.x=_postSpacing*plankRandom;
				scale.y=_plankSize.y;
				plank.localScale=scale;

				Vector3 pos=center+Vector3.up*i*_plankSpacing;
				float front=f%2==0?1f:-1f;
				pos+=transform.forward*_postSpacing*0.5f*front;
				plank.position=pos;

				Quaternion randRot = Random.rotation;
				plank.rotation = Quaternion.Slerp(plank.rotation,randRot,Random.value*_maxPlankRotation);
			}
		}

		//front door
		Transform frontDoor=Instantiate(_door,doors);
		frontDoor.position=center+transform.forward*_postSpacing*0.5f+Vector3.up*_doorSize.y*0.5f;
		scale=Vector3.one;
		scale.y=_doorSize.y;
		scale.z=_doorSize.z;
		scale.x=_doorSize.x;
		frontDoor.localScale=scale;

		//side base windows
		for(int i=0;i<2;i++){
			Transform window = Instantiate(_window,windows);
			scale = Vector3.one;
			scale.z=_windowSize.x;
			scale.x=_windowSize.z;
			scale.y=_windowSize.y;
			window.localScale=scale;

			Vector3 pos=center+Vector3.up*_windowHeight;
			float right=i%2==0?1f:-1f;
			pos+=transform.right*_postSpacing*0.5f*right;
			window.position=pos;
		}

		//generate roof
		Transform roof = Instantiate(_roof,roofs);
		roof.position=center+Vector3.up*_secondaryHeight+Vector3.up*_roofHeightFudge;
		//roof.position+=transform.forward
		scale=Vector3.one;
		scale.y=_roofHeight;
		scale.x=_postSpacing;
		scale.z=_postSpacing;
		roof.localScale=scale;

	}
}
