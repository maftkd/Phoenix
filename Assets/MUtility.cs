using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MUtility : MonoBehaviour
{
    public static Transform FindRecursive(Transform cur, string name){
		Transform t=null;
		foreach(Transform child in cur){
			if(child.name==name)
			{
				//match found
				return child;
			}
			else
			{
				//recursive search
				t=FindRecursive(child,name);
				if(t!=null)
					return t;
			}
		}
		return null;
	}
}
