using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waddle : MonoBehaviour
{
	Animator _anim;
	MCamera _mCam;
	public float _walkSpeed;
	Bird _bird;
	public float _maxWalkSlope;
	public float _animSpeedMult;
	public float _stepVolume;
	float _stepTimer;

	void Awake(){
		_anim=GetComponent<Animator>();
		_mCam=FindObjectOfType<MCamera>();
		_bird = GetComponent<Bird>();
	}

	void OnEnable(){
		_anim.SetFloat("walkSpeed",0.1f);

		RaycastHit hit;
		if(Physics.Raycast(transform.position+Vector3.up*0.5f,Vector3.down,out hit, 1f,_bird._collisionLayer)){
			transform.position=hit.point;
			if(hit.transform.GetComponent<Footstep>()!=null)
				TakeStep(hit.transform.GetComponent<Footstep>());
		}
		_stepTimer=0;
	}
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
		_stepTimer+=Time.deltaTime;
		//Vector3 rawInput = _npc ? _npcInput : _mCam.GetInputDir();
		Vector3 rawInput = _mCam.GetInputDir();
		float animSpeed=rawInput.magnitude*_animSpeedMult;
		_anim.SetFloat("hopTime",animSpeed);
		transform.forward=rawInput;
		Vector3 move=transform.forward*rawInput.magnitude*Time.deltaTime*_walkSpeed*2*Mathf.Max(0,Vector3.Dot(transform.forward,rawInput));
		Vector3 targetPos = transform.position+move;
		RaycastHit hit;
		if(Physics.Raycast(targetPos+Vector3.up*0.5f,Vector3.down,out hit, 1f,_bird._collisionLayer)){
			targetPos.y=hit.point.y;
			float dy = (targetPos.y-transform.position.y);
			float dx = move.magnitude;
			float slope = dy/dx;
			if(slope<_maxWalkSlope)
				transform.position=hit.point;
			if(_stepTimer>=0.5f/animSpeed){
				TakeStep(hit.transform.GetComponent<Footstep>());
				_stepTimer=0;
			}
		}
    }

	void TakeStep(Footstep f){
		if(f!=null)
			f.Sound(transform.position,_stepVolume);
		_bird.MakeFootprint();
	}
}
