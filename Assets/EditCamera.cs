using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditCamera : Shot
{
	public float _moveSpeed;
	public float _shiftMult;
	public float _mouseSens;

	protected override void Awake(){
		base.Awake();
	}
	
    // Start is called before the first frame update
    protected override void Start()
    {

    }

	void OnEnable(){

	}

	protected override void Update(){
		base.Update();
		Vector3 move=Vector3.zero;
		move+=transform.forward*Input.GetAxis("Vertical");
		move+=transform.right*Input.GetAxis("Horizontal");
		move+=transform.up*Input.GetAxis("UpDown");
		if(move!=Vector3.zero&&move.sqrMagnitude>1f)
			move.Normalize();
		transform.position+=move*Time.deltaTime*_moveSpeed*(Input.GetKey(KeyCode.LeftShift)?_shiftMult : 1f);

		transform.Rotate(Vector3.up*Input.GetAxis("Mouse X")*_mouseSens, Space.World);
		transform.Rotate(Vector3.right*Input.GetAxis("Mouse Y")*_mouseSens*-1);
	}
}
