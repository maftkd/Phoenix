using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonPrompts : MonoBehaviour
{
	Transform _low;
	Transform _mid;
	Transform _high;
	bool _active;

	Transform _lowTarget;
	Transform _midTarget;
	Transform _highTarget;
	Camera _cam;

	Material _lowMat;
	Material _midMat;
	Material _highMat;

	MInput _mIn;

	void Awake(){
		_low=transform.Find("Low");
		_mid=transform.Find("Mid");
		_high=transform.Find("High");
		SetActive(false);

		_lowMat=_low.GetComponent<RawImage>().material;
		_midMat=_mid.GetComponent<RawImage>().material;
		_highMat=_high.GetComponent<RawImage>().material;

		Sing sing = GameManager._player.transform.GetComponentInChildren<Sing>();
		_lowTarget=sing.transform.Find("Low");
		_midTarget=sing.transform.Find("Mid");
		_highTarget=sing.transform.Find("High");
		_cam=GameManager._mCam.GetComponent<Camera>();
		_mIn=GameManager._mIn;
	}
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if(_active){
			_low.position=_cam.WorldToScreenPoint(_lowTarget.position);
			_mid.position=_cam.WorldToScreenPoint(_midTarget.position);
			_high.position=_cam.WorldToScreenPoint(_highTarget.position);
			_lowMat.SetFloat("_Highlight",_mIn.GetLow()?1:0);
			_midMat.SetFloat("_Highlight",_mIn.GetMid()?1:0);
			_highMat.SetFloat("_Highlight",_mIn.GetHigh()?1:0);
		}
    }

	public void SetActive(bool active){
		_low.gameObject.SetActive(active);
		_mid.gameObject.SetActive(active);
		_high.gameObject.SetActive(active);
		_active=active;
	}
}
