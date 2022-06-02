using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waddle : MonoBehaviour
{
	Animator _anim;
	MInput _mIn;
	public float _walkSpeed;
	Bird _bird;
	LayerMask _colLayer;
	float _size;
	public float _maxWalkSlope;
	public float _animSpeedMult;
	public float _stepVolume;
	public Vector2 _wadePitchRange;
	float _stepTimer;
	Vector3 _input;
	Vector3 _rawInput;
	public float _inputSmoothLerp;
	public float _slerp;
	public float _minInput;
	public float _minSlopeToCheckSpeed;
	float _knockBackTimer;
	[Header("Knock back")]
	public float _knockBackTime;
	Vector3 _knockBackDir;
	public float _knockBackSpeedMult;
	Collider [] _cols;

	//npc
	Terrain _terrain;
	Vector3 _destination;
	public bool _npc;
	Vector3 _npcInput;
	float _timeEstimate;
	float _walkTimer;

	public delegate void WalkEvent();
	public event WalkEvent _onDoneWalking;

	WaddleCam _cam;

	void Awake(){
		_anim=GetComponent<Animator>();
		_mIn = GameManager._mIn;
		_bird = GetComponent<Bird>();
		_cols = new Collider[2];
		_cam=transform.GetComponentInChildren<WaddleCam>();
		_colLayer=_bird!=null?_bird._collisionLayer : (LayerMask)1;
		_size=_bird!=null?_bird._size.y:GetComponent<SphereCollider>().radius*transform.localScale.x;
	}

	void OnEnable(){
		_anim.SetFloat("walkSpeed",0.1f);

		//ground at start
		RaycastHit hit;
		if(Physics.Raycast(transform.position+Vector3.up*0.01f,Vector3.down,out hit, 0.02f,_colLayer)){
			transform.position=hit.point;
			if(hit.transform.GetComponent<Footstep>()!=null)
				TakeStep(hit.transform);
		}
		_stepTimer=0;
		_knockBackTimer=0;
		_input=_mIn.GetInputDir();
	}

	void OnDisable(){
	}
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
		_stepTimer+=Time.deltaTime;
		_walkTimer+=Time.deltaTime;
		if(_knockBackTimer<=0){
			_rawInput = _npc? _npcInput : _mIn.GetInputDir();
			_input=Vector3.Lerp(_input,_rawInput,_inputSmoothLerp*Time.deltaTime);
			if(_input.sqrMagnitude<_minInput*_minInput)
				return;
		}
		else{
			_knockBackTimer-=Time.deltaTime;
			if(_knockBackTimer<=0){
				return;
			}
		}
		float animSpeed=_input.magnitude*_animSpeedMult;
		Vector3 move=Vector3.zero;
		if(_knockBackTimer<=0)
		{
			Quaternion curRot=transform.rotation;
			transform.forward=_input;
			Quaternion endRot=transform.rotation;
			transform.rotation=Quaternion.Slerp(curRot,endRot,_slerp*Time.deltaTime);
			move=transform.forward*_input.magnitude*Time.deltaTime*_walkSpeed*Mathf.Max(0,Vector3.Dot(transform.forward,_input));
			_anim.SetFloat("walkSpeed",animSpeed);
		}
		else
		{
			move=_input*Time.deltaTime*_walkSpeed;
		}

		Vector3 targetPos = transform.position+move;

		RaycastHit hit;
		if(Physics.Raycast(targetPos+Vector3.up*_size,Vector3.down,out hit, 
					_size*1.5f,_colLayer)){
			//raycast to ground
			targetPos.y=hit.point.y;
			float dy = (targetPos.y-transform.position.y);
			float dx = move.magnitude;
			float slope = dy/dx;
			if(slope<_maxWalkSlope)
			{
				if(slope>_minSlopeToCheckSpeed){
					Vector3 dir=hit.point-transform.position;
					targetPos=transform.position+dir.normalized*_walkSpeed*Time.deltaTime;
					if(Physics.Raycast(targetPos+Vector3.up*_size,Vector3.down,out hit, _size*1.5f,_colLayer)){
						transform.position=hit.point;
					}
					else{
						//Debug.Log("yo");
					}
				}
				else
				{
					transform.position=hit.point;
				}
			}
			else{
			}
			//always ground?
			if(_bird!=null)
				_bird.Ground(false);
			if(_stepTimer>=0.5f/animSpeed){
				TakeStep(hit.transform);
				_stepTimer=0;
			}

			if(_npc){
				if(Arrived()){
					if(_onDoneWalking!=null)
						_onDoneWalking.Invoke();
					StopWaddling();
				}
			}
		}
		else{
			if(_bird!=null&&!_bird.IsGrounded()){
				_anim.SetFloat("walkSpeed",0f);
				Debug.Log("erm?");
				_bird.StartHopping(true);
			}
		}
    }

	void TakeStep(Transform t){
		if(t==null)
			return;
		Footstep f = t.GetComponent<Footstep>();
		if(f!=null&&!_npc)
		{
			if(_bird!=null&&_bird._inWater)
			{
				f.Sound(transform.position,-1f,Random.Range(_wadePitchRange.x,_wadePitchRange.y));
			}
			else
				f.Sound(transform.position,_stepVolume);
		}
		if(_bird!=null)
			_bird.MakeFootprint(t);
		if(_npc){
			//recalibrate
			float mag = _npcInput.magnitude;
			Vector3 diff = _destination-transform.position;
			diff.y=0;
			diff.Normalize();
			_npcInput=diff*mag;
		}
	}

	public bool IsWaddling(){
		return _input.sqrMagnitude>=_minInput*_minInput||_rawInput.sqrMagnitude>_minInput*_minInput;
	}
	
	public bool IsKnockBack(){
		return _knockBackTimer>0;
	}

	public void KnockBack(Vector3 dir){
		float mag = _input.magnitude;
		Vector3 normIn = _input.normalized;
		//figure out which dot is the properly reflected angle,
		//I'm not really sure why we aren't just using Vector3 reflect, but w/e
		Vector3 optionA = Vector3.Cross(dir,Vector3.up);
		Vector3 optionB = Vector3.Cross(dir,Vector3.down);
		float dotA=Vector3.Dot(optionA,normIn);
		float dotB=Vector3.Dot(optionB,normIn);
		if(dotA>dotB){
			_input=optionA*mag;
		}
		else
			_input=optionB*mag;
	}

	public void WaddleTo(Vector3 target,float speed=-1f){
		speed=speed<0?_walkSpeed : speed;
		_destination=target;
		RaycastHit hit;
		if(Physics.Raycast(_destination+Vector3.up*_size*0.5f,Vector3.down, out hit,1f,_colLayer)){
			_destination.y=hit.point.y;
		}
		Vector3 diff = _destination-transform.position;
		diff.y=0;
		diff.Normalize();
		_npcInput=diff;
		_timeEstimate=(_destination-transform.position).magnitude/_walkSpeed;
		_walkTimer=0;
		enabled=true;
	}

	public bool Arrived(){
		float sqrDst=(transform.position-_destination).sqrMagnitude;
		bool closeEnough=sqrDst<_size*_size;
		return  closeEnough||(_walkTimer>_timeEstimate);
	}

	public void StopWaddling(){
		//transform.position=_destination;
		_npcInput=Vector3.zero;
		_anim.SetFloat("walkSpeed",0f);
		enabled=false;
	}

	public void ToggleCamLines(){
		_cam.ToggleCamLines();
	}

	void OnDrawGizmos(){
		if(_destination!=null)
		{
			Gizmos.color=Color.red;
			Gizmos.DrawWireSphere(_destination,0.1f);
		}
	}
}
