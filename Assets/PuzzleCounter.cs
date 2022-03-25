using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleCounter : MonoBehaviour
{
	public int _counter=0;
	float _width;
	Vector3 _showPos;
	Vector3 _hidePos;
	Text _cur;
	Text _next;
	public Color _textColor;
	Color _hiddenColor;
	public float _tabDur;
	IEnumerator _incRoutine;
	Transform _bar;
	Vector3 _barStartLocal;

	[Header("Animation params")]
	public float _slideInDur;
	public float _incDelay;
	public float _incDur;
	public float _fastIncDur;
	public float _hideDelay;
	public float _barDur;

	void Awake(){
		_showPos=transform.position;
		_hidePos=_showPos+Vector3.right*GetComponent<RectTransform>().sizeDelta.x;
		transform.position=_hidePos;
		Transform bg=transform.GetChild(0);
		_cur=bg.GetChild(0).GetComponent<Text>();
		_next=bg.GetChild(1).GetComponent<Text>();
		_bar=bg.GetChild(2);
		_barStartLocal=_bar.localPosition;
		_hiddenColor=_textColor;
		_hiddenColor.a=0;
		SetCount();
	}

	void SetCount(){
		_cur.text=_counter.ToString("0");
		_cur.color=_textColor;
		_next.color=_hiddenColor;
	}

	public void Increment(int count){
		if(_incRoutine==null){
			_incRoutine=IncrementR();
			StartCoroutine(_incRoutine);
		}
		else{
			StopCoroutine(_incRoutine);
			_incRoutine=FastIncrementR(count);
			StartCoroutine(_incRoutine);
		}
	}

	IEnumerator IncrementR(){
		//slide in
		float timer=0;
		float dur=_slideInDur;
		while(timer<dur){
			timer+=Time.deltaTime;
			float frac=timer/dur;
			transform.position=Vector3.Lerp(_hidePos,_showPos,frac);
			yield return null;
		}
		transform.position=_showPos;

		//wait
		yield return new WaitForSeconds (_incDelay);

		//inc
		_counter++;
		_next.text=_counter.ToString("0");
		timer=0;
		dur=_incDur;
		Transform bottom=_next.transform;
		Transform top = _cur.transform;
		Vector3 bottomPos=bottom.position;
		Vector3 topPos=top.position;
		float yDiff=topPos.y-bottomPos.y;
		Vector3 tippyTopPos=topPos+Vector3.up*yDiff;
		while(timer<dur){
			timer+=Time.deltaTime;
			float frac=timer/dur;
			bottom.position=Vector3.Lerp(bottomPos,topPos,frac);
			top.position=Vector3.Lerp(topPos,tippyTopPos,frac);
			_cur.color=Color.Lerp(_textColor,_hiddenColor,frac);
			_next.color=Color.Lerp(_hiddenColor,_textColor,frac);
			yield return null;
		}
		_cur.text=_counter.ToString("0");
		top.position=topPos;
		bottom.position=bottomPos;
		_cur.color=_textColor;
		_next.color=_hiddenColor;

		StartCoroutine(AnimateBar());

		//wait
		yield return new WaitForSeconds (_hideDelay);

		//slide out
		timer=0;
		dur=_slideInDur;
		while(timer<dur){
			timer+=Time.deltaTime;
			float frac=timer/dur;
			transform.position=Vector3.Lerp(_showPos,_hidePos,frac);
			yield return null;
		}
		transform.position=_hidePos;
		_incRoutine=null;
	}

	IEnumerator FastIncrementR(int count){
		//slide in
		float timer=0;
		float dur=_slideInDur;
		while(timer<dur){
			timer+=Time.deltaTime;
			float frac=timer/dur;
			transform.position=Vector3.Lerp(_hidePos,_showPos,frac);
			yield return null;
		}
		transform.position=_showPos;

		//wait
		yield return new WaitForSeconds (_incDelay);

		//inc
		int startCount=_counter;
		int targetCount=count;
		for(int i=startCount;i<targetCount;i++){
			_cur.text=_counter.ToString("0");
			_counter++;
			_next.text=_counter.ToString("0");
			timer=0;
			dur=_fastIncDur;
			Transform bottom=_next.transform;
			Transform top = _cur.transform;
			Vector3 bottomPos=bottom.position;
			Vector3 topPos=top.position;
			float yDiff=topPos.y-bottomPos.y;
			Vector3 tippyTopPos=topPos+Vector3.up*yDiff;
			while(timer<dur){
				timer+=Time.deltaTime;
				float frac=timer/dur;
				bottom.position=Vector3.Lerp(bottomPos,topPos,frac);
				top.position=Vector3.Lerp(topPos,tippyTopPos,frac);
				_cur.color=Color.Lerp(_textColor,_hiddenColor,frac);
				_next.color=Color.Lerp(_hiddenColor,_textColor,frac);
				yield return null;
			}
			_cur.text=_counter.ToString("0");
			top.position=topPos;
			bottom.position=bottomPos;
			_cur.color=_textColor;
			_next.color=_hiddenColor;
		}

		StartCoroutine(AnimateBar());

		//wait
		yield return new WaitForSeconds (_hideDelay);

		//slide out
		timer=0;
		dur=_slideInDur;
		while(timer<dur){
			timer+=Time.deltaTime;
			float frac=timer/dur;
			transform.position=Vector3.Lerp(_showPos,_hidePos,frac);
			yield return null;
		}
		transform.position=_hidePos;
		_incRoutine=null;
	}

	public void Show(bool show=true){
		if(_incRoutine!=null)
			return;
		if(show)
			StartCoroutine(ShowR());
		else
			StartCoroutine(HideR());
	}

	IEnumerator ShowR(){
		//slide in
		float timer=0;
		float dur=_slideInDur;
		while(timer<dur){
			timer+=Time.deltaTime;
			float frac=timer/dur;
			transform.position=Vector3.Lerp(_hidePos,_showPos,frac);
			yield return null;
		}
		transform.position=_showPos;

		StartCoroutine(AnimateBar());
	}

	IEnumerator HideR(){
		float timer=0;
		float dur=_slideInDur;
		while(timer<dur){
			timer+=Time.deltaTime;
			float frac=timer/dur;
			transform.position=Vector3.Lerp(_showPos,_hidePos,frac);
			yield return null;
		}
		transform.position=_hidePos;
	}

	IEnumerator AnimateBar(){
		float timer=0;
		float dur=_barDur;
		_bar.gameObject.SetActive(true);
		Vector3 barStart=_barStartLocal;
		Vector3 barEnd=barStart+Vector3.right*50f;
		while(timer<dur){
			timer+=Time.deltaTime;
			float frac=timer/dur;
			_bar.localPosition=Vector3.Lerp(barStart,barEnd,frac);
			yield return null;
		}
		_bar.localPosition=barStart;
		_bar.gameObject.SetActive(false);
	}

	public void Reset(){
		_counter=0;
	}
}
