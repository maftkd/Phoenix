using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdHouse : MonoBehaviour
{
	Bird _player;
	GameObject _interior;
	GameObject _exterior;
	Transform _playerStart;
	Camera _doorCam;
	Transform _mainCam;
	bool _intActive;
	Transform [] _walls;
	public float _wallVisDot;
	public bool _swapInteriorExterior;
	bool _solved;
	Material _interiorRim;
	Material _exteriorRim;
	[Header("Solved effects")]
	public float _pulseDelay;
	public AudioClip _rewardSound;

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

		_interiorRim=_interior.transform.Find("Floor").GetComponent<Renderer>().materials[1];
		_exteriorRim=_exterior.transform.Find("House").GetComponent<Renderer>().materials[0];

		//may need these later
		/*
		Transform walls =_interior.transform.Find("Walls");
		_walls = new Transform[walls.childCount];
		for(int i=0;i<walls.childCount;i++)
			_walls[i]=walls.GetChild(i);
		_mainCam=Camera.main.transform;
			*/

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
	public void Exit(){
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
}
