using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stork : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void DeliverBox(PuzzleBox box){
		Transform t = box.transform;
		Debug.Log("Delivering box to "+t.position);
	}
}
