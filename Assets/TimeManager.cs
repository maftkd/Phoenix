using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
	[Tooltip("0 is midWinter on year N, 1 is midWinter on year N+1")]
	[Range(0,1)]
	public float _timeOfYear;
	[Tooltip("0 is midNight on day N, 1 is midNight on day N+1")]
	[Range(0,1)]
	public float _timeOfDay;
	Transform _sun;

	[Header("Real Time")]
	public float _secsPerDay;
	public int _daysPerYear;
	int _day;
    // Start is called before the first frame update
    void Start()
    {
		_sun = transform.GetComponentInChildren<Light>().transform;
		_day=0;
    }

	void SetSunPos(){
		float theta = Mathf.PI*2*_timeOfDay-Mathf.PI*0.5f;
		float phi = -Mathf.PI*0.25f*(1-_timeOfYear);
		float z = Mathf.Sin(phi);
		float r = Mathf.Cos(phi);
		Vector3 pos = new Vector3(r*Mathf.Cos(theta),r*Mathf.Sin(theta),z);
		_sun.position=pos;
		_sun.LookAt(Vector3.zero);
		RenderSettings.ambientIntensity=(0.5f-Mathf.Abs(_timeOfDay-0.5f))*2;
	}

    // Update is called once per frame
    void Update()
    {
		_timeOfDay+=Time.deltaTime/_secsPerDay;
		if(_timeOfDay>1f){
			_timeOfDay-=1f;
			_day++;
			if(_day>=_daysPerYear)
				_day=0;
			_timeOfYear=_day/(float)_daysPerYear;
		}
		SetSunPos();
    }
}
