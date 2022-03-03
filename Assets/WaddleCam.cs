using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaddleCam : Shot
{
	Vector3 _dollyDir;
	public float _yOffset;
	public float _yLerp;
	public float _lookLerp;
	public RectTransform _sweetSpot;
	public RectTransform _playerPos;
	Vector2 _panBounds;
	Vector2 _dollyBounds;
	public float _panMult;
	public float _maxPan;
	public float _dollyMult;
	public float _maxDolly;
	Vector3 _position;
	Quaternion _rotation;
	[Header("Distance based constraints")]
	public float _maxDistance;
	public float _distanceMoveMult;
	public float _maxDistanceMoveMult;
	bool _debugLines;

	[Header("Warm up period")]
	[Range(0,1)]
	[Tooltip("Power is a multiplier across all movement. It ramps up based on warm up speed")]
	public float _power;
	public float _warmUpSpeed;

	protected override void Awake(){
		base.Awake();
		_player=GameManager._player;
		Vector2 refRes=new Vector2(1920,1080);
		Vector2 midPoint=refRes*0.5f;
		_panBounds=new Vector2(midPoint.x+_sweetSpot.offsetMin.x,midPoint.x+_sweetSpot.offsetMax.x)/refRes.x;
		_dollyBounds=new Vector2(midPoint.y+_sweetSpot.offsetMin.y, midPoint.y+_sweetSpot.offsetMax.y)/refRes.y;
		_position=transform.position;
		_rotation=transform.rotation;
		//_yOffset=transform.position.y-_player.transform.position.y;
		SetDebugLines(_debugLines);
	}

	void OnEnable(){
		_position=transform.position;
		_rotation=transform.rotation;
		_power=0;
		UpdateDolly();
	}

	//establish dolly direction
	void UpdateDolly(){
		_dollyDir=transform.forward;
		_dollyDir.y=0;
		_dollyDir.Normalize();
	}
    // Start is called before the first frame update
    protected override void Start()
    {
        
    }

    // Update is called once per frame
    protected override void Update()
    {
		//cancel out the parent/child effect
		transform.position=_position;
		transform.rotation=_rotation;

		//cast player to screen
		Vector3 viewPoint = _cam.WorldToViewportPoint(_player.transform.position+_player._size.y*Vector3.up);
		_playerPos.anchoredPosition=new Vector2(viewPoint.x*1920f,viewPoint.y*1080f);

		//check if player within sweet spot
		if(viewPoint.x<_panBounds.x)
		{
			//rotate as a function of distance from sweet spot
			float rotationAmount=viewPoint.x-_panBounds.x;
			rotationAmount=Mathf.Clamp(rotationAmount,-_maxPan,_maxPan)*_power;
			_rotation=Quaternion.Euler(0,rotationAmount*Time.deltaTime*_panMult,0)*_rotation;
			UpdateDolly();
		}
		else if(viewPoint.x>_panBounds.y)
		{
			float rotationAmount=viewPoint.x-_panBounds.y;
			rotationAmount=Mathf.Clamp(rotationAmount,-_maxPan,_maxPan)*_power;
			_rotation=Quaternion.Euler(0,rotationAmount*Time.deltaTime*_panMult,0)*_rotation;
			UpdateDolly();
		}
		if(viewPoint.y<_dollyBounds.x){
			float moveAmount=viewPoint.y-_dollyBounds.x;
			moveAmount=Mathf.Clamp(moveAmount,-_maxDolly,_maxDolly)*_power;
			_position+=_dollyDir*moveAmount*_dollyMult*Time.deltaTime;
		}
		else if(viewPoint.y>_dollyBounds.y){
			float moveAmount=viewPoint.y-_dollyBounds.y;
			moveAmount=Mathf.Clamp(moveAmount,-_maxDolly,_maxDolly)*_power;
			_position+=_dollyDir*moveAmount*_dollyMult*Time.deltaTime;
		}

		//check if player exceeds max distance
		float sqrDist=(transform.position-_player.transform.position).sqrMagnitude;
		if(sqrDist>_maxDistance*_maxDistance){
			float moveAmount=Mathf.Sqrt(sqrDist)-_maxDistance;
			moveAmount=Mathf.Clamp(moveAmount,-_maxDistanceMoveMult,_maxDistanceMoveMult)*_power;
			_position+=_dollyDir*moveAmount*_distanceMoveMult*Time.deltaTime;
		}
		//fix y offset
		_position.y=Mathf.Lerp(_position.y,_player.transform.position.y+_yOffset,_yLerp*Time.deltaTime*_power);

		//after all is said and done
		if(HandleMouseMotion()){
			_position=Vector3.Lerp(_position,transform.position,_lookLerp*Time.deltaTime);
			_rotation=Quaternion.Slerp(_rotation,transform.rotation,_lookLerp*Time.deltaTime);
		}

		//ramp up power
		if(_power<1f){
			_power+=Time.deltaTime*_warmUpSpeed;
			if(_power>1f)
				_power=1f;
		}
    }

	public void ToggleCamLines(){
		_debugLines=!_debugLines;
		SetDebugLines(_debugLines);
	}

	void SetDebugLines(bool on){
		RawImage [] images = transform.GetComponentsInChildren<RawImage>();
		foreach(RawImage ri in images)
			ri.enabled=on;
	}

	[ContextMenu("Check dist")]
	public void CheckDist(){
		_player=GameObject.FindGameObjectWithTag("Player").GetComponent<Bird>();
		float dst = (_player.transform.position-transform.position).magnitude;
		Debug.Log(dst);
	}
}
