using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MOutline : MonoBehaviour
{

	public Vector3 _scale;
	public Material _outlineMat;

	void Awake(){
		GameObject outline = new GameObject("Outline");
		Transform outT = outline.transform;
		outT.SetParent(transform);
		outT.localPosition=Vector3.zero;
		outT.localEulerAngles=Vector3.zero;
		outT.localScale=_scale;
		MeshFilter meshF=outline.AddComponent<MeshFilter>();
		meshF.sharedMesh=GetComponent<MeshFilter>().sharedMesh;
		MeshRenderer meshR=outline.AddComponent<MeshRenderer>();
		meshR.material=_outlineMat;
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
