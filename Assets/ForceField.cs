using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceField : MonoBehaviour
{
	public AudioClip _powerOff;
	Material _mat;
	public float _deactivatedHeight;

	void Awake(){
		_mat=GetComponent<MeshRenderer>().material;
	}

	void OnDisable(){
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void Deactivate(){
		if(_mat==null)
			_mat=GetComponent<MeshRenderer>().material;
		Sfx.PlayOneShot3D(_powerOff,transform.position);
		_mat.SetFloat("_VCut",_deactivatedHeight);
		_mat.SetFloat("_PhaseMult",0);
		GetComponent<SphereCollider>().enabled=false;
	}

	public void Activate(){
		if(_mat==null)
			_mat=GetComponent<MeshRenderer>().material;
		_mat.SetFloat("_VCut",1f);
		GetComponent<SphereCollider>().enabled=true;
	}
}
