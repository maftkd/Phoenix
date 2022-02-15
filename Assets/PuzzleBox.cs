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
	float _revealDur=1f;
	MCamera _mCam;
	MInput _mIn;
	float _resetCamDelay=1f;
	public float _shotRadius;
	public float _shotDistance;
	public float _shotHeight;
	bool _shotTaken;
	public UnityEvent _onRevealed;
	public UnityEvent _onShot;
	public UnityEvent _onActivated;
	public bool _activateOnAwake;
	ForceField _forceField;
	public string _puzzleId;
	public Bird _unlockBird;
	public float _liftDelay;
	public static PuzzleBox _latestPuzzle;
	bool _solved;
	public Cable _cable;
	Bird _player;
	Transform _box;
	Transform _key;
	Material _keyMat;

	protected virtual void Awake(){
		_effects=transform.Find("Effects");
		_surroundCam=GetComponent<SurroundCamHelper>();
		_mCam=GameManager._mCam;
		_mIn=GameManager._mIn;
		_forceField=transform.Find("ForceField").GetComponent<ForceField>();
		_box=transform.GetChild(0);
		_player=GameManager._player;
		_key=transform.Find("Key").transform;
		_keyMat=_key.GetChild(0).GetComponent<Renderer>().material;

		if(_activateOnAwake)
			Activate();
		else
		{
			_forceField.Activate();
		}

		Transform label = MUtility.FindRecursive(transform,"PuzzleLabel");
		label.GetComponent<Text>().text=_puzzleId;
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
		if(_unlockBird!=null && _player.IsPlayerInRange(transform,_surroundCam._outerRadius)){
			_unlockBird.Call();
			_player.CopyCall(_unlockBird);
			_unlockBird=null;
		}
    }

	public virtual void SolveSilent(){
		_onSolved.Invoke();
		_surroundCam.enabled=false;
		_solved=true;
		Destroy(_forceField.gameObject);
		GameManager._instance.PuzzleSolved(this);
	}

	public virtual void PuzzleSolved(){
		if(_solved)
			return;
		_onSolved.Invoke();
		_solved=true;
		_keyMat.SetFloat("_Powered",1);
		_surroundCam.enabled=false;
		//Destroy(_forceField.gameObject);
		if(_effects!=null)
			_effects.gameObject.SetActive(true);
		StartCoroutine(OpenBox());
		GameManager._instance.PuzzleSolved(this);
	}

	protected virtual IEnumerator OpenBox(){
		yield return new WaitForSeconds(_liftDelay);
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

	public virtual void Activate(){
		_onActivated.Invoke();
		//_surroundCam.enabled=true;
		_forceField.Deactivate();
		_latestPuzzle=this;

		_cable.FillNearPosition(transform.position);
	}

	public Transform GetPerch(){
		return transform.Find("Perch");
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.green;
		Gizmos.DrawWireSphere(transform.position,_shotRadius);
	}
}
