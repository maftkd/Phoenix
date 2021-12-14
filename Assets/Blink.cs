using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Blink : MonoBehaviour
{
	float _blinkTimer;
	float _blinkDur;
	float _blinkDelay;
	public float _minBlinkDur;
	public float _maxBlinkDur;
	public float _minBlinkDelay;
	public float _maxBlinkDelay;
	float _blinkAmount;
	RectTransform _top;
	RectTransform _bottom;
	int _blinks;
	bool _unblink;

	void Awake(){
		_blinkTimer=_blinkDur;
		_top=transform.GetChild(0).GetComponent<RectTransform>();
		_bottom=transform.GetChild(1).GetComponent<RectTransform>();
		_top.anchoredPosition=Vector3.zero;
		_bottom.anchoredPosition=Vector3.zero;
		_top.GetComponent<RawImage>().enabled=true;
		_bottom.GetComponent<RawImage>().enabled=true;
	}

	void OnDisable(){
		_top.GetComponent<RawImage>().enabled=false;
		_bottom.GetComponent<RawImage>().enabled=false;
	}

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
		if(_blinkTimer<_blinkDur+_blinkDelay)
		{
			_blinkTimer+=Time.deltaTime;
			if(_blinkTimer<_blinkDur){
				float t = _blinkTimer/_blinkDur;
				if(!_unblink || t<0.5f)
				{
					_top.anchoredPosition=Vector3.up*Mathf.Sin(t*Mathf.PI)*_blinkAmount;
					_bottom.anchoredPosition=Vector3.down*Mathf.Sin(t*Mathf.PI)*_blinkAmount;
				}
			}
			else if(_blinkTimer<_blinkDur+_blinkDelay){
				//just wait
				if(_unblink)
				{
					_top.anchoredPosition=Vector3.up*_blinkAmount;
					_bottom.anchoredPosition=Vector3.down*_blinkAmount;
				}
				else{
					_top.anchoredPosition=Vector3.zero;
					_bottom.anchoredPosition=Vector3.zero;
				}
			}
			else{
				_blinks--;
				if(_blinks>0)
				{
					_blinkTimer=0;
					_blinkDur=Random.Range(_minBlinkDur,_maxBlinkDur);
					_blinkDelay=Random.Range(_minBlinkDelay,_maxBlinkDelay);
				}
			}
		}
    }

	public void DoBlinks(int numBlinks){
		_blinkTimer=0;
		_blinks=numBlinks;
		_blinkDur=Random.Range(_minBlinkDur,_maxBlinkDur);
		_blinkDelay=Random.Range(_minBlinkDelay,_maxBlinkDelay);
		_blinkAmount=_top.rect.height;
	}

	public void Unblink(){
		_blinkTimer=0;
		_blinkDur=_maxBlinkDur;
		_blinkDelay=1f;
		_blinkAmount=_top.rect.height;
		_unblink=true;
	}

	public bool DoneBlinking(){
		return _blinks<=0;
	}
}
