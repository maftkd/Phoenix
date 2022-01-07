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
	float _revealDur=1f;
	MCamera _mCam;
	float _resetCamDelay=1f;
	public UnityEvent _onRevealed;

	protected virtual void Awake(){
		Debug.Log("Starting puzzle box");
		_effects=transform.Find("Effects");
		_surroundCam=GetComponent<SurroundCamHelper>();
		_guideLine=transform.Find("GuideLine").gameObject;
		_mCam=Camera.main.GetComponent<MCamera>();
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

	public virtual void Reveal(){
		if(transform.parent!=null){
			StartCoroutine(RevealR());
		}
	}

	public virtual IEnumerator RevealR(){
		Transform carrier=transform.parent;
		Transform carrierMesh=carrier.GetChild(0);
		//clear parent
		transform.SetParent(null);
		carrierMesh.SetParent(null);
		float timer=0;
		Vector3 scale=Vector3.one;
		while(timer<_revealDur){
			timer+=Time.deltaTime;
			scale.y=1f-timer/_revealDur;
			carrierMesh.localScale=scale;
			yield return null;
		}
		Destroy(carrier.gameObject);
		Destroy(carrierMesh.gameObject);
		yield return new WaitForSeconds(_resetCamDelay);
		_mCam.DefaultCam();
		_onRevealed.Invoke();
	}

	void OnDrawGizmos(){
	}
}
