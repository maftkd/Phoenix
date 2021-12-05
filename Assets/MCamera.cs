using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MCamera : MonoBehaviour
{
	public float _rotateAmount;
	Transform _mainCam;
	float _readTimer;
	int _state;

	void OnEnable(){
		Debug.Log("camera's enabled baby");
		if(Walk._instance==null)
			return;
		Walk._instance.enabled=false;
		if(Crosshair._instance==null)
			return;
		Crosshair._instance.enabled=false;
		//position and rotation
		_mainCam = Camera.main.transform;
		transform.position = _mainCam.position+_mainCam.forward*0.25f;
		transform.rotation = _mainCam.rotation;
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		switch(_state){
			default:
			case 0:
				//inspection
				_readTimer+=Time.deltaTime;
				transform.rotation=_mainCam.rotation;
				Vector3 mouse = Input.mousePosition;
				transform.Rotate(new Vector3(mouse.y/Screen.height-0.5f,-(mouse.x/Screen.width-0.5f),0)*_rotateAmount);
				if(Input.anyKeyDown && _readTimer>1f){
					_state=1;
					Walk._instance.enabled=true;
					Crosshair._instance.enabled=true;
					Hand h = Hand._instance;
					transform.SetParent(h.transform);
					transform.localPosition=Vector3.zero;
					transform.localEulerAngles=Vector3.zero;
				}
				break;
			case 1:
				//idle
				break;
		}
        
    }
}
