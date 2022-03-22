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
	Boundary _boundary;
	public float _liftDelay;
	public float _liftAmount;
	public float _labelOpacity;
	public GameObject _beacon;
	public static PuzzleBox _latestPuzzle;
	bool _solved;
	public Cable _cable;
	Bird _player;
	Transform _box;
	Transform _boxLp;
	Transform _pistons;
	float _lodDist=10f;
	bool _prevInZone;
	Gate _window;
	//Material _keyMat;
	public AudioClip _buzzClip;
	public PuzzleBox _nextPuzzle;
	public GameObject _nestBox;
	[HideInInspector]
	public string _puzzleId;
	PuzzleCam _puzzleCam;
	public Transform _sparrow;

	protected virtual void Awake(){
		_mIn=GameManager._mIn;
		_mCam=GameManager._mCam;
		_box=transform.Find("BoxMesh");
		_player=GameManager._player;
		_window=_box.Find("Window Variant").GetComponent<Gate>();
		if(_nestBox==null)
			_window._onGateActivated.AddListener(PuzzleSolved);
		else
			_nestBox.SetActive(false);
		_puzzleCam = transform.GetComponentInChildren<PuzzleCam>();


		Transform label = MUtility.FindRecursive(transform,"PuzzleLabel");
		Transform researchLogo=MUtility.FindRecursive(transform,"ResearchLogo");
		Island i = transform.GetComponentInParent<Island>();
		int islandIndex = i.transform.GetSiblingIndex();
		int puzzleIndex = transform.GetSiblingIndex();
		_puzzleId=(islandIndex+1)+"."+(puzzleIndex+1);
		label.GetComponent<Text>().text=_puzzleId;

		//setup lod
		_boxLp=_box.Find("BoxLowDet");
		_boxLp.SetParent(transform);
		Material mat = _box.GetComponent<MeshRenderer>().sharedMaterial;
		_boxLp.GetComponent<MeshRenderer>().sharedMaterial=mat;

		//set force field color
		Color c = mat.color;
		_boundary=_box.Find("Boundary").GetComponent<Boundary>();
		_boundary.SetColor(c);
		Color labelC=c;
		labelC.a*=_labelOpacity;

		label.GetComponent<Text>().color=labelC;
		researchLogo.GetComponent<RawImage>().color=labelC;


		//init
		_boundary.gameObject.SetActive(false);
		if(_activateOnAwake)
			Activate();
		else
		{
			ActivateBeacon(false);
			ActivateElements(false);
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
		//_keyMat.SetFloat("_Powered",1);
		_solved=true;
		Destroy(_boundary.gameObject);
		ActivateBeacon(false);
		GameManager._instance.PuzzleSolved(this);
		ActivateNextPuzzle(true);
	}

	public virtual void PuzzleSolved(){
		if(_solved)
			return;
		_onSolved.Invoke();
		_solved=true;
		Destroy(_boundary.gameObject);
		ActivateBeacon(false);
		//_keyMat.SetFloat("_Powered",1);
		if(_effects!=null)
			_effects.gameObject.SetActive(true);
		//StartCoroutine(OpenBox());
		_player.TransitionToRelevantCamera();
		GameManager._instance.PuzzleSolved(this);
		ActivateNextPuzzle();
		SpawnSparrow();
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

	public void ActivateNextPuzzle(bool silent=false){
		if(_nextPuzzle!=null)
			_nextPuzzle.Activate(silent);
		_onActivatingNextPuzzle.Invoke();
	}

	public virtual void Activate(bool silent=false){
		if(!gameObject.activeSelf)
			return;
		_onActivated.Invoke();
		_boundary.gameObject.SetActive(true);
		_latestPuzzle=this;

		_cable.FillNearPosition(transform.position,_activateOnAwake||silent);
		ActivateBeacon(true);

		ActivateElements(true);
		//activate lines
		//
		//
	}

	public void ActivateElements(bool active){
		Circuit [] circuits = transform.GetComponentsInChildren<Circuit>();
		PressurePlate [] plates = transform.GetComponentsInChildren<PressurePlate>();
		Gate [] gates = transform.GetComponentsInChildren<Gate>();
		/*
		LineRenderer [] lines = transform.GetComponentsInChildren<LineRenderer>();

		foreach(LineRenderer l in lines)
			l.gameObject.SetActive(active);
		foreach(PressurePlate p in plates)
			p.gameObject.SetActive(active);
		foreach(Gate g in gates)
			g.gameObject.SetActive(active);
			*/
		foreach(Circuit c in circuits){
			c.Activate(active);
		}
		foreach(PressurePlate pp in plates){
			pp.Activate(active);
		}
		foreach(Gate g in gates){
			g.Activate(active);
		}
	}

	public Transform GetPerch(){
		return transform.Find("Perch");
	}

	public void ResetCamera(){
		_puzzleCam.ResetZone();
	}


	void ActivateBeacon(bool active){
		if(_beacon!=null)
			_beacon.SetActive(active);
	}

	public void EnterNestBox(){
		if(_nestBox==null){
			Debug.Log("Error cannot enter nest box, nest box is null");
			return;
		}
		Debug.Log("entering nest box");
		_player.WalkInNestBox(_box,_nestBox);
	}

	public void ExitNestBox(){
		if(_nestBox==null){
			Debug.Log("Error cannot exit nest box, nest box is null");
			return;
		}
		Debug.Log("Exit nest box");
		_player.WalkOutNestBox(_box,_nestBox);
	}

	public void SpawnSparrow(){
		Transform sparrow=Instantiate(_sparrow,transform);
		Transform perch = _window.transform.Find("Perch");
		perch.gameObject.SetActive(true);
		sparrow.position=perch.position;
		sparrow.forward=-_window.transform.forward;
	}

	void OnDrawGizmos(){
	}
}
