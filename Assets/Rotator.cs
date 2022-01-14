using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
	public Vector3 _rotationPerSec;

	void Awake(){
		transform.Rotate(_rotationPerSec.normalized*Random.value*360f);
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		transform.Rotate(_rotationPerSec*Time.deltaTime);
    }
}
