using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shot : MonoBehaviour
{

	public int _priority;
	public Transform _target;
	Transform _originalParent;
	public bool _alwaysEnabled;
	protected Camera _cam;

	//awakey
	protected virtual void Awake(){
		_originalParent=transform.parent;
		if(!_alwaysEnabled)
			enabled=false;
		_cam=GetComponent<Camera>();
	}

    // Start is called before the first frame update
    protected virtual void Start()
    {
        
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }

	public virtual void StartTracking(Transform t){
		_target=t;
		enabled=true;
	}

	public virtual void StopTracking(){
		//transform.SetParent(_originalParent);
		if(!_alwaysEnabled)
			enabled=false;
	}
}
