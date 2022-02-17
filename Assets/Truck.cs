using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Truck : MonoBehaviour
{
	float _offsetTimer;
	Vector3 _localPos;
	public float _offsetFrequency;
	public float _offsetAmp;

	void Awake(){
		enabled=false;
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		_offsetTimer+=Time.deltaTime;
		transform.localPosition=_localPos+Vector3.right*Mathf.Sin(_offsetTimer*Mathf.PI*2*_offsetFrequency)*_offsetAmp;
        
    }

	public void StartTracking(Transform t){
		transform.SetParent(t);
		_localPos=transform.localPosition;
		enabled=true;
	}
}
