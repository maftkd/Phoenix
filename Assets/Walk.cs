using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walk : MonoBehaviour
{
	public float _mouseSens;
	public Vector3 _maxEulers;
	public float _walkSpeed;
	public float _runSpeed;
	public float _walkStepTime;
	public float _runStepTime;
	float _moveSpeed;
	float _prevMoveSpeed;
	float _stepTimer;
	bool _prevRun;
    // Start is called before the first frame update
    void Start()
    {
		/*
		RaycastHit hit;
		if(Physics.Raycast(transform.position,Vector3.down
		*/
		//Vector3 pos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
		//cursor
		if(Input.GetKeyDown(KeyCode.LeftControl)){
			Cursor.visible = !Cursor.visible;
			Cursor.lockState = Cursor.lockState==CursorLockMode.Locked? 
				CursorLockMode.None : CursorLockMode.Locked;
		}

		//look
		float mouseX = Input.GetAxis("Mouse X");
		float mouseY = Input.GetAxis("Mouse Y");
		Vector3 eulers = transform.eulerAngles;
		//change yEulers
		eulers.y+=mouseX*_mouseSens;
		//change xEulers
		eulers.x+=mouseY*_mouseSens*-1;
		if(eulers.x>180)
			eulers.x=-(360-eulers.x);
		else if(eulers.x<-180)
			eulers.x=(360+eulers.x);
		eulers.x = Mathf.Clamp(eulers.x,-_maxEulers.x,_maxEulers.x);
		//set eulers
		transform.eulerAngles=eulers;

		//walk
		Vector3 input = new Vector3(Input.GetAxis("Horizontal"),0,Input.GetAxis("Vertical"));
		Vector3 flatForward = transform.forward;
		flatForward.y=0;
		flatForward.Normalize();
		Vector3 flatRight = Vector3.Cross(flatForward,Vector3.down);
		if(input.sqrMagnitude>0)
			input.Normalize();
		//get speed
		bool run = Input.GetKey(KeyCode.LeftShift);
		float stepTime = run? _walkStepTime : _runStepTime;
		_moveSpeed=run ? _walkSpeed : _runSpeed;
		if(input==Vector3.zero)
			_moveSpeed=0;
		transform.position+=(flatForward*input.z+flatRight*input.x)
			*Time.deltaTime*_moveSpeed;

		//footsteps
		if(_prevMoveSpeed==0&&_moveSpeed>0){
			//start moving
			Step();
		}
		else if(_moveSpeed==0&&_prevMoveSpeed>0){
			//stop moving
			Step();
		}
		else if(_moveSpeed>0){
			if(_prevRun!=run){
				//switch between walk and run
				Step();
			}
			_stepTimer+=Time.deltaTime;
			if(_stepTimer>=stepTime){
				//regular steps
				Step();
			}
		}

		_prevMoveSpeed=_moveSpeed;
		_prevRun=run;
        
    }

	void Step(){
		_stepTimer=0;
		RaycastHit hit;
		if(Physics.Raycast(transform.position,Vector3.down, out hit, 1.65f,1)){
			if(hit.transform.GetComponent<Footstep>()!=null){
				hit.transform.GetComponent<Footstep>().Sound(transform.position+Vector3.down*1.6f);
			}
		}
		//Vector3 pos = transform.position;
	}
}
