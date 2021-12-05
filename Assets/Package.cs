using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Package : MonoBehaviour
{
	public GameObject _letter;
	public GameObject _camera;

    // Start is called before the first frame update
    void Start()
    {
		_letter.SetActive(false);
		_camera.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	void OnMouseEnter(){
		Crosshair._instance.SetOverItem(name);
	}

	void OnMouseExit(){
		Crosshair._instance.ClearOverItem(name);

	}

	void OnMouseDown(){
		Debug.Log("touch down");
		//time to show letter and camera somehow
		_letter.SetActive(true);
		GetComponent<MeshRenderer>().enabled=false;
		GetComponent<BoxCollider>().enabled=false;
	}

	public void DoneReadingLetter(){
		_letter.SetActive(false);
		_camera.SetActive(true);
	}
}
