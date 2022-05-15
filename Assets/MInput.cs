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
	public bool _mouseEnabled;

	float _triggerR;
	int _triggerRState;
	float _triggerL;
	int _triggerLState;
	
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
		_triggerR=Input.GetAxis("Right Trigger");
		_triggerL=Input.GetAxis("Left Trigger");
		switch(_triggerRState){
			case 0:
				if(_triggerR>0.5f)
					_triggerRState=1;
				break;
			case 1:
				if(_triggerR>0.5f)
					_triggerRState=2;
				else
					_triggerRState=3;
				break;
			case 2:
				if(_triggerR<0.5f)
					_triggerRState=3;
				break;
			case 3:
				if(_triggerR>0.5f)
					_triggerRState=1;
				else
					_triggerRState=0;
				break;
		}
		switch(_triggerLState){
			case 0:
				if(_triggerL>0.5f)
					_triggerLState=1;
				break;
			case 1:
				if(_triggerL>0.5f)
					_triggerLState=2;
				else
					_triggerLState=3;
				break;
			case 2:
				if(_triggerL<0.5f)
					_triggerLState=3;
				break;
			case 3:
				if(_triggerL>0.5f)
					_triggerLState=1;
				else
					_triggerLState=0;
				break;
		}
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

	public bool GetFeed(){
		if(_inputLocked)
			return false;
		return Input.GetButtonDown("Feed");
	}

	public bool GetJump(){
		if(_inputLocked)
			return false;
		return Input.GetButton("Jump")||Input.GetMouseButton(0);
	}

	public bool GetJumpDown(){
		if(_inputLocked)
			return false;
		return Input.GetButtonDown("Jump")||Input.GetMouseButtonDown(0);
	}

	public bool GetLandDown(){
		if(_inputLocked)
			return false;
		return Input.GetButtonDown("Cancel")||Input.GetMouseButtonDown(0);
	}

	public bool GetJumpUp(){
		if(_inputLocked)
			return false;
		return Input.GetButtonUp("Jump")||Input.GetMouseButtonUp(0);
	}

	public bool GetSingDown(){
		if(_inputLocked)
			return false;
		return Input.GetButtonDown("Sing");
	}

	public bool GetLowDown(){
		if(_inputLocked)
			return false;
		return Input.GetButtonDown("Low");
	}

	public bool GetLowUp(){
		if(_inputLocked)
			return false;
		return Input.GetButtonUp("Low");
	}

	public bool GetMidDown(){
		if(_inputLocked)
			return false;
		return Input.GetButtonDown("Middle");
	}

	public bool GetMidUp(){
		if(_inputLocked)
			return false;
		return Input.GetButtonUp("Middle");
	}

	public bool GetHighDown(){
		if(_inputLocked)
			return false;
		return Input.GetButtonDown("High");
	}

	public bool GetHighUp(){
		if(_inputLocked)
			return false;
		return Input.GetButtonUp("High");
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
		Vector2 mouseIn=Vector2.zero;
		if(_mouseEnabled)//&&Input.GetMouseButton(1))
		{
			mouseIn=new Vector2(Input.GetAxis("Mouse X"),-Input.GetAxis("Mouse Y"));
		}
		Vector2 joyIn=Vector2.zero;
#if UNITY_STANDALONE_OSX
		joyIn=new Vector2(Input.GetAxis("Joy Xmac"),Input.GetAxis("Joy X"));
#else
		joyIn=new Vector2(Input.GetAxis("Joy X"),Input.GetAxis("Joy Y"));
#endif
		return mouseIn*_mouseSens+joyIn*_joySens*Time.deltaTime;
	}

	public void LockInput(bool locked){
		_inputLocked=locked;
	}

	public bool InputLocked(){
		return _inputLocked;
	}

	public void EnableCursor(bool en){
		Cursor.visible=en;
		Cursor.lockState=en?CursorLockMode.None : CursorLockMode.Locked;
	}

	public bool GetTabDown(){
		if(_inputLocked)
			return false;
		return (Input.GetKeyDown(KeyCode.Tab)||_triggerRState==1);
	}

	public bool GetTabUp(){
		if(_inputLocked)
			return false;
		return (Input.GetKeyUp(KeyCode.Tab)||_triggerRState==3);
	}

	public bool GetTriggerLDown(){
		if(_inputLocked)
			return false;
		return (Input.GetKeyUp(KeyCode.Tab)||_triggerLState==1);
	}
	public bool GetTriggerRDown(){
		if(_inputLocked)
			return false;
		return _triggerRState==1;
	}
	public bool GetTriggerR(){
		if(_inputLocked)
			return false;
		return _triggerR>0.5f;
	}

	public bool GetResetDown(){
		if(_inputLocked)
			return false;
		return Input.GetKeyDown(KeyCode.R);

	}

	public bool GetInteractDown(){
		if(_inputLocked)
			return false;
		return (Input.GetButtonDown("Interact")||Input.GetMouseButtonDown(1));
	}
}
