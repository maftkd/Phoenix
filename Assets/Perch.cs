using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Perch : MonoBehaviour
{
	public bool _occupied;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	void OnDrawGizmos(){
		Gizmos.color = _occupied? Color.red : Color.green;
		Gizmos.DrawWireSphere(transform.position,.05f);
		Gizmos.DrawRay(transform.position,transform.forward*0.2f);
	}
}
