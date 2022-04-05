using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MEditor : MonoBehaviour
{
	Camera _editCam;
	int _state;

	public Transform _puzzlePrefab;
	Transform _puzzle;
	public float _rotationScale;

	void Awake(){
		_editCam=transform.GetComponentInChildren<Camera>();
		enabled=false;
		_puzzle = Instantiate(_puzzlePrefab);
		Collider [] cols = _puzzle.GetComponentsInChildren<Collider>();
		foreach(Collider c in cols)
			c.enabled=false;
		_puzzle.gameObject.SetActive(false);
	}

	void OnEnable(){
		GameManager._mCam.Transition(_editCam,MCamera.Transitions.CUT_BACK);
		DebugScreen.Print("Edit Mode", 0);
		DebugScreen.Print("", 1);
		GameManager._player.SetEditMode(true);
		_state=0;
	}

	void OnDisable(){
		DebugScreen.Print("", 0);
		GameManager._player.SetEditMode(false);
		if(_puzzle!=null)
			_puzzle.gameObject.SetActive(false);
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		switch(_state){
			case 0:
				if(Input.GetKeyDown(KeyCode.Tab)){
					_state++;
					DebugScreen.Print("Place Puzzle", 1);
					_puzzle.gameObject.SetActive(true);
				}
				break;
			case 1:
				RaycastHit hit;
				if(Physics.Raycast(_editCam.transform.position,_editCam.transform.forward,out hit, 10f, 1)){
					_puzzle.position=hit.point+Vector3.up;
				}
				_puzzle.Rotate(Vector3.up*Input.mouseScrollDelta.y*_rotationScale);
				if(Input.GetMouseButtonDown(0)){
					_state++;
					DebugScreen.Print("Set Grid Size", 1);
				}
				break;
			case 2:
				break;
			default:
				break;
		}
    }
}
