using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PuzzleBox : MonoBehaviour
{
	Transform _effects;
	public UnityEvent _onSolved;
	SurroundCamHelper _surroundCam;
	GameObject _guideLine;

	protected virtual void Awake(){
		Debug.Log("Starting puzzle box");
		_effects=transform.Find("Effects");
		_surroundCam=GetComponent<SurroundCamHelper>();
		_guideLine=transform.Find("GuideLine").gameObject;
	}

	protected virtual void OnEnable(){

	}

	protected virtual void OnDisable(){

	}

    // Start is called before the first frame update
    protected virtual void Start()
	{
        
    }

    // Update is called once per frame
    protected virtual void Update()
    {
    }

	public virtual void PuzzleSolved(){
		Debug.Log("Puzzle Solved");
		_onSolved.Invoke();
		_surroundCam.enabled=false;
		_guideLine.SetActive(false);
		if(_effects!=null)
			_effects.gameObject.SetActive(true);
		StartCoroutine(OpenBox());
	}

	protected virtual IEnumerator OpenBox(){
		yield return null;
	}

	void OnDrawGizmos(){
	}
}
