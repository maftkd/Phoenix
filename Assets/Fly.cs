using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fly : MonoBehaviour
{
	public static Fly _instance;
	Hop _hop;
	public AudioClip _flip;
	public AudioClip _flap;
	AudioSource [] _flapSources;
	Collider[] _hitCheck;
	Vector3 _velocity;
	[Range(0,1f)]
	public float _upLerp;

	void Awake(){
		_instance=this;
		_flapSources = transform.Find("Flaps").GetComponentsInChildren<AudioSource>();
		_hitCheck = new Collider[5];
	}

	void OnDisable(){
		Cursor.lockState = CursorLockMode.None; 
		Cursor.visible=false;
	}

	void OnEnable(){
		Cursor.lockState = CursorLockMode.Locked; 
		Cursor.visible=false;
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

		//wing flap
		if(Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)){
			Flip();
		}
		else if(Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)){
			Flap();
		}

		transform.position+=_velocity*Time.deltaTime;


		//ground check
		if(Physics.OverlapSphereNonAlloc(transform.position,_hop._height,_hitCheck)>0){
			Debug.Log("ground check");
			enabled=false;
			_hop.enabled=true;
		}
        
    }

	void Flip(){
		foreach(AudioSource a in _flapSources){
			//play audio
			if(!a.isPlaying){
				a.clip=_flip;
				a.Play();
				break;
			}
		}

		_velocity = Vector3.Lerp(transform.forward,Vector3.up,_upLerp);
		_velocity.Normalize();
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

	void OnDrawGizmos(){
		Gizmos.color=Color.blue;
		Gizmos.DrawRay(transform.position,_velocity);
		Gizmos.color=Color.red;
		//Gizmos.DrawRay(transform.position,_flatRight);
	}
}
