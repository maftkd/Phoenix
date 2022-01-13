using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroySound : MonoBehaviour
{
	AudioSource _a;

	void Awake(){
		_a=GetComponent<AudioSource>();
	}

    // Update is called once per frame
    void Update()
    {
		if(!_a.isPlaying)
			Destroy(transform.gameObject);
    }
}
