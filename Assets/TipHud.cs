using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TipHud : MonoBehaviour
{

	public static TipHud _instance;
	CanvasGroup _cg;
	Text _text;
	Transform _target;
	Vector3 _offset;
	Camera _cam;

	void Awake(){
		_instance=this;
		_cg=GetComponent<CanvasGroup>();
		_cg.alpha=0f;
		_text=transform.GetComponentInChildren<Text>();
		_cam=GameManager._mCam.GetComponent<Camera>();
	}
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		/*
		if(_cg.alpha==1f&&_target!=null){
			Vector3 screenPoint=_cam.WorldToScreenPoint(_target.position+_offset);
			screenPoint.z=0;
			transform.position=screenPoint;
		}
		*/
    }

	public static void ShowTip(string s,Transform target, Vector3 offset){
		_instance.ShowTipA(s,target,offset);
	}

	public static void ClearTip(){
		_instance.ClearTipA();
	}

	public void ShowTipA(string s,Transform target, Vector3 offset){
		_text.text=s;
		_cg.alpha=1f;
		_target=target;
		_offset=offset;
	}

	public void ClearTipA(){
		_cg.alpha=0f;
	}
}
