﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeBehaviour : MonoBehaviour
{
	MTree[] _trees;
	public MTree _curTree;
	int _state;
	public Vector2 _flySpeed;
	public float _flyArcHeight;
	Animator _anim;
	public float _flapDur;
	public AudioClip _flapSound;
	public Vector2 _flapSoundRange;
	TrailRenderer _trail;
	public float _fleeRadius;
	Bird _player;
	GroundForager _groundForager;
	float _hideTimer;
	float _hideTime;
	public Vector2 _hideTimeRange;
	public int _takeOffFlaps;
	Sing _sing;
	NPB _npb;

	void Awake(){
		_anim=GetComponent<Animator>();
		_trail=transform.GetComponentInChildren<TrailRenderer>();
		_player=GameManager._player;
		_groundForager=GetComponent<GroundForager>();
		_sing=transform.GetComponentInChildren<Sing>();
		_npb=GetComponent<NPB>();
	}

	void OnEnable(){
		_hideTimer=0f;
		_hideTime=Random.Range(_hideTimeRange.x,_hideTimeRange.y);
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		switch(_state){
			case 0:
				if(_groundForager!=null&&!_sing._male){
					_hideTimer+=Time.deltaTime;
					if(_hideTimer>=_hideTime){
						ReturnToGround();
						_hideTimer=0f;
					}
				}
				if((_player.transform.position-transform.position).sqrMagnitude<_fleeRadius*_fleeRadius){
					ScareIntoTree();
				}
				break;
			case 1:
				break;
			default:
				break;
		}
    }

	public void ScareIntoTree(bool silent=false){
		if(_state!=0)
			return;
		if(_npb._scanned)
			return;
		//MTree tree = GetNearestTree();
		MTree tree = GetRandomTree();
		if(tree!=null){
			_curTree=tree;
			Vector3 perch = tree.GetRandomPerch();
			StartCoroutine(FlyTo(perch));
		}
		if(!silent)
			_sing.FleeAlarm();
	}

	MTree GetNearestTree(){
		if(_trees==null)
			_trees = transform.parent.parent.Find("Planters").GetComponentsInChildren<MTree>();
		float minDistSqr=10000;
		MTree nearestTree=null;
		foreach(MTree t in _trees){
			if(_curTree!=null&&t==_curTree)
				continue;
			float sqrDist=(t.transform.position-transform.position).sqrMagnitude;
			if(sqrDist<minDistSqr){
				minDistSqr=sqrDist;
				nearestTree=t;
			}
		}
		return nearestTree;
	}

	MTree GetRandomTree(){
		if(_trees==null)
			_trees = transform.parent.parent.Find("Planters").GetComponentsInChildren<MTree>();
		MTree tree = null;
		while(tree==null||(_curTree!=null&&tree==_curTree)){
			tree=_trees[Random.Range(0,_trees.Length)];
		}
		return tree;
	}

	IEnumerator FlyTo(Vector3 p,bool returnToGround=false){
		_state=2;

		//transform.forward=_hopDir;
		_anim.SetBool("fly",true);
		_trail.emitting=true;
		Vector3 startPos=transform.position;
		Vector3 endPos=p;
		Vector3 diff=endPos-startPos;
		diff.y=0;
		diff.Normalize();
		transform.forward=diff;
		float dist = (endPos-startPos).magnitude;
		float timer=0;
		float flapTimer=100f;
		int flapCounter=0;
		float speed=Random.Range(_flySpeed.x,_flySpeed.y);
		float dur=dist/speed;
		float height=_flyArcHeight;
		while(timer<dur){
			timer+=Time.deltaTime;
			float frac=timer/dur;
			Vector3 pos=Vector3.Lerp(startPos,endPos,frac);
			//float yOffset=(-4*Mathf.Pow(frac-0.5f,2)+1)*height;
			float yOffset=Mathf.Abs(Mathf.Cos(frac*Mathf.PI*2f*dist*0.2f))*height;
			pos.y+=yOffset;
			transform.position=pos;

			flapTimer+=Time.deltaTime;
			if(flapCounter<_takeOffFlaps&&flapTimer>_flapDur){
				flapTimer=0f;
				Sfx.PlayOneShot3D(_flapSound,transform.position,Random.Range(_flapSoundRange.x,_flapSoundRange.y),0.3f,"Birds");
				flapCounter++;
			}
			yield return null;
		}
		transform.position=endPos;
		_anim.SetBool("fly",false);
		_trail.emitting=false;
		_state=0;
		if(returnToGround)
		{
			enabled=false;
			_groundForager.enabled=true;
		}
		_sing.Reset();
	}

	void ReturnToGround(){
		if(_npb._scanned)
			return;
		Vector3 groundPoint=_groundForager.GetRandomSpotOnGround();
		StartCoroutine(FlyTo(groundPoint,true));
	}
}
