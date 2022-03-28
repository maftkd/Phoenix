﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialZone : MonoBehaviour
{
	Bird _bird;
	public bool _hideOnButtonDown;
	public bool _hideOnButtonHold;
	float _holdTimer;
	public string _buttonName;
	public bool _dontHideMesh;

	void Awake(){
		_bird=GameManager._player;
		if(!_dontHideMesh)
			GetComponent<MeshRenderer>().enabled=false;
	}

	public void ShowTutorial(int index){
		_bird.ShowTutorial(index);
	}
	public void HideTutorial(){
		_bird.ShowTutorial(-1);
	}

	void OnDisable(){
		//HideTutorial();
	}

	void Update(){
		if(_hideOnButtonDown&&Input.GetButtonDown(_buttonName))
			HideTutorial();
		if(_hideOnButtonHold){
			if(Input.GetButton(_buttonName))
			{
				_holdTimer+=Time.deltaTime;
				if(_holdTimer>1f)
					HideTutorial();
			}
			else
				_holdTimer=0;
		}
	}
}
