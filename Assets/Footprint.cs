using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footprint : MonoBehaviour
{
	public float _startLifetime;
	float _lifetime;
	Material _mat;
    // Start is called before the first frame update
    void Start()
    {
		_mat=GetComponent<MeshRenderer>().material;
		_lifetime=_startLifetime;
    }

    // Update is called once per frame
    void Update()
    {
		_lifetime-=Time.deltaTime;
		_mat.SetFloat("_Alpha",_lifetime/_startLifetime);
		if(_lifetime<=0)
			Destroy(gameObject);
    }
}
