using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PuzzleBox : MonoBehaviour
{
	Transform _effects;
	public UnityEvent _onSolved;
	public UnityEvent _onActivatingNextPuzzle;
	float _revealDur=1f;
	MInput _mIn;
	MCamera _mCam;
	float _resetCamDelay=1f;
	bool _shotTaken;
	public UnityEvent _onRevealed;
	public UnityEvent _onShot;
	public UnityEvent _onActivated;
	public bool _activateOnAwake;
	ForceField _forceField;
	public string _puzzleId;
	public float _liftDelay;
	public static PuzzleBox _latestPuzzle;
	bool _solved;
	public Cable _cable;
	Bird _player;
	Transform _box;
	Transform _boxLp;
	Transform _pistons;
	float _lodDist=10f;
	bool _prevInZone;
	Transform _key;
	Material _keyMat;
	public AudioClip _buzzClip;
	public PuzzleBox _nextPuzzle;
	PuzzleCam _puzzleCam;

	protected virtual void Awake(){
		_effects=transform.Find("Effects");
		_mIn=GameManager._mIn;
		_mCam=GameManager._mCam;
		_forceField=transform.Find("ForceField").GetComponent<ForceField>();
		_box=transform.Find("BoxDetailed");
		_boxLp=transform.Find("BoxLowDet");
		_player=GameManager._player;
		_key=_box.Find("Key").transform;
		_keyMat=_key.GetChild(0).GetComponent<Renderer>().material;
		_puzzleCam = transform.GetComponentInChildren<PuzzleCam>();

		//init
		_forceField.gameObject.SetActive(true);
		if(_activateOnAwake)
			Activate();
		else
		{
			_forceField.Activate();
		}

		Transform label = MUtility.FindRecursive(transform,"PuzzleLabel");
		label.GetComponent<Text>().text=_puzzleId;

		//set piston colors
		_pistons=transform.Find("Pistons");
		Material mat = _boxLp.GetComponent<MeshRenderer>().sharedMaterial;
		Transform bottomPanel=_box.Find("Bottom");
		foreach(Transform p in _pistons){
			Transform b = p.GetChild(0);
			b.GetComponent<MeshRenderer>().material=mat;
		}
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
		//lod check
		float sqrDist=(_player.transform.position-transform.position).sqrMagnitude;
		bool inZone=sqrDist<=_lodDist*_lodDist;
		if(inZone!=_prevInZone||Time.frameCount==1){
			_box.gameObject.SetActive(inZone);
			_boxLp.gameObject.SetActive(!inZone);
		}
		_prevInZone=inZone;
    }

	//#todo - there's some overlap between solve silent and puzzle solved
	public virtual void SolveSilent(){
		_onSolved.Invoke();
		_keyMat.SetFloat("_Powered",1);
		_solved=true;
		RemoveForceField();
		GameManager._instance.PuzzleSolved(this);
		SnapOpen();
		//destroy seed
		GameObject seed = transform.GetComponentInChildren<Seed>().gameObject;
		Destroy(seed);
		ActivateNextPuzzle();
	}

	public virtual void PuzzleSolved(){
		if(_solved)
			return;
		_onSolved.Invoke();
		_solved=true;
		RemoveForceField(true);
		_keyMat.SetFloat("_Powered",1);
		//Destroy(_forceField.gameObject);
		if(_effects!=null)
			_effects.gameObject.SetActive(true);
		StartCoroutine(OpenBox());
		GameManager._instance.PuzzleSolved(this);
		StartCoroutine(FlyAwayMate());
	}

	protected virtual IEnumerator OpenBox(){
		yield return new WaitForSeconds(_liftDelay);
		Transform bottomPanel=_box.Find("Bottom");
		bottomPanel.SetParent(transform);

		Vector3 startPos=_box.position;
		Vector3 endPos=startPos+Vector3.up*0.3f;
		float timer=0;
		Sfx.PlayOneShot3D(_buzzClip,startPos,1f+(Random.value*2-1)*0.2f);
		float dur=2f;
		while(timer<dur){
			timer+=Time.deltaTime;
			_box.position=Vector3.Lerp(startPos,endPos,timer/dur);
			yield return null;
		}
	}

	void SnapOpen(){
		Transform bottomPanel=_box.Find("Bottom");
		bottomPanel.SetParent(transform);
		Vector3 startPos=_box.position;
		Vector3 endPos=startPos+Vector3.up*0.3f;
		_box.position=endPos;
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
		_onRevealed.Invoke();
	}

	public void ActivateNextPuzzle(){
		//light up seed lines
		Circuit seedLines = transform.Find("SeedLines").GetComponentInChildren<Circuit>();
		seedLines.Power(true);

		_onActivatingNextPuzzle.Invoke();

		//activate next puzzle
		if(_nextPuzzle!=null)
			_nextPuzzle.Activate();
	}

	public virtual void Activate(){
		if(!gameObject.activeSelf)
			return;
		_onActivated.Invoke();
		_forceField.Deactivate(_activateOnAwake);
		_latestPuzzle=this;

		_cable.FillNearPosition(transform.position,_activateOnAwake);
	}

	public Transform GetPerch(){
		return transform.Find("Perch");
	}

	void RemoveForceField(bool transition=false){
		Destroy(_forceField.gameObject);
		_puzzleCam.enabled=false;
		if(transition)
			_player.TransitionToRelevantCamera();
	}

	public void ResetCamera(){
		_puzzleCam.ResetZone();
	}

	IEnumerator FlyAwayMate(){
		yield return new WaitForSeconds(3f);
		_player.FlyAwayMates();
	}

	void OnDrawGizmos(){
	}
}
