using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dialog : MonoBehaviour
{
	GameObject _canvas;

	void Awake(){
		_canvas=transform.Find("Dialog").gameObject;
		//_canvas=transform.GetComponentInChildren<Canvas>().gameObject;
		_canvas.SetActive(false);
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void Activate(bool active){
		_canvas.SetActive(active);
	}
}
