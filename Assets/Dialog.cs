using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dialog : MonoBehaviour
{
	Text _text;

	void Awake(){
		_text=transform.GetChild(0).GetComponent<Text>();
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void ShowText(string s){
		gameObject.SetActive(true);
		_text.text=s;
	}

	public void Hide(){
		gameObject.SetActive(false);
	}
}
