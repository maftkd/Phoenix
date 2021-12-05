using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Letter : MonoBehaviour
{
	public float _rotateAmount;
	Transform _mainCam;
	float _readTimer;

	void OnEnable(){
		Debug.Log("letter's enabled baby");
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

	void OnDisable(){
		Debug.Log("letters disabled baby");
		if(Walk._instance==null)
			return;
		Walk._instance.enabled=true;
		if(Crosshair._instance==null)
			return;
		Crosshair._instance.enabled=true;
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		_readTimer+=Time.deltaTime;
		transform.rotation=_mainCam.rotation;
		Vector3 mouse = Input.mousePosition;
		transform.Rotate(new Vector3(mouse.y/Screen.height-0.5f,-(mouse.x/Screen.width-0.5f),0)*_rotateAmount);
		if(Input.anyKeyDown&&_readTimer>1f){
			//player presses button to end reading the letter
			transform.parent.GetComponent<Package>().DoneReadingLetter();
			//#todo save letter for later
			Destroy(gameObject);
		}
    }
}
