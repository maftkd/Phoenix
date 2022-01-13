using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceField : MonoBehaviour
{
	public AudioClip _powerOff;

	void OnDisable(){
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void Deactivate(){
		Sfx.PlayOneShot3D(_powerOff,transform.position);
		GetComponent<MeshRenderer>().enabled=false;
		GetComponent<SphereCollider>().enabled=false;
	}

	public void Activate(){
		GetComponent<MeshRenderer>().enabled=true;
		GetComponent<SphereCollider>().enabled=true;
	}
}
