using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dialog : MonoBehaviour
{
	Text _text;
	Canvas _canvas;
    // Start is called before the first frame update
    void Start()
    {
		_canvas=GetComponent<Canvas>();
		_text = transform.GetComponentInChildren<Text>();
		_canvas.enabled=false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void ShowText(string s){
		_canvas.enabled=true;
		_text.text=s;
	}

	public void HideText(){
		_canvas.enabled=false;
	}
}
