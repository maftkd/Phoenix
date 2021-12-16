using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GameManager : MonoBehaviour
{
	public static GameManager _instance;
	public int _levelIndex;
	public Transform _mainCam;
	public static string _jumpButton;
	public static string _perchButton;
	public static string _horizontalAxis;
	public static string _verticalAxis;
	public bool _altControls;

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
		_jumpButton=_altControls?"Perch":"Jump";
		_perchButton=_altControls?"Jump":"Perch";
#if UNITY_EDITOR_OSX
		_horizontalAxis=_altControls?"HorizontalAlt":"Horizontal";
#else
		_horizontalAxis="Horizontal";
#endif
		_verticalAxis=_altControls?"VerticalAlt":"Vertical";
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
