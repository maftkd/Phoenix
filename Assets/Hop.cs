using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hop : MonoBehaviour
{
	float _hopTimer=0;
	[Range(0f,1f)]
	public float _inThresh;
	Vector3 _hopTarget;
	Vector3 _hopStart;
	Vector3 _midPos;
	public float _hopDist;
	public float _hopSpeed;
	float _hopTime;
	public float _hopHeight;
	Footstep _footstep;
	Trap _trap;
	public Transform _endZone;
	public bool _flightEnabled;
	public bool _startUpsideDown;
	Bird _bird;
	[Header("Wobble")]
	public float _wobbleTime;
	public float _postWobbleDelay;
	bool _wobble;
	AudioSource _thonk;
	AudioSource _woosh;
	AudioSource _rustle;
	Collider[] _cols;
	Fly _fly;
	public float _landVolumeMult;
	Vector3 _startPos;
	bool _firstFrame;
	public float _preJumpPeriod;
	public Transform _dustParts;
    // Start is called before the first frame update
    void Start()
    {
		_hopTime=_hopDist/_hopSpeed;
		_bird=GetComponent<Bird>();
		_fly=GetComponent<Fly>();
		_woosh=transform.Find("Woosh").GetComponent<AudioSource>();
		_thonk=transform.Find("Thonk").GetComponent<AudioSource>();
		_rustle=transform.Find("Rustle").GetComponent<AudioSource>();
		_cols = new Collider[4];
		_startPos=transform.position;
		_firstFrame=true;
    }

	void OnEnable(){
		if(_startUpsideDown)
			return;

		transform.rotation=Quaternion.identity;
		_hopTimer=0;
		//#temp
		RaycastHit hit;
		if(Physics.Raycast(transform.position+Vector3.up*Bird._height,Vector3.down, out hit, Bird._height*2f,1)){
			//transform.position=hit.point;
			_footstep=hit.transform.GetComponent<Footstep>();
		}
		if(_footstep!=null&&_fly!=null)
		{
			float vol = Mathf.Min(1f,-_fly._velocity.y*_landVolumeMult);
			Debug.Log("vol: "+vol);
			if(vol<0.5f)
				_footstep.Sound(transform.position);
			else{
				//heavy y velocity -> screen shake and louder land
				Camera.main.GetComponent<CameraShake>().Shake(vol);
				_footstep.Sound(transform.position,vol);
				Instantiate(_dustParts,transform.position,Quaternion.identity);
			}
		}
	}

    // Update is called once per frame
    void Update()
    {
		//#why does initial raycast fail to find a ground point if holding horizontal on start
		//#why does this only happen between levels and not at start?
		if(_firstFrame){
			_firstFrame=false;
			return;
		}
		float horIn = Input.GetAxis(GameManager._horizontalAxis);
		float h = horIn<0?-1f : 1f;
		float verIn = Input.GetAxis(GameManager._verticalAxis);
		if(_hopTimer<=0)
		{
			if(Mathf.Abs(horIn)>_inThresh){
				//horizontal hop
				Vector3 rayStart=transform.position+Vector3.up*Bird._height*2;
				if(horIn>0){
					//go right
					rayStart.x+=_hopDist;
				}
				else{
					//go left
					rayStart.x-=_hopDist;
				}
				RaycastHit hit;
				if(Physics.Raycast(rayStart,Vector3.down, out hit, Bird._height*4f,1)){
					//check for good ground spot
					if(Physics.OverlapSphereNonAlloc(hit.point+Vector3.up*Bird._height,0.01f,_cols,1)>0){
						//make sure no walls are in the way
						if(!_wobble)
						{
							Debug.Log("Hi");
							Debug.Log(_cols[0].name);
							StartCoroutine(Wobble(horIn));
						}
					}
					else{
						_hopTarget=hit.point;
						_hopTimer=_hopTime;
						_hopStart=transform.position;
						_midPos=Vector3.Lerp(_hopStart,_hopTarget,0.5f);
						_midPos+=Vector3.up*_hopHeight;
						_footstep=hit.transform.GetComponent<Footstep>();
						_trap=hit.transform.GetComponent<Trap>();
						transform.eulerAngles=Vector3.back*5f*h;
					}
				}
				else{
					if(Physics.Raycast(transform.position+Bird._height*Vector3.up,Vector3.right*h,out hit,_hopDist,1)){
						//walk against wall
						if(!_wobble)
							StartCoroutine(Wobble(horIn));
					}	
					else{
						//edge of cliff
						if(_flightEnabled){
							_woosh.Play();
							transform.eulerAngles=Vector3.back*45f*horIn;

							//enable fly
							_fly._freeFlap=true;
							_fly.enabled=true;
							
							//disable hop
							enabled=false;
						}
					}
				}
				if(_bird!=null){
					if(_bird.InWater())
						_bird.Sloosh();
				}
			}
			else if(verIn<-1f*_inThresh&&_flightEnabled){
				//jump down if on perch
				RaycastHit hit;
				if(!Physics.Raycast(transform.position+Vector3.up*0.01f,Vector3.down, out hit, Bird._height,1)){
					Debug.Log("wtf?");
					//nothing below player
					_rustle.Play();
					
					//enable fly
					_fly._killVert=true;
					_fly.enabled=true;
					
					//disable hop
					enabled=false;
				}
			}
		}
		if(_hopTimer>0)
		{
			float t = 1f-_hopTimer/_hopTime;
			if(t<0.5f)//first half of hop
				transform.position=Vector3.Lerp(_hopStart,_midPos,t*2f);
			else//second half of hop
				transform.position=Vector3.Lerp(_midPos,_hopTarget,(t-0.5f)*2f);

			_hopTimer-=Time.deltaTime;
			if(_hopTimer<=0){
				//end of hop
				if(_footstep!=null)
					_footstep.Sound(_hopTarget);
				if(_trap!=null)
					_trap.Activate(this);
				transform.position=_hopTarget;
				transform.eulerAngles=Vector3.zero;
			}
			if(_fly!=null&&(Input.GetButtonDown(GameManager._jumpButton)||Time.time-_fly._lastJumpPress<_preJumpPeriod)){
				if(_flightEnabled)
				{
					transform.eulerAngles=Vector3.back*45f*horIn;

					//enable fly
					_fly.enabled=true;
					
					//disable hop
					enabled=false;
				}
			}
			if((transform.position-_endZone.position).sqrMagnitude<1f)
			{
				Debug.Log("End zone!");
				GameManager._instance.NextLevel();
				//enabled=false;
			}
		}
		else{
			//not hopping
			if(Input.GetButtonDown(GameManager._jumpButton)&&_flightEnabled)
			{
				//enable fly
				_fly.enabled=true;
				
				//disable hop
				enabled=false;
			}
		}
    }//end of update

	IEnumerator Wobble(float hor){
		_wobble=true;
		float timer=0;
		float h=0;
		if(hor<0)
			h=-1f;
		else
			h=1f;
		RaycastHit hit;
		Vector3 rayStart=transform.position+Bird._height*Vector3.up;
		Vector3 dir=Vector3.right*h;
		Debug.Log("raycasting from: "+rayStart+", to "+dir);
		if(Physics.Raycast(rayStart,dir,out hit,_hopDist,1)){
			_thonk.Play();
		}	
		else{
			_woosh.Play();
		}
		while(timer<_wobbleTime){
			timer+=Time.deltaTime;
			float t = timer/_wobbleTime;
			if(t<0.5f)
				transform.eulerAngles=Vector3.back*45f*h*t*2f;
			else
				transform.eulerAngles=Vector3.back*45f*h*(1-(t-0.5f)*2f);
			yield return null;
		}
		transform.eulerAngles=Vector3.zero;
		yield return new WaitForSeconds(_postWobbleDelay);
		_wobble=false;
	}

	public void Reset(){
		transform.position=_startPos;
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.blue;
		if(_hopTarget!=null){
			Gizmos.DrawSphere(_hopTarget,0.25f);
		}
		Gizmos.color=Color.green;
		if(_endZone!=null){
			Gizmos.DrawWireSphere(_endZone.position,1.0f);
		}
	}
}
