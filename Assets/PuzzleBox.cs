using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PuzzleBox : MonoBehaviour
{
	Transform _effects;
	public UnityEvent _onSolved;
	SurroundCamHelper _surroundCam;
	GameObject _guideLine;
	float _revealDur=1f;
	MCamera _mCam;
	float _resetCamDelay=1f;
	public float _shotRadius;
	public float _shotDistance;
	public float _shotHeight;
	bool _shotTaken;
	Transform _player;
	public UnityEvent _onRevealed;
	public UnityEvent _onShot;
	public UnityEvent _onActivated;
	public bool _activateOnAwake;
	ForceField _forceField;
	public Transform _seedPrefab;
	public int _numRewards;
	public Vector2 _ejectForceRange;
	public float _ejectForceX;
	public float _ejectForceY;
	public Vector2 _seedDispenseDelayRange;
	public AudioClip _dispenseSound;
	public string _puzzleId;

	protected virtual void Awake(){
		Debug.Log("Starting puzzle box");
		_effects=transform.Find("Effects");
		_surroundCam=GetComponent<SurroundCamHelper>();
		_guideLine=transform.Find("GuideLine").gameObject;
		_mCam=Camera.main.transform.parent.GetComponent<MCamera>();
		_player=GameObject.FindGameObjectWithTag("Player").transform;
		_forceField=transform.GetComponentInChildren<ForceField>();
		if(_activateOnAwake)
			Activate();
		else
			_forceField.Activate();
	}

	protected virtual void OnEnable(){

	}

	protected virtual void OnDisable(){

	}

    // Start is called before the first frame update
    protected virtual void Start()
	{
        
    }

    // Update is called once per frame
    protected virtual void Update()
    {
		if(!_shotTaken&&(_player.position-transform.position).sqrMagnitude<_shotRadius*_shotRadius){
			StartCoroutine(FocusOnBox());
		}
    }

	public virtual void PuzzleSolved(){
		Debug.Log("Puzzle Solved");
		_onSolved.Invoke();
		_surroundCam.enabled=false;
		_guideLine.SetActive(false);
		if(_effects!=null)
			_effects.gameObject.SetActive(true);
		StartCoroutine(OpenBox());
	}

	protected virtual IEnumerator OpenBox(){
		yield return null;
	}

	public virtual void Reveal(){
		if(transform.parent!=null){
			StartCoroutine(RevealR());
		}
	}

	public virtual IEnumerator RevealR(){
		Transform carrier=transform.parent;
		Transform carrierMesh=carrier.GetChild(0);
		//clear parent
		transform.SetParent(null);
		carrierMesh.SetParent(null);
		float timer=0;
		Vector3 scale=Vector3.one;
		while(timer<_revealDur){
			timer+=Time.deltaTime;
			scale.y=1f-timer/_revealDur;
			carrierMesh.localScale=scale;
			yield return null;
		}
		Destroy(carrier.gameObject);
		Destroy(carrierMesh.gameObject);
		yield return new WaitForSeconds(_resetCamDelay);
		_mCam.DefaultCam();
		_onRevealed.Invoke();
	}

	public virtual IEnumerator FocusOnBox(){
		_shotTaken=true;
		yield return null;
		/*
		Vector3 dir=transform.position-_mCam.transform.position;
		dir.y=0;
		Vector3 targetPos=transform.position-dir.normalized*_shotDistance+Vector3.up*_shotHeight;
		//_mCam.TrackTargetFrom(transform,targetPos,transform.localScale.y*Vector3.up*0.5f);
		//_mCam.TrackTarget(transform,transform.localScale.y*0.5f*Vector3.up);
		_mCam.LetterBox(true);
		yield return new WaitForSeconds(3f);
		//_mCam.DefaultCam();
		_mCam.LetterBox(false);
		_onShot.Invoke();
		*/
	}

	public virtual void Activate(){
		_onActivated.Invoke();
		_surroundCam.enabled=true;
		_forceField.Deactivate();
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.green;
		Gizmos.DrawWireSphere(transform.position,_shotRadius);
	}
}
