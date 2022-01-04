using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleBox : MonoBehaviour
{
	Transform _effects;

	protected virtual void Awake(){
		Debug.Log("Starting puzzle box");
		_effects=transform.Find("Effects");
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
		if(_effects!=null)
			_effects.gameObject.SetActive(true);
		StartCoroutine(OpenBox());
	}

	protected virtual IEnumerator OpenBox(){
		yield return null;
	}
}
