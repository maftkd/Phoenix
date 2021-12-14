using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GameManager : MonoBehaviour
{
	public static GameManager _instance;
	public int _levelIndex;
	public Transform _mainCam;

	void Awake(){
		_instance = this;
		SetupLevel();
	}
    // Start is called before the first frame update
    void Start()
    {
		Vector3 pos = _mainCam.position;
		pos.y=0;
		_mainCam.position=pos;
    }

    // Update is called once per frame
    void Update()
    {

    }
	
	void SetupLevel(){
		for(int i=0; i<transform.childCount; i++){
			transform.GetChild(i).gameObject.SetActive(i==_levelIndex);
		}
		Transform curLevel = transform.GetChild(_levelIndex);
		curLevel.position=Vector3.zero;
	}

	public void NextLevel(){
		_levelIndex++;
		if(_levelIndex<transform.childCount)
		{
			SetupLevel();
		}
	}

	void OnValidate(){
		if(Application.isPlaying)
			return;
		if(_mainCam!=null){
			Vector3 pos = _mainCam.position;
			pos.y=-12f*_levelIndex;
			_mainCam.position=pos;
		}
	}
}
