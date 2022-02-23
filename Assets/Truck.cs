using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Truck : Shot
{
	float _offsetTimer;
	Vector3 _localPos;
	public float _offsetFrequency;
	public float _offsetAmp;
	public Vector3 _offset;

	protected override void Awake(){
		base.Awake();
		enabled=false;
	}

    // Start is called before the first frame update
    protected override void Start()
    {
		base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
		base.Update();
		_offsetTimer+=Time.deltaTime;
		transform.localPosition=_localPos;
		transform.position+=transform.right*Mathf.Sin(_offsetTimer*Mathf.PI*2*_offsetFrequency)*_offsetAmp;
    }

	public override void StartTracking(Transform t){
		base.StartTracking(t);
		transform.SetParent(t);
		//transform.localPosition=_offset;
		transform.position=_target.position+transform.forward*_offset.z+transform.up*_offset.y+transform.right*_offset.x;
		_localPos=transform.localPosition;
		enabled=true;
	}
}
