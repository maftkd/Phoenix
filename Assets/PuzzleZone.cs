using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleZone : MonoBehaviour
{

	void OnTriggerEnter(Collider other){
		other.GetComponent<Bird>().SetZone(transform);
	}

	void OnTriggerExit(Collider other){
		other.GetComponent<Bird>().SetZone(null);
	}
}
