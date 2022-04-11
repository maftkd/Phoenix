using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour
{
	public static DebugScreen _instance;
	public int _numSlots;
	Text [] _slots;
	Vector3 [] _pSlots;
	Color [] _pColors;
	int _curSlot;

	void Awake(){
		_instance=this;

		//instance the text slots
		_slots = new Text[_numSlots];
		Transform reference=transform.GetChild(0);
		_slots[0]=reference.GetComponent<Text>();
		for(int i=1;i<_numSlots;i++){
			Transform s = Instantiate(reference,transform);
			s.localPosition+=i*reference.GetComponent<RectTransform>().sizeDelta.y*Vector3.down;
			_slots[i]=s.GetComponent<Text>();
		}

		foreach(Text t in _slots)
			t.text="";

		//setup point slots
		_pSlots = new Vector3[_numSlots];
		_pColors = new Color[_numSlots];
		for(int i=0; i<_numSlots; i++)
		{
			_pSlots[i]=Vector3.zero;
			_pColors[i]=Color.black;
		}

	}

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
		_curSlot=0;
    }

	//text debugging
	public static void Print(string s, bool b, int slot){
		_instance.PrintA(s,b,slot);
	}
	public static void Print(string s, float f, int slot){
		_instance.PrintA(s,f,slot);
	}
	public static void Print(string s, int slot){
		_instance.PrintA(s,slot);
	}
	public static void Print(string s){
		_instance.PrintA(s);
	}

	public void PrintA(string s, bool b, int slot){
		_slots[slot].text=s+b.ToString();
	}
	public void PrintA(string s, float f, int slot){
		_slots[slot].text=s+f.ToString("0.000");
	}
	public void PrintA(string s, int slot){
		if(_slots[slot]!=null)
			_slots[slot].text=s;
	}
	public void PrintA(string s){
		if(_slots[_curSlot]!=null)
		{
			_slots[_curSlot].text=s;
			_curSlot++;
		}
	}

	//point debugging
	public static void DebugPoint(Vector3 v, Color c, int slot){
		_instance.DebugPointA(v,c,slot);
	}
	public void DebugPointA(Vector3 v, Color c, int slot){
		_pSlots[slot]=v;
		_pColors[slot]=c;
	}

	void OnDrawGizmos(){
		if(_pColors==null||_pColors.Length!=_numSlots)
			return;
		for(int i=0; i<_numSlots; i++){
			Color c = _pColors[i];
			if(c!=Color.white){
				Gizmos.color=c;
				Gizmos.DrawWireSphere(_pSlots[i],0.1f);
			}
		}
	}
}
