using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Secret : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	void OnTriggerEnter(Collider other){
		Debug.Log("Player entered "+other.transform.name);
		GetComponent<MeshRenderer>().enabled=false;
	}

	void OnTriggerExit(Collider other){
		Debug.Log("Player exited "+other.transform.name);
		GetComponent<MeshRenderer>().enabled=true;
	}
}
