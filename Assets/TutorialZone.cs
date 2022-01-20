using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialZone : MonoBehaviour
{
	Bird _bird;
	public bool _hideOnButtonDown;
	public string _buttonName;

	void Awake(){
		_bird=GameObject.FindGameObjectWithTag("Player").GetComponent<Bird>();
		GetComponent<MeshRenderer>().enabled=false;
	}

	public void ShowTutorial(int index){
		_bird.ShowTutorial(index);
	}
	public void HideTutorial(){
		_bird.ShowTutorial(-1);
	}

	void Update(){
		if(_hideOnButtonDown&&Input.GetButtonDown(_buttonName))
			HideTutorial();
	}
}
