using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boundary : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void SetColor(Color c){
		MeshRenderer mr = GetComponent<MeshRenderer>();
		mr.material.SetColor("_ColorShallow",c);
	}
}
