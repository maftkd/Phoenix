using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public static GameManager _instance;
	public int _levelIndex;

	void Awake(){
		_instance = this;
		SetupLevel();
	}
    // Start is called before the first frame update
    void Start()
    {

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
}
