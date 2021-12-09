using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
	public static Crosshair _instance;
	public GameObject _label;
	Text _labelText;
	List<string> _overItems;
	bool _labelVis;
	Canvas _can;

	void OnDisable(){
		_can.enabled=false;
	}

	void OnEnable(){
		if(_can!=null)
			_can.enabled=true;
	}

    // Start is called before the first frame update
    void Start()
    {
		_instance = this;
		_labelText=_label.transform.GetChild(0).GetComponent<Text>();
		_overItems = new List<string>();
		_label.SetActive(false);
		_can = GetComponent<Canvas>();
    }

    // Update is called once per frame
    void Update()
    {
		if(_overItems.Count>0){
			if(!_labelVis){
				_labelVis=true;
				_label.SetActive(true);
				_labelText.text=_overItems[0];
			}
		}
		else{
			if(_labelVis){
				_label.SetActive(false);
				_labelVis=false;
			}
		}
    }

	public void SetOverItem(string name){
		if(Fly._instance.enabled)
			return;
		_overItems.Add(name);
	}

	public void ClearOverItem(string name){
		_overItems.Remove(name);
	}
}
