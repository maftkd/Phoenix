using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fly : MonoBehaviour
{
	public static Fly _instance;
	Hop _hop;
	public AudioClip _flip;
	public AudioClip _flap;
	AudioSource [] _flapSources;
	Collider[] _hitCheck;
	Vector3 _velocity;
	public Vector3 _flapAccel;
	public float _gravity;
	public float _mouseSens;
	public Vector3 _maxEulers;
	public float _maxSpeed;
	public int _numFlaps;
	int _maxFlaps;
	public float _airControlAccel;
	public Transform _flapContainer;
	public Transform _flapIcon;
	public Canvas _flapCanvas;
	RawImage [] _flapIcons;

	void Awake(){
		_instance=this;
		_flapSources = transform.Find("Flaps").GetComponentsInChildren<AudioSource>();
		_hitCheck = new Collider[5];
		_maxFlaps=_numFlaps;
	}

	void OnDisable(){
		Cursor.lockState = CursorLockMode.None; 
		Cursor.visible=false;
		_flapCanvas.enabled=false;
	}

	void OnEnable(){
		Cursor.lockState = CursorLockMode.Locked; 
		Cursor.visible=false;
		_flapCanvas.enabled=true;
		if(_flapContainer.childCount==0){
			//create flap icons and get references
			_flapIcons = new RawImage[_numFlaps];
			for(int i=0;i<_numFlaps;i++)
			{
				Transform fi = Instantiate(_flapIcon,_flapContainer);
				_flapIcons[i]=fi.GetComponent<RawImage>();
			}
		}
		//reset num flaps
		_numFlaps=_maxFlaps;
		if(_hop==null)
			_hop=Hop._instance;
		_velocity=Vector3.zero;

		Flip();
	}

    // Start is called before the first frame update
    void Start()
    {
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
		
		//wing flap
		if(Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)){
			Flip();
		}
		else if(Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)){
			Flap();
		}
		else{
			Vector3 input = new Vector3(Input.GetAxis("Horizontal"),0,Input.GetAxis("Vertical"));
			/*
			Vector3 flatForward = transform.forward;
			flatForward.y=0;
			flatForward.Normalize();
			Vector3 flatRight = Vector3.Cross(flatForward,Vector3.down);
			*/
			if(input.sqrMagnitude>0)
				input.Normalize();

			if(input!=Vector3.zero){
				Vector3 vDiff = (transform.forward*input.z+transform.right*input.x)*_airControlAccel*Time.deltaTime;
				//prevent air control from giving additional lift
				if(vDiff.y>0)
					vDiff.y=0;
				_velocity+=vDiff;
			}
		}

		if(_velocity.sqrMagnitude>_maxSpeed*_maxSpeed){
			_velocity.Normalize();
			_velocity*=_maxSpeed;
		}
		_velocity+=Vector3.down*_gravity*Time.deltaTime;
		transform.position+=_velocity*Time.deltaTime;

		//ground check
		if(Physics.OverlapSphereNonAlloc(transform.position+Vector3.down*_hop._height,_hop._height*0.5f,_hitCheck)>0){
			Debug.Log("ground check");
			enabled=false;
			_hop.enabled=true;
		}
    }

	void Flip(){
		if(_numFlaps<=0)
			return;
		foreach(AudioSource a in _flapSources){
			//play audio
			if(!a.isPlaying){
				a.clip=_flip;
				a.Play();
				break;
			}
		}
		_velocity+=_flapAccel.y*Vector3.up;
		_velocity+=transform.forward*_flapAccel.z;

		_numFlaps--;
		UpdateFlapGui();
	}

	void Flap(){
		foreach(AudioSource a in _flapSources){
			if(!a.isPlaying){
				a.clip=_flap;
				a.Play();
				break;
			}
		}
	}

	public void FlyTowards(Vector3 pos,float strength){
		float mag = _velocity.magnitude;
		Vector3 newVel = pos-transform.position;
		newVel.Normalize();
		newVel*=mag;
		_velocity=Vector3.Lerp(_velocity,newVel,strength);
		_numFlaps=_maxFlaps;
		UpdateFlapGui();
	}

	void UpdateFlapGui(){
		for(int i=0;i<_maxFlaps;i++){
			_flapIcons[i].enabled=_numFlaps>i;
		}
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.blue;
		//Gizmos.DrawRay(transform.position,_velocity);
		//Gizmos.DrawSphere(transform.position+transform.right*_turnRadius,0.5f);
		Gizmos.color=Color.red;
		//Gizmos.DrawRay(transform.position,_flatRight);
	}
}
