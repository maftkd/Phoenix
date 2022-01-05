using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tool : MonoBehaviour
{
	Transform _toolCanvas;
	public ToolPath _path;
	Transform _tool;
	MCamera _mCam;
	public float _moveSpeed;
	Vector3 _input;
	public float _inputLerp;

	void Awake(){
		_toolCanvas=transform.Find("ToolCanvas");
		_mCam=Camera.main.GetComponent<MCamera>();
	}

	void OnEnable(){
		Debug.Log("We using tools!");
		_toolCanvas.gameObject.SetActive(true);
		_path.EnableCanvas();
	}
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		Vector3 rawInput=_mCam.GetControllerInput();
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
