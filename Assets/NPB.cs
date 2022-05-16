using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPB : MonoBehaviour
{
	bool _targeted;
	GameObject _target;
	Sing _sing;
	public float _tempSingPeriod;

	void Awake(){
		_target=transform.Find("Target").gameObject;
		_sing=transform.GetComponentInChildren<Sing>();

	}
    // Start is called before the first frame update
    void Start()
    {
		StartCoroutine(TempSing());
    }

	IEnumerator TempSing(){
		yield return new WaitForSeconds(_tempSingPeriod);
		_sing.SingSong();
		StartCoroutine(TempSing());
	}

    // Update is called once per frame
    void Update()
    {
		if(_targeted&&!_target.activeSelf)
			_target.SetActive(true);
		else if(!_targeted&&_target.activeSelf)
			_target.SetActive(false);
    }
        
	void LateUpdate(){
		_targeted=false;
	}

	public void Targeted(){
		_targeted=true;
	}
}
