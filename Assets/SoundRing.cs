using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundRing : MonoBehaviour
{
	public float _scaleTime;
	public float _maxScale;
	public float _maxLineThickness;
	float _timer;
	Material _mat;
	public AnimationCurve _scaleCurve;
    // Start is called before the first frame update
    void Start()
    {
		transform.localScale=Vector3.zero;
		_mat=GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
		_timer+=Time.deltaTime;
		transform.localScale=Mathf.Lerp(0,_maxScale,_scaleCurve.Evaluate(_timer/_scaleTime))*Vector3.one;
		_mat.SetFloat("_LineThickness",Mathf.Lerp(_maxLineThickness,0,_timer/_scaleTime));
		if(_timer>=_scaleTime)
			Destroy(gameObject);
    }
}
