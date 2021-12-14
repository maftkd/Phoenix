using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

	public void Activate(Hop h){
		Transform player = h.transform;
		GetComponent<MeshRenderer>().enabled=false;
		h.enabled=false;
		player.GetComponent<Fall>().enabled=true;
		GetComponent<AudioSource>().Play();
		Fall[] _rubble = transform.GetComponentsInChildren<Fall>();
		foreach(Fall f in _rubble)
			f.enabled=true;
	}
}
