using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallBubble : MonoBehaviour
{
	public float _maxScale;
	public float _scaleSpeed;
	
	void Awake(){
		//#temp - if we have visual aids on then don't destroy this
		Destroy(gameObject);
	}

    // Start is called before the first frame update
    void Start()
    {
		transform.SetParent(transform.parent.parent);
		StartCoroutine(GrowBubble());
    }

	IEnumerator GrowBubble(){
		float endScale=_maxScale;
		while(transform.localScale.x<endScale){
			transform.localScale=(transform.localScale.x+Time.deltaTime*_scaleSpeed)*Vector3.one;
			yield return null;
		}
		Destroy(gameObject);
	}
}
