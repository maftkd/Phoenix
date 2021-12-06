using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hop : MonoBehaviour
{
	public float _mouseSens;
	public Vector3 _maxEulers;
	public AnimationCurve _hopCurve;
	public float _hopDist;
	public float _hopHeight;
	public float _hopSpeed;
	public float _height;
	float _hopTimer;
	float _hopDur;
	float _prevH;
	Vector3 _hopForward;
	Vector3 _hopTarget;
	public static Hop _instance;

	void OnDisable(){
		Cursor.lockState = CursorLockMode.None; 
		Cursor.visible=false;
	}

	void OnEnable(){
		Cursor.lockState = CursorLockMode.Locked; 
		Cursor.visible=false;
	}
    // Start is called before the first frame update
    void Start()
    {
		_instance = this;
		_hopDur = _hopDist/_hopSpeed;
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

		if(input!=Vector3.zero&& _hopTimer==0){
			//start a new hop
			_hopTimer=0.0001f;
			_hopForward=flatForward*input.z+flatRight*input.x;
			_prevH=0;
			FindHopTarget(flatForward);
		}
		else if(input==Vector3.zero&&_hopTimer==0){
			//not hopping
			FindHopTarget(flatForward);

		}

		//hop movement
		if(_hopTimer>0){
			transform.position+=_hopForward*Time.deltaTime;
			float h = _hopCurve.Evaluate(_hopTimer/_hopDur);
			float dh = h-_prevH;
			transform.position+=dh*Vector3.up*_hopHeight;
			_prevH=h;
			_hopTimer+=Time.deltaTime;
			if(_hopTimer>=_hopDur){
				_hopTimer=0;
				Vector3 pos = transform.position;
				pos.y=_height;
				transform.position=pos;
			}
		}

		//temp
		//transform.position+=(flatForward*input.z+flatRight*input.x)
		//	*Time.deltaTime*0.5f;
		/*
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
		*/
    }

	public void ResetPosRot(){
		Vector3 pos = transform.position;
		pos.y=_height;
		transform.position=pos;
		transform.forward=Vector3.forward;
	}

	void FindHopTarget(Vector3 flatForward){
		RaycastHit hit;
		//#temp doesn't account height
		Vector3 basePos = transform.position-Vector3.up*_height;
		Vector3 defaultTarget=basePos+flatForward*_hopDist;
		bool useDefault=true;
		if(Physics.Raycast(transform.position,transform.forward,out hit,1f,1)){
			_hopTarget=hit.point;
			if((hit.point-basePos).sqrMagnitude>_hopDist*_hopDist){
				//check distance
				_hopTarget=defaultTarget;
			}
			else{
				_hopTarget=hit.point;
			}
		}
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.red;
		if(_hopTarget!=null)
		{
			Gizmos.DrawSphere(_hopTarget,0.025f);
		}
	}
}
