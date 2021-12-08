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
	int _state;

	void OnDisable(){
		Cursor.lockState = CursorLockMode.None; 
		Cursor.visible=false;
	}

	void OnEnable(){
		Cursor.lockState = CursorLockMode.Locked; 
		Cursor.visible=false;
		_hopTimer=_hopDur+1f;
	}

	void Awake(){
		_instance = this;
	}
    // Start is called before the first frame update
    void Start()
    {
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
		
		//hop
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
				RaycastHit hit;
				if(Physics.Raycast(transform.position,Vector3.down,out hit,_height*2f,1)){
					Vector3 pos=hit.point;
					pos.y+=_height;
					transform.position=pos;
					Step();
				}
				else{
					Debug.Log("ahhhhh");
					enabled=false;
					Fly._instance.enabled=true;
				}
			}
			else if(Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)){
				enabled=false;
				Fly._instance.enabled=true;
			}
		}
    }

	public void ResetPosRot(){
		Vector3 pos = transform.position;
		//#temp hardcoded 10 for known starting terrain height
		pos.y=10+_height;
		transform.position=pos;
		transform.forward=Vector3.forward;
	}

	//this whole thing does nothing right now
	//except find the spot to draw the gizmo
	//The original point here was to allow shorter hops by pointing the cursor closer to the
	//player pos
	void FindHopTarget(Vector3 flatForward){
		RaycastHit hit;
		//#temp doesn't account height
		Vector3 basePos = transform.position-Vector3.up*_height;
		Vector3 defaultTarget=basePos+flatForward*_hopDist;
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

	void Step(){
		//_stepTimer=0;
		RaycastHit hit;
		if(Physics.Raycast(transform.position,Vector3.down, out hit, 1.65f,1)){
			if(hit.transform.GetComponent<Footstep>()!=null){
				hit.transform.GetComponent<Footstep>().Sound(
						transform.position+Vector3.down*1.6f);
			}
		}
		//Vector3 pos = transform.position;
	}

	public bool Hopping(){
		return _hopTimer>0;
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.red;
		if(_hopTarget!=null)
		{
			Gizmos.DrawSphere(_hopTarget,0.025f);
		}
	}
}
