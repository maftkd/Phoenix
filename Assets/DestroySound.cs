using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroySound : MonoBehaviour
{
	AudioSource _a;
    // Start is called before the first frame update
    void Start()
    {
		_a=GetComponent<AudioSource>();
		Destroy(gameObject,_a.clip.length);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
