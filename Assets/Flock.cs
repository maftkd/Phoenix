using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour
{

	public Transform _birdPrefab;
	public int _minBirds;
	public int _maxBirds;
	public Terrain _terrain;
	[HideInInspector]
	public Vector3 _center;
	Vector3 _prevCenter;
	public MBird [] _mBirds;
	public float _spawnRadius;
	public Vector2 _updateTimeRange;
	public float _walkChance;
	public float _walkRadius;
	public Vector2 _groundDurRange;
	float _groundTimer;
	public Vector2 _flightDurRange;
	float _flightTimer;
	public int _state;
	[Header("Boid behaviour")]
	public float _visualRange;
	public float _centering;
	public Vector3 _minZone;
	public Vector3 _maxZone;
	public float _margin;
	public float _turnFactor;
	public float _steerFactor;
	public float _minDistance;
	public float _avoidance;
	public float _matching;
	public float _avoidTerrain;
	public float _speedLimit;
	public float _minSpeed;
	public float _maxVertDot;
	public float _yVelDamp;
	public float _minDistToLand;
	public bool _nearTarget;

	[Header("Audio")]
	public AudioClip _flapSound;
	public int _minFlaps;
	public int _maxFlaps;
	public Vector2 _pitchRange;
	public Vector2 _flapDelayRange;

	[Header("Flock cam")]
	public bool _toggleCam;
	Camera _flockCam;
	FlockCam _fCam;
	Bird _player;

	[Header("interactivity")]
	public float _minPlayerDist;
	bool _playerNear;
	public Color _nearOutline;

	public  class MBird {
		public Transform _transform;
		public int _state;
		Animator _anim;
		float _updateTimer;
		Waddle _waddle;
		Flock _flock;
		Vector3 _velocity;
		Vector3 _targetPos;
		float _flightDur;
		Material _mat;
		Footstep _footstep;

		public MBird(Flock f, Transform t){
			_flock=f;
			_transform=t;
			_state=0;
			_anim=t.GetComponent<Animator>();
			_anim.SetFloat("cycleOffset",Random.value);
			_anim.SetFloat("idleSpeed",Random.Range(0.5f,1.5f));
			_anim.SetFloat("flySpeed",Random.Range(0.5f,1.5f));
			ResetUpdateTimer();
			_waddle=t.GetComponent<Waddle>();
			_waddle._onDoneWalking+=DoneWalking;
			_velocity=Vector3.zero;
			_mat=t.GetComponentInChildren<SkinnedMeshRenderer>().material;
		}

		public void Update(){
			switch(_state){
				case 0:
					_updateTimer-=Time.deltaTime;
					if(_updateTimer<=0f){
						float r=Random.value;
						if(r<_flock._walkChance){
							/*
							//pick random walk spot, and waddle to it
							Vector2 v = Random.insideUnitCircle*_flock._walkRadius;
							Vector3 pos = _transform.position + new Vector3(v.x,0,v.y);
							pos.y=_flock._terrain.SampleHeight(pos);
							if(pos.y>6f){
								_waddle.WaddleTo(pos);
								_state=1;
								ResetUpdateTimer();
							}
							*/
						}
						else{
							_anim.SetTrigger("peck");
							ResetUpdateTimer();
							/*
							RaycastHit hit;
							if(Physics.Raycast(_transform.position+Vector3.up*0.01f,Vector3.down,out hit, 0.02f,1)){
								_transform.position=hit.point;
								Footstep f = hit.transform.GetComponent<Footstep>();
								if(f!=null)
									f.Sound(_transform.position,0.1f);
							}
							*/
						}
					}
					break;
				case 1://walking
					break;
				case 2://boid flight
					//fly towards center
					FlyTowardsCenter();
					//avoid others
					AvoidOthers();
					//match velocity
					MatchVelocity();
					//check terrain
					CheckTerrain();
					//keep within bounds
					//KeepWithinBounds();
					//seek target
					SeekTarget();
					//limit speed
					LimitSpeed();
					//level out
					LevelOut();
					//add velocity
					_transform.position+=_velocity*Time.deltaTime;
					if(_velocity!=Vector3.zero)
						_transform.forward=_velocity;
					break;
				case 3://landing
					_updateTimer+=Time.deltaTime;
					if(_updateTimer>=_flightDur){
						DoneWalking();
						_anim.SetTrigger("land");
						_transform.position=_targetPos;
						Vector3 eulers=_transform.eulerAngles;
						eulers.x=0;
						_transform.eulerAngles=eulers;
						if(_footstep!=null)
							_footstep.Sound(_transform.position,0.2f);
					}
					else{
						_transform.position+=_velocity*Time.deltaTime;
					}
					break;
			}
		}

		void FlyTowardsCenter(){
			Vector3 center=Vector3.zero;
			int numNeighbors=0;
			for(int i=0; i<_flock._mBirds.Length;i++){
				MBird other=_flock._mBirds[i];
				if(other!=this&&(other._transform.position-_transform.position).sqrMagnitude<_flock._visualRange*_flock._visualRange){
					center+=other._transform.position;
					numNeighbors++;
				}
			}
			if(numNeighbors>0){
				center/=(float)numNeighbors;
				_velocity+=(center-_transform.position)*_flock._centering;
			}
		}

		void AvoidOthers(){
			Vector3 move = Vector3.zero;
			for(int i=0; i<_flock._mBirds.Length; i++){
				MBird other=_flock._mBirds[i];
				if(other!=this&&(other._transform.position-_transform.position).sqrMagnitude<_flock._minDistance*_flock._minDistance){
					move+=_transform.position-other._transform.position;
				}
			}
			_velocity+=move*_flock._avoidance;
		}

		void MatchVelocity(){
			Vector3 avgDelta = Vector3.zero;
			int numNeighbors=0;

			for(int i=0; i<_flock._mBirds.Length; i++){
				MBird other=_flock._mBirds[i];
				if(other!=this&&(other._transform.position-_transform.position).sqrMagnitude<_flock._visualRange*_flock._visualRange){
					avgDelta+=other._velocity;
					numNeighbors++;
				}
			}

			if(numNeighbors>0){
				avgDelta/=(float)numNeighbors;
				_velocity+=(avgDelta-_velocity)*_flock._matching;
			}
		}

		void CheckTerrain(){
			float height=_flock._terrain.SampleHeight(_transform.position);
			if(height>_transform.position.y)
				_velocity+=Vector3.up*_flock._avoidTerrain;
		}

		void LimitSpeed(){
			float speedSqr = _velocity.sqrMagnitude;
			if(speedSqr>_flock._speedLimit*_flock._speedLimit){
				_velocity=_velocity.normalized*_flock._speedLimit;
			}
			else if(speedSqr<_flock._minSpeed*_flock._minSpeed){
				_velocity=_velocity.normalized*_flock._minSpeed;
			}
		}

		void LevelOut(){
			if(Mathf.Abs(Vector3.Dot(_velocity,Vector3.up))>_flock._maxVertDot)
				_velocity.y*=_flock._yVelDamp;
		}

		/*
		void KeepWithinBounds(){
			Vector3 pos = _transform.position;
			if(pos.x<_flock._minZone.x+_flock._margin){
				_velocity.x+=_flock._turnFactor;
			}
			else if(pos.x>_flock._maxZone.x-_flock._margin){
				_velocity.x-=_flock._turnFactor;
			}
			if(pos.y<_flock._minZone.y+_flock._margin){
				_velocity.y+=_flock._turnFactor;
			}
			else if(pos.y>_flock._maxZone.y-_flock._margin){
				_velocity.y-=_flock._turnFactor;
			}
			if(pos.z<_flock._minZone.z+_flock._margin){
				_velocity.z+=_flock._turnFactor;
			}
			else if(pos.z>_flock._maxZone.z-_flock._margin){
				_velocity.z-=_flock._turnFactor;
			}
		}
		*/

		void SeekTarget(){
			Vector3 targetVel=_targetPos-_transform.position;
			float dist=targetVel.magnitude;
			if(dist<=_flock._minDistToLand){
				_flock._nearTarget=true;
			}
			targetVel.Normalize();
			if(Vector3.Dot(_velocity.normalized,targetVel)<1){
				_velocity=Vector3.Lerp(_velocity,targetVel*_velocity.magnitude,_flock._steerFactor*Time.deltaTime);
			}
		}

		void ResetUpdateTimer(){
			_updateTimer=Random.Range(_flock._updateTimeRange.x,_flock._updateTimeRange.y);
		}

		public void DoneWalking(){
			_state=0;
			ResetUpdateTimer();
		}
		
		public void Fly(Tree t){
			if(_state==1){
				_waddle.StopWaddling();
			}
			_state=2;
			_anim.SetTrigger("flyLoop");
			ResetUpdateTimer();
			Transform leaf = t.GetRandomPerch();
			Vector3 [] verts = leaf.GetComponent<MeshFilter>().sharedMesh.vertices;
			_targetPos =  leaf.TransformPoint(verts[Random.Range(0,verts.Length)]);
			_footstep=leaf.GetComponent<Footstep>();
		}

		public void LandNearTarget(){
			_state=3;
			Vector3 diff=_targetPos-_transform.position;
			float dist=diff.magnitude;
			diff.Normalize();
			_velocity=diff*_flock._speedLimit;
			_transform.LookAt(_targetPos);
			_flightDur=dist/_flock._speedLimit;
			_updateTimer=0f;
		}

		public bool HasLanded(){
			return _state<2;
		}

		public void SetHighlight(bool c){
			_mat.SetFloat("_Highlight", c?1f:0f);
		}
	}

	void Awake(){
		//pick random start spot
		float height=0;
		Vector3 center=transform.parent.position;
		float maxDist=Mathf.Abs(_terrain.transform.localPosition.x);
		int iters=0;
		int maxIters=100;
		Vector3 pos=Vector3.zero;
		//Random.InitState(0);
		Random.InitState((int)System.DateTime.Now.Ticks);
		while(height<6f&&iters<maxIters){
			Vector2 v = new Vector2(Random.Range(-maxDist,maxDist)+center.x,Random.Range(-maxDist,maxDist)+center.z);
			pos = new Vector3(v.x,0,v.y);
			pos.y=_terrain.SampleHeight(pos);
			height=pos.y;
			iters++;
		}
		if(iters>=maxIters){
			Debug.Log("Well poop");
			Destroy(gameObject);
		}
		else{
			_center=pos;
			_prevCenter=_center;
		}

		int numBirds=Random.Range(_minBirds,_maxBirds+1);
		_mBirds = new MBird[numBirds];
		//spawn some birds near center
		for(int i=0; i<numBirds; i++){
			height=0f;
			iters=0;
			Vector3 spawnPos=_center;
			while(height<6f&&iters<maxIters){
				Vector2 v = Random.insideUnitCircle*_spawnRadius;
				pos = _center + new Vector3(v.x,0,v.y);
				pos.y=_terrain.SampleHeight(pos);
				height=pos.y;
				iters++;
			}
			if(iters>=maxIters){
				Debug.Log("Well poop");
			}
			else{
				spawnPos=pos;
			}
			Transform bird = Instantiate(_birdPrefab,spawnPos,Quaternion.identity,transform);
			bird.Rotate(Vector3.up*Random.value*360f);
			MBird mb = new MBird(this,bird);
			_mBirds[i]=mb;
		}

		/*
		_groundTimer=Random.Range(_groundDurRange.x,_groundDurRange.y);
		//50% to start with flight
		if(Random.value<0.5f)
			_groundTimer=-1f;
			*/
		_groundTimer=-1f;
		_flockCam=transform.GetComponentInChildren<Camera>();
		_fCam=_flockCam.GetComponent<FlockCam>();
		_player=GameManager._player;
	}

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
		switch(_state){
			case 0://perched
				_groundTimer-=Time.deltaTime;
				if(_groundTimer<0){
					//start flying
					_state=1;
					Island i=transform.GetComponentInParent<Island>();
					Tree[] trees = i.GetComponentsInChildren<Tree>();
					Tree t = trees[Random.Range(0,trees.Length)];
					//choose tree target
					foreach(MBird mb in _mBirds)
						mb.Fly(t);
					StartCoroutine(FlapSounds());
					//_flightTimer=Random.Range(_flightDurRange.x,_flightDurRange.y);
				}
				break;
			case 1://flying
				if(_nearTarget){
					_state=2;
					foreach(MBird mb in _mBirds){
						//come in for landing
						mb.LandNearTarget();
					}
				}
				break;
			case 2://landing
				//if all birds have landed
				//go to state 0 - foraging
				bool allLanded=true;
				foreach(MBird mb in _mBirds){
					if(!mb.HasLanded())
					{
						allLanded=false;
						break;
					}
				}
				if(allLanded){
					_groundTimer=Random.Range(_groundDurRange.x,_groundDurRange.y);
					_state=0;
					//back to foraging
				}
				break;
			default:
				break;
		}

		foreach(MBird mb in _mBirds)
			mb.Update();

		//update center of boid
		_center=Vector3.zero;
		foreach(MBird mb in _mBirds)
			_center+=mb._transform.position;
		_center/=_mBirds.Length;

		//check for player close
		float sqrDst=(_center-_player.transform.position).sqrMagnitude;

		bool playerIn=sqrDst<_minPlayerDist*_minPlayerDist;
		if(playerIn!=_playerNear){
			foreach(MBird mb in _mBirds){
				mb.SetHighlight(playerIn);
			}
			_playerNear=playerIn;
		}
		if(playerIn)
			DebugScreen.Print("Near flock: "+name);

		//handle cam toggling
		if(_toggleCam==true){
			_toggleCam=false;
			if(!_fCam.enabled)
				GameManager._mCam.Transition(_flockCam,MCamera.Transitions.CUT_BACK);
			else
				_player.ResetState();
		}
    }

	/*
	public Vector3 GetRandomPositionOnGround(){
		Vector2 v = Random.insideUnitCircle*_innerRadius;
		Vector3 pos = transform.position+Vector3.right*v.x+Vector3.forward*v.y;
		pos.y=_terrain.SampleHeight(pos);
		return pos;
	}
	*/

	IEnumerator FlapSounds(){
		int numFlaps=Random.Range(_minFlaps,_maxFlaps);
		for(int i=0;i<numFlaps;i++)
		{
			Sfx.PlayOneShot3D(_flapSound,_center,Random.Range(_pitchRange.x,_pitchRange.y));
			float delay = Random.Range(_flapDelayRange.x,_flapDelayRange.y);
			yield return new WaitForSeconds(delay);
		}
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.magenta;
		Gizmos.DrawSphere(_center,0.25f);
		Gizmos.color=Color.blue;
		Gizmos.DrawWireCube(Vector3.Lerp(_minZone,_maxZone,0.5f)
				,new Vector3(_maxZone.x-_minZone.x,_maxZone.y-_minZone.y,_maxZone.z-_minZone.z));
	}
}
