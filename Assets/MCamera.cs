using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MCamera : MonoBehaviour
{
	public Vector3 _followOffset;
	public Vector3 _followBack;
	public float _lerpSpeed;
	public float _angleLerp;
	Vector3 _worldSpaceInput;
	Vector3 _controllerInput;
	Transform _player;
	Hop _hop;
	
	void Awake(){
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		_player=player.transform;
		_hop=_player.GetComponent<Hop>();
		_followBack=-_player.forward;
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		CalcInputVector();
		//assume bird is hopping
		//lerp rotation to match birds
		if(_controllerInput.y>=0){
			Quaternion curRot=transform.rotation;
			Vector3 birdBack=-_player.forward;
			transform.forward=_player.forward;
			Quaternion targetRot=transform.rotation;
			transform.rotation=Quaternion.Slerp(curRot,targetRot,_angleLerp*Time.deltaTime);
		}
		//lerp position based on rotation and follow offset vector
		Vector3 targetPos = _hop.GetCamTarget()-transform.forward*_followOffset.z+Vector3.up*_followOffset.y;
		transform.position = Vector3.Lerp(transform.position,targetPos,_lerpSpeed*Time.deltaTime);
    }

	void CalcInputVector(){
		float verIn = Input.GetAxis("Vertical");
		float horIn = Input.GetAxis("Horizontal");
		_controllerInput=new Vector3(horIn,verIn,0);
		Vector3 flatForward=transform.forward;
		flatForward.y=0;
		flatForward.Normalize();
		Vector3 flatRight=Vector3.Cross(Vector3.up,flatForward);
		_worldSpaceInput = Vector3.zero;
		if(Mathf.Abs(verIn)>_hop._inThresh)
			_worldSpaceInput+=verIn*flatForward;
		if(Mathf.Abs(horIn)>_hop._inThresh)
			_worldSpaceInput+=horIn*flatRight;
		float sqrMag=_worldSpaceInput.sqrMagnitude;
		if(sqrMag>1)
			_worldSpaceInput.Normalize();
	}

	public Vector3 GetInputDir(){
		return _worldSpaceInput;
	}
}
