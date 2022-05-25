using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideTreesInEditor : MonoBehaviour
{
	public bool _hideTrees;
	public bool _showTrees;

	void OnValidate(){
		if(_hideTrees){
			_hideTrees=false;
			ToggleTrees(false);
		}
		else if(_showTrees){
			_showTrees=false;
			ToggleTrees(true);
		}
	}

	void Awake(){
		ToggleTrees(true);
	}

	void ToggleTrees(bool active){
		foreach(Transform t in transform)
			t.gameObject.SetActive(active);
	}
}
