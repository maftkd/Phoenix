using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BirdHouse : MonoBehaviour
{
	Bird _player;
	GameObject _interior;
	GameObject _exterior;
	Transform _playerStart;
	Camera _doorCam;
	Transform _door;
	Material _doorMatR;
	Material _doorMatL;
	Transform _mainCam;
	bool _intActive;
	public bool _swapInteriorExterior;
	public bool _solved;
	public GameObject _tempEnd;
	Transform _perch;
	public Cable _cable;
	public bool _activateOnAwake;
	public UnityEvent _onSolveExit;
	public UnityEvent _onStateChange;
	bool _exitAndSolve;
	TouchPlate [] _plates;
	[HideInInspector]
	public bool _activated;
	[HideInInspector]
	public string _houseId;

	void OnValidate(){
		if(_swapInteriorExterior){
			_interior = transform.Find("Interior").gameObject;
			_exterior = transform.Find("Exterior").gameObject;
			if(_interior==null||_exterior==null){
				Debug.Log("Cannot swap because couldn't find interior or exterior!");
			}
			else{
				SetInteriorActive(!_interior.gameObject.activeSelf);
			}
			_swapInteriorExterior=false;
		}
	}

	void Awake(){
		_player=GameManager._player;
		_interior=transform.Find("Interior").gameObject;
		_exterior=transform.Find("Exterior").gameObject;
		_interior.SetActive(true);
		_exterior.SetActive(true);
		_playerStart=MUtility.FindRecursive(_interior.transform,"PlayerStart");
		_doorCam=transform.GetComponentInChildren<Camera>();
		_door=_doorCam.transform.parent;
		_doorMatR=_door.Find("DoorRight").GetComponent<Renderer>().material;
		_doorMatL=_door.Find("DoorLeft").GetComponent<Renderer>().material;

		_plates=transform.GetComponentsInChildren<TouchPlate>();

		//do get references before this is called
		SetInteriorActive(false);

		/*
		if(_activateOnAwake)
			Activate(true);
			*/

		Island i = transform.GetComponentInParent<Island>();
		if(i!=null){
			int islandIndex = i.transform.GetSiblingIndex();
			int houseIndex = transform.GetSiblingIndex();
			_houseId=(islandIndex+1)+"."+(houseIndex+1);
		}
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame

	public void Enter(){
		_player.WalkInNestBox(transform,this);
	}
	public void Exit(int exitNumber){
		if(exitNumber>0)
		{
			_tempEnd.SetActive(true);
			StartCoroutine(FadeOutTemp());
			enabled=false;
			return;
		}

		_player.WalkOutNestBox(transform,this);
	}

	public void SetInteriorActive(bool intActive){
		_interior.SetActive(intActive);
		_exterior.SetActive(!intActive);
		_intActive=intActive;
	}

	public bool GetInteriorActive(){
		return _intActive;
	}

	public Transform GetPlayerStart(){
		return _playerStart;
	}

	public Camera GetDoorCam(){
		return _doorCam;
	}

	public void Solve(bool solved){
		//GameManager._instance.PuzzleSolved(this);
		_solved=solved;
		if(solved)
			_cable.Fill(transform.position);
		else
			_cable.Unfill(transform.position);
		_onStateChange.Invoke();
	}

	public void Activate(bool suppress=false){
		if(_cable!=null)
			_cable.FillNearPosition(_door.position,suppress);
		_doorMatR.SetFloat("_Lerp",1f);
		_doorMatL.SetFloat("_Lerp",1f);
		_activated=true;
	}

	public Transform GetDoor(){
		if(_door==null){
			_doorCam=transform.GetComponentInChildren<Camera>();
			if(_doorCam!=null)
				_door=_doorCam.transform.parent;
		}
		return _door;
	}

	IEnumerator FadeOutTemp(){
		_tempEnd.SetActive(true);
		CanvasGroup cg = _tempEnd.GetComponent<CanvasGroup>();
		cg.alpha=0f;
		float timer=0;
		while(timer<1f){
			timer+=Time.deltaTime;
			cg.alpha=timer;
			yield return null;
		}
		cg.alpha=1f;

	}

	public Transform GetPerchSpot(){
		if(_exterior==null)
			_exterior = transform.Find("Exterior").gameObject;
		_perch=_exterior.transform.Find("PerchSpot");
		return _perch;
	}

	public Transform GetExterior(){
		return _exterior.transform;
	}

	//solved and exited
	public void DoneWalkingOut(){
		if(_solved&&!_exitAndSolve){
			_onSolveExit.Invoke();
			//cardinal go next
			/*
			Cardinal card = transform.GetComponentInChildren<Cardinal>();
			if(card==null)
				card=FindObjectOfType<Cardinal>();
			card.FlyToNextHouse(this);
			if(_next!=null)
				_next.Activate();
				*/
			_player.SetCheckPoint();
			_exitAndSolve=true;
		}
	}

	public void CheckPlates(){
		bool allOn=true;
		foreach(TouchPlate tp in _plates){
			if(!tp._isOn&&tp.gameObject.activeSelf)
				allOn=false;
		}

		Solve(allOn);
		/*
		if(allOn)
			Solve();
		else if(_solved){
			Unsolve();
		}
		*/
	}

	void OnDrawGizmos(){
		if(_perch==null){
			GetPerchSpot();
		}
		if(_perch!=null){
			Gizmos.color=Color.red;
			Gizmos.DrawWireSphere(_perch.position,0.25f);
		}
		/*
		if(_next==null)
			return;
		if(_door==null){
			_doorCam=transform.GetComponentInChildren<Camera>();
			if(_doorCam!=null)
				_door=_doorCam.transform.parent;
		}
		Transform nextDoor=_next.GetDoor();
		if(_door==null||nextDoor==null)
			return;

		Gizmos.color=Color.magenta;
		Vector3 posA=_door.position;
		Vector3 posB=nextDoor.position;
		Gizmos.DrawLine(posA,posB);
		Vector3 mid=Vector3.Lerp(posA,posB,0.5f);
		float dist=(posA-posB).magnitude;
		float yDiff=(posB.y-posA.y);
		float xDiff=Mathf.Sqrt(dist*dist-yDiff*yDiff);

		Handles.BeginGUI();
		GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.magenta;
		Handles.Label(mid,"dist: "+dist+"\nflat: "+xDiff+"\nclimb: "+yDiff,style);
		Handles.EndGUI();
		*/
	}
}
