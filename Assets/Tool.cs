using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tool : MonoBehaviour
{
	Transform _toolCanvas;
	public ToolPath _path;
	Transform _tool;
	MInput _mIn;
	public float _moveSpeed;
	Vector3 _input;
	public float _inputLerp;

	void Awake(){
		_toolCanvas=transform.Find("ToolCanvas");
		_mIn=GameManager._mIn;
	}

	void OnEnable(){
		Debug.Log("We using tools!");
		_toolCanvas.gameObject.SetActive(true);
		_path.EnableCanvas(true);
	}

	void OnDisable(){
		_toolCanvas.gameObject.SetActive(false);
		_path.EnableCanvas(false);
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		Vector3 rawInput=_mIn.GetControllerInput();
		_input = Vector3.Lerp(_input,rawInput,_inputLerp*Time.deltaTime);
		if(_input!=Vector3.zero){
			//move
			_path.MoveTool(_input*_moveSpeed);
		}
		//_tool.position+=Vector3.right*rawInput.x*Time.deltaTime*(_moveSpeed*Screen.width);
		//_tool.position+=Vector3.up*rawInput.y*Time.deltaTime*(_moveSpeed*Screen.width);
		//_tool.position+=rawInput*Time.deltaTime*_moveSpeed;
    }
}
