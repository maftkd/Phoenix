using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human : MonoBehaviour
{
	[HideInInspector]
	public Transform _temptress;
	Transform _cameraParent;

    // Start is called before the first frame update
    void Start()
    {
		_temptress=transform.parent.GetComponentInChildren<Temptress>().transform;
		_cameraParent=transform.GetChild(0);
    }

    // Update is called once per frame
    void Update()
    {
		if(_temptress!=null){
			Vector3 toTemptress=_temptress.position-_cameraParent.position;
			//_cameraParent.right=toTemptress;
			float ang = Mathf.Atan2(toTemptress.y,toTemptress.x);
			_cameraParent.eulerAngles=Vector3.forward*ang*Mathf.Rad2Deg;
		}
    }

}
