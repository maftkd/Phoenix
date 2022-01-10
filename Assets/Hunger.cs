using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hunger : MonoBehaviour
{
	Bird _bird;
	int _state;
	Transform _player;
	public float _eatDistance;
	MCamera _mCam;
	public Vector3 _eatCamOffset;
	public float _waddleSpeed;
	public float _bowTime;
	public float _riseTime;
	Quaternion _standRot, _bowRot,_throwRot;
	public float _bowAngle;
	[Header("Seed flip")]
	public float _throwAngle;
	public float _flipTime;
	public float _flipSpeed;
	public float _flipHeight;
	public AnimationCurve _flipArc;
	Transform _seed;
	Vector3 _seedStart;
	public AudioClip _chalp;
	bool _eating;
	public Transform _eatParts;
	public float _effectsDur;

	void Awake(){
		_bird=GetComponent<Bird>();
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		_player=player.transform;
		_mCam = Camera.main.transform.parent.GetComponent<MCamera>();
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if(!_eating&&_bird._mate.HasSeed()&&_bird.IsPlayerClose(_bird._mate)&&!_bird.IsRunningAway()){
			StartCoroutine(EatSeed());
		}
    }

	IEnumerator EatSeed(){
		_eating=true;
		_bird._mate.Ground();
		Vector3 dir = _player.position-transform.position;
		Vector3 target=_player.position-dir.normalized*_eatDistance;
		_player.LookAt(transform);
		//i want dat seed
		_bird.WaddleTo(target,_waddleSpeed);
		_mCam.TrackTargetFrom(transform,
				_player.position+_player.right*_eatCamOffset.x+Vector3.up*_eatCamOffset.y+_player.forward*_eatCamOffset.z,Vector3.up*_eatCamOffset.y);
		yield return null;
		while(!_bird.ArrivedW())
			yield return null;

		_bird.StopWaddling();
		_standRot = transform.rotation;
		transform.Rotate(Vector3.right*_bowAngle);
		_bowRot=transform.rotation;
		transform.rotation=_standRot;
		transform.Rotate(Vector3.right*_throwAngle);
		_throwRot=transform.rotation;
		transform.rotation=_standRot;

		//bow
		float timer=0;
		while(timer<_bowTime){
			timer+=Time.deltaTime;
			transform.rotation=Quaternion.Slerp(_standRot,_bowRot,timer/_bowTime);
			yield return null;
		}
		transform.rotation=_bowRot;
		_bird.TakeSeedFromMate();
		_seed=_bird.GetSeed();

		//rise
		timer=0;
		while(timer<_riseTime){
			timer+=Time.deltaTime;
			transform.rotation=Quaternion.Slerp(_bowRot,_throwRot,timer/_riseTime);
			yield return null;
		}
		transform.rotation=_standRot;
		_seedStart=_seed.position;

		//throw
		timer=0;
		while(timer<_flipTime){
			timer+=Time.deltaTime;
			_seed.position=_seedStart+Vector3.up*_flipHeight*_flipArc.Evaluate(timer/_flipTime);
			_seed.Rotate(Vector3.right*_flipSpeed*Time.deltaTime);
			yield return null;
		}

		//play effects
		Sfx.PlayOneShot3D(_chalp,transform.position);
		Instantiate(_eatParts,_seed.position,Quaternion.identity);
		Destroy(_seed.gameObject);
		yield return new WaitForSeconds(_effectsDur);

		//return to main gameplay
		_eating=false;
		_mCam.DefaultCam();
		_bird.RunAwayNextPath();
	}
}
