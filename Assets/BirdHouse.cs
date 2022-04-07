using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
	Transform _mainCam;
	bool _intActive;
	public bool _swapInteriorExterior;
	bool _solved;
	Material _interiorRim;
	Material _exteriorRim;
	[Header("Solved effects")]
	public float _pulseDelay;
	public AudioClip _rewardSound;
	public BirdHouse _next;
	public GameObject _tempEnd;

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

		_interiorRim=_interior.transform.Find("Floor").GetComponent<Renderer>().materials[1];
		_exteriorRim=_exterior.transform.Find("House").GetComponent<Renderer>().materials[0];

		SetInteriorActive(false);
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

	public Transform GetPlayerStart(){
		return _playerStart;
	}

	public Camera GetDoorCam(){
		return _doorCam;
	}

	public void Solve(){
		if(_solved)
			return;
		StartCoroutine(PulseRim());
		Sfx.PlayOneShot3D(_rewardSound,transform.position);
		_solved=true;
	}

	IEnumerator PulseRim(){
		for(int i=0;i<5;i++){
			_interiorRim.SetColor("_EmissionColor",Color.black);
			yield return new WaitForSeconds(_pulseDelay);
			_interiorRim.SetColor("_EmissionColor",Color.white);
			yield return new WaitForSeconds(_pulseDelay);
		}
		_exteriorRim.SetColor("_EmissionColor",Color.white);
	}

	public Transform GetDoor(){
		if(_door==null){
			_doorCam=transform.GetComponentInChildren<Camera>();
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

	void OnDrawGizmos(){
		if(_next==null)
			return;
		if(_door==null){
			_doorCam=transform.GetComponentInChildren<Camera>();
			_door=_doorCam.transform.parent;
		}
		if(_door==null)
			return;

		Gizmos.color=Color.magenta;
		Vector3 posA=_door.position;
		Vector3 posB=_next.GetDoor().position;
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
	}
}
