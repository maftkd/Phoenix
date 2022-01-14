using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MRandom : MonoBehaviour
{
	public static float RandSign(){
		if(Random.value>=0.5f)
			return 1f;
		else
			return -1f;
	}
}
