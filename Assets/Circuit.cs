using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Circuit : MonoBehaviour
{
	Material _mat;

	public delegate void CircuitEvent();
	public event CircuitEvent _powerChange;

	bool _powered;
	public UnityEvent _onPowered;
	public UnityEvent _onPoweredOff;

	public string _fillVar;
	float _length;
	float _fillRate=0.5f;
	float _fallRate=2f;
	public Circuit _next;
	float _fill;
	[HideInInspector]
	public Circuit _prev;

	void Awake(){
		_mat=GetComponent<Renderer>().material;
		if(GetComponent<Cable>()!=null)
		{
			_length=GetComponent<Cable>().GetLength();
		}
		else if(GetComponent<LineRenderer>()!=null)
		{
			_length=GetLengthFromLineRenderer();
		}
		if(_next!=null)
			_next._prev=this;
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void Power(bool on){
		_powered=on;
		//_mat.SetFloat("_Lerp", on? 1f : 0f);
		/*
		if(_powerChange!=null)
		{
			_powerChange.Invoke();
		}
		if(_powered)
			_onPowered.Invoke();
		else
			_onPoweredOff.Invoke();
			*/
		StopAllCoroutines();
		if(_powered){
			StartCoroutine(PowerR());
		}
		else{
			StartCoroutine(PowerDownR());

		}
	}

	IEnumerator PowerR(){
		float timer=0;
		float rate = _fillRate;
		float length = _length;
		float fullDur =length/rate;
		length=(1-_fill)*length;
		float dur=length/rate;
		timer=fullDur-dur;
		while(timer<fullDur){
			timer+=Time.deltaTime;
			_fill=timer/dur;
			_mat.SetFloat(_fillVar,_fill);
			yield return null;
		}
		_fill=1f;
		_mat.SetFloat(_fillVar,_fill);
		//continue power
		if(_next!=null){
			_next.Power(true);
		}
		if(_powerChange!=null)
		{
			_powerChange.Invoke();
		}
	}

	IEnumerator PowerDownR(){
		float timer=0;
		float rate = _fallRate;
		float length = _length;
		float fullDur = length/rate;
		length*=_fill;
		float dur = length/rate;
		timer=dur;
		if(_powerChange!=null)
		{
			_powerChange.Invoke();
		}
		while(timer>0){
			timer-=Time.deltaTime;
			_fill=timer/fullDur;
			_mat.SetFloat(_fillVar,_fill);
			yield return null;
		}
		_fill=0;
		_mat.SetFloat(_fillVar,_fill);
		if(_prev!=null){
			_prev.Power(false);
		}
	}

	public bool Powered(){
		return _fill>=1f;
	}

	float GetLengthFromLineRenderer(){
		LineRenderer lr = GetComponent<LineRenderer>();
		Vector3 [] points = new Vector3[lr.positionCount];
		lr.GetPositions(points);
		float dist=0;
		Vector3 prevPos=Vector3.zero;
		bool first=true;
		foreach(Vector3 p in points){
			if(first)
			{
				prevPos=p;
				first=false;
			}
			else
				dist+=(p-prevPos).magnitude;
			prevPos=p;
		}
		return dist;
	}

	public Circuit GetLastInChainWithPower(){
		Circuit next = this;
		while(next._next!=null&&next._next._fill>0){
			next=next._next;
		}
		return next;
	}
}
