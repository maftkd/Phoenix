using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transformation : MonoBehaviour
{
	Material _mat;
	public float _dur;
	float _timer;
    // Start is called before the first frame update
    void Start()
    {
		_mat=GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
		_timer+=Time.deltaTime;
		if(_timer<_dur){
			//Color c = new Color(_timer/_dur,0,0,1);
			//_mat.SetColor("_Color",c);
			transform.localScale = new Vector3(1f,1-_timer/_dur,1f);
		}
    }
}
