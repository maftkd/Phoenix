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
	MInput _mIn;
	float _resetCamDelay=1f;
	public float _shotRadius;
	public float _shotDistance;
	public float _shotHeight;
	bool _shotTaken;
	public Transform _player;
	public UnityEvent _onRevealed;
	public UnityEvent _onShot;
	public UnityEvent _onActivated;
	public bool _activateOnAwake;
	ForceField _forceField;
	public string _puzzleId;
	GameManager _gm;
	public Bird _unlockBird;
	public Feeder _feeder;
	public float _liftDelay;
	public static PuzzleBox _latestPuzzle;

	protected virtual void Awake(){
		_effects=transform.Find("Effects");
		_surroundCam=GetComponent<SurroundCamHelper>();
		_guideLine=transform.Find("GuideLine").gameObject;
		_mCam=Camera.main.transform.parent.GetComponent<MCamera>();
		_mIn=_mCam.GetComponent<MInput>();
		_player=GameObject.FindGameObjectWithTag("Player").transform;
		_forceField=transform.GetComponentInChildren<ForceField>();
		if(_activateOnAwake)
			Activate();
		else
			_forceField.Activate();

		Transform label = MUtility.FindRecursive(transform,"PuzzleLabel");
		label.GetComponent<Text>().text=_puzzleId;
		_gm=GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
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

	public virtual void SolveSilent(){
		_onSolved.Invoke();
		_surroundCam.enabled=false;
		_guideLine.SetActive(false);
		Destroy(_forceField.gameObject);
		_gm.PuzzleSolved(this);
	}

	public virtual void PuzzleSolved(){
		_onSolved.Invoke();
		_surroundCam.enabled=false;
		_guideLine.SetActive(false);
		Destroy(_forceField.gameObject);
		if(_effects!=null)
			_effects.gameObject.SetActive(true);
		StartCoroutine(OpenBox());
		_gm.PuzzleSolved(this);
	}

	protected virtual IEnumerator OpenBox(){
		yield return new WaitForSeconds(_liftDelay);
		_feeder.Feed();
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
		_latestPuzzle=this;
	}

	public Transform GetPerch(){
		return transform.Find("Perch");
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.green;
		Gizmos.DrawWireSphere(transform.position,_shotRadius);
	}
}
