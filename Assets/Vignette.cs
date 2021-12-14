using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Vignette : MonoBehaviour
{
	Material _mat;
	float _max=10.73f;
	float _min;
	public float _openUpTime;

	void Awake(){
		_mat=GetComponent<RawImage>().material;
		_mat.SetFloat("_Amount",0);
	}

    // Start is called before the first frame update
    void Start()
    {

    }

	public void SetMax(){
		_mat.SetFloat("_Amount",_max);
	}
	public void SetMin(){
		_mat.SetFloat("_Amount",_min);
	}

	public void OpenUp(){
		StartCoroutine(OpenUpR());
	}

	IEnumerator OpenUpR(){
		float timer=0;
		while(timer<_openUpTime){
			timer+=Time.deltaTime;
			_mat.SetFloat("_Amount",Mathf.Lerp(_max,_min,timer/_openUpTime));
			yield return null;
		}
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
