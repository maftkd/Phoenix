using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{

	RawImage _button;
	Text _buttonText;
	Text _inputText;
	Text _responseText;
	Canvas _can;
	CanvasGroup _cg;

	public float _pulseDur;
	float _pulseTimer;
	public Color [] _textColors;
	public Color [] _colors;
	public string [] _buttons;
	public string [] _inputs;
	public string [] _responses;

	Transform _mainCam;

	void Awake(){
		_button=transform.GetChild(0).GetComponent<RawImage>();
		_buttonText=_button.GetComponentInChildren<Text>();
		_inputText=transform.GetChild(1).GetComponent<Text>();
		_responseText=transform.GetChild(2).GetComponent<Text>();
		_can=GetComponent<Canvas>();
		_cg=GetComponent<CanvasGroup>();

		_can.enabled=false;
		_cg.alpha=0f;

		_mainCam=Camera.main.transform;
		enabled=false;
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		//transform.LookAt(_mainCam);
		transform.forward=_mainCam.forward;
    }

	public void ShowTutorial(int index){
		if(index<0)
		{
			HideTutorial();
			return;
		}
		_button.color=_colors[index];
		_button.transform.GetChild(0).GetComponent<RawImage>().color=_textColors[index];
		_buttonText.text=_buttons[index];
		_buttonText.color=_textColors[index];
		_inputText.text=_inputs[index];
		_inputText.color=_textColors[index];
		_responseText.text=_responses[index];
		_responseText.color=_textColors[index];
		_can.enabled=true;
		_cg.alpha=1f;
		enabled=true;
	}

	public void HideTutorial(){
		_can.enabled=false;
		_cg.alpha=0f;
		enabled=false;
	}
}
