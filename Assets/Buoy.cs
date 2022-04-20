using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buoy : MonoBehaviour
{
	public float _spinSpeed;
	float _wobbleTimer;
	public float _wobblePeriod;
	public float _wobbleAmplitude;

    // Start is called before the first frame update
    void Start()
    {
		_wobbleTimer=Random.value*_wobblePeriod;
    }

    // Update is called once per frame
    void Update()
    {
		transform.Rotate(Vector3.up*Time.deltaTime*_spinSpeed,Space.World);
		//Vector3 eulerAngles=transform.eulerAngles;
		_wobbleTimer+=Time.deltaTime;
		if(_wobbleTimer>_wobblePeriod)
			_wobbleTimer=0;
		transform.Rotate(Vector3.right*Time.deltaTime*Mathf.Sin(_wobbleTimer/_wobblePeriod*Mathf.PI*2f)*_wobbleAmplitude);
		//eulerAngles.z=-90f+Mathf.Sin(_wobbleTimer/_wobblePeriod*Mathf.PI*2f)*_wobbleAmplitude;
		//transform.eulerAngles=eulerAngles;
    }
}
