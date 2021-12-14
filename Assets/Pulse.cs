using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pulse : MonoBehaviour
{
	public float _period;
	public AnimationCurve _curve;
	float _timer;
	CanvasGroup _cg;
    // Start is called before the first frame update
    void Start()
    {
		_cg = GetComponent<CanvasGroup>();
    }

    // Update is called once per frame
    void Update()
    {
		_timer+=Time.deltaTime;
		if(_timer>_period)
			_timer=0;
		_cg.alpha=_curve.Evaluate(_timer/_period);
    }
}
