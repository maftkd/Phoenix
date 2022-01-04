using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBCamera : PuzzleBox
{
	public float _liftDelay;
	Transform _box;
	public float _liftDur;
	public AnimationCurve _liftCurve;
	public AudioSource _gearsAudio;
	protected override void Awake(){
		base.Awake();
		//your code here
		_box=transform.GetChild(0);
	}

	protected override void OnEnable(){
		base.OnEnable();
		//your code here
	}

	protected override void OnDisable(){
		base.OnDisable();
		//your code here
	}

    // Start is called before the first frame update
    protected override void Start()
    {
		base.Start();
		//your code here
    }

    // Update is called once per frame
    protected override void Update()
    {
		base.Update();
		//your code here
    }

	public override void PuzzleSolved(){
		base.PuzzleSolved();
		//your code here
	}

	protected override IEnumerator OpenBox(){
		yield return new WaitForSeconds(_liftDelay);
		_gearsAudio.Play();
		float timer=0;
		Vector3 startPos=_box.position;
		Vector3 endPos=startPos+Vector3.up*_box.localScale.y*0.5f;
		while(timer<_liftDur){
			timer+=Time.deltaTime;
			_box.position=Vector3.Lerp(startPos,endPos,_liftCurve.Evaluate(timer/_liftDur));
			yield return null;
		}
		_box.position=endPos;
	}
}
