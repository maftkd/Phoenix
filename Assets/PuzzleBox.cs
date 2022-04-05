using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PuzzleBox : MonoBehaviour
{
	[HideInInspector]
	public string _puzzleId;
	Material _rimMat;

	public AudioClip _enterSound;
	public AudioClip _exitSound;

	bool _solved;
	public UnityEvent _onSolve;

	public AudioClip _rewardSound;
	public float _pulseDelay;
	ScienceCamera _cam;
	public PuzzleBox _nextPuzzle;

	protected virtual void Awake(){
		Island i = transform.GetComponentInParent<Island>();
		if(i!=null){
			int islandIndex = i.transform.GetSiblingIndex();
			int puzzleIndex = transform.GetSiblingIndex();
			_puzzleId=(islandIndex+1)+"."+(puzzleIndex+1);
		}
		_rimMat=transform.GetChild(0).GetComponent<MeshRenderer>().material;
		_cam=transform.GetComponentInChildren<ScienceCamera>();
	}

	public void BirdEnter(){
		_cam.Activate(true);
		if(_solved)
			return;
	}

	public void BirdExit(){
		_cam.Activate(false);
		if(_solved)
			return;
	}

	public void PuzzleSolved(){
		if(_solved)
			return;
		_solved=true;
		_onSolve.Invoke();
		Sfx.PlayOneShot3D(_rewardSound,transform.position);
		StartCoroutine(PulseRim());
		//_cam.Activate(false);
	}

	IEnumerator PulseRim(){
		for(int i=0;i<5;i++){
			_rimMat.SetColor("_EmissionColor",Color.black);
			yield return new WaitForSeconds(_pulseDelay);
			_rimMat.SetColor("_EmissionColor",Color.white);
			yield return new WaitForSeconds(_pulseDelay);
		}
	}

	void OnDrawGizmos(){
	}
}
