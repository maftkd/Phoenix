using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour
{
	public static DebugScreen _instance;
	public int _numSlots;
	Text [] _slots;

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
	}

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public static void Print(bool b, int slot){
		_instance.PrintA(b,slot);
	}
	public static void Print(float f, int slot){
		_instance.PrintA(f,slot);
	}

	public void PrintA(bool b, int slot){
		_slots[slot].text=b.ToString();
	}
	public void PrintA(float f, int slot){
		_slots[slot].text=f.ToString("0.000");
	}
}
