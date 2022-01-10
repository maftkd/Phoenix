using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MInput : MonoBehaviour
{
	Vector3 _controllerInput;
	Vector3 _worldSpaceInput;
	public float _joySens;
	public float _mouseSens;
	public bool _inputLocked;
	
	void Awake(){

	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		CalcInputVector();
    }

	void CalcInputVector(){

		//controller-space
		float verIn = Input.GetAxis("Vertical");
		float horIn = Input.GetAxis("Horizontal");
		_controllerInput=new Vector3(horIn,verIn,0);
		RemapInputFromSquareToCircle();

		//world-space input
		Vector3 flatForward=transform.forward;
		flatForward.y=0;
		flatForward.Normalize();
		Vector3 flatRight=Vector3.Cross(Vector3.up,flatForward);
		_worldSpaceInput = Vector3.zero;
		_worldSpaceInput+=verIn*flatForward;
		_worldSpaceInput+=horIn*flatRight;
		float sqrMag=_worldSpaceInput.sqrMagnitude;
		if(sqrMag>1)
			_worldSpaceInput.Normalize();
	}

	public Vector3 GetInputDir(){
		if(_inputLocked)
			return Vector3.zero;
		return _worldSpaceInput;
	}

	public Vector3 GetControllerInput(){
		if(_inputLocked)
			return Vector3.zero;
		return _controllerInput;
	}

	public bool GetJump(){
		if(_inputLocked)
			return false;
		return Input.GetButton("Jump");
	}

	void RemapInputFromSquareToCircle(){
		float theta = Mathf.Atan2(_controllerInput.y,_controllerInput.x)*Mathf.Rad2Deg;
		float maxX=0;
		float maxY=0;
		if(45f-Mathf.Abs(theta)>=0||Mathf.Abs(theta)>=135f)
		{
			maxX=1f;
			maxY=Mathf.Abs(Mathf.Tan(theta*Mathf.Deg2Rad));
		}
		else
		{
			maxY=1f;
			maxX=Mathf.Abs(1f/(Mathf.Tan(theta*Mathf.Deg2Rad)));
		}
		float max = Mathf.Sqrt(maxX*maxX+maxY*maxY);
		//#hack - some reason we aren't seeing values above the 1.2 range I would expect 1*sqrt(2)
		max = Mathf.Min(1.2f,max);
		_controllerInput/=max;
	}

	public Vector2 GetMouseMotion(){
		if(_inputLocked)
			return Vector2.zero;
		Vector2 mouseIn=new Vector2(Input.GetAxis("Mouse X"),Input.GetAxis("Mouse Y"));
		Vector2 joyIn=new Vector2(Input.GetAxis("Joy X"),Input.GetAxis("Joy Y"));
		return mouseIn*_mouseSens+joyIn*_joySens*Time.deltaTime;
	}

	public void LockInput(bool locked){
		_inputLocked=locked;
	}
}
