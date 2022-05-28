using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TipHud : MonoBehaviour
{

	public static TipHud _instance;
	CanvasGroup _cg;
	Text _title;
	Text _detail;
	Text _prompt;
	Transform _target;
	Vector3 _offset;
	Camera _cam;
	Animator _anim;
	public float _tipDur;
	float _tipTimer;
	Image _timerBar;
	MInput _mIn;

	void Awake(){
		_instance=this;
		_title=transform.Find("Title").GetComponent<Text>();
		_detail=transform.Find("Detail").GetComponent<Text>();
		_prompt=transform.Find("Prompt").GetComponent<Text>();
		_cam=GameManager._mCam.GetComponent<Camera>();
		_mIn=GameManager._mIn;
		_anim=GetComponent<Animator>();
		_timerBar=transform.Find("Timer").GetComponent<Image>();
	}
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if(_tipTimer>0){
			_tipTimer-=Time.deltaTime;
			float frac=_tipTimer/_tipDur;
			_timerBar.fillAmount=frac;
			if(_tipTimer<=0f||_mIn.GetInteractDown()){
				ClearTipA();
			}
		}
    }

	public static void ShowTip(string t, string d, string p){
		_instance.ShowTipA(t,d,p);
	}

	public static void ClearTip(){
		_instance.ClearTipA();
	}

	public void ShowTipA(string t, string d, string p){
		_title.text=t;
		_detail.text=d;
		_prompt.text=p;
		_anim.SetBool("show",true);
		_tipTimer=_tipDur+0.33f;
	}

	public void ClearTipA(){
		_anim.SetBool("show",false);
	}
}
