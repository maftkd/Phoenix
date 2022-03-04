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
	protected MInput _mIn;
	protected Bird _player;

	//awakey
	protected virtual void Awake(){
		_originalParent=transform.parent;
		if(!_alwaysEnabled)
			enabled=false;
		_cam=GetComponent<Camera>();
		_mIn=GameManager._mIn;
		_player=GameManager._player;
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

	protected bool HandleMouseMotion(){
		//we should rotate on mouse motion
		Vector2 mouseMotion = _mIn.GetMouseMotion();
		if(mouseMotion.x!=0){
			Vector3 diff=transform.position-_player.transform.position;
			float r = diff.magnitude;
			float phi=Mathf.Asin(diff.y/diff.magnitude);

			//calc theta
			Vector3 camBack=-transform.forward;
			camBack.y=0;
			camBack.Normalize();
			float theta=Mathf.Atan2(camBack.z,camBack.x);

			//modify theta
			theta-=mouseMotion.x;

			//update position
			float y = Mathf.Sin(phi);
			float xzRad = Mathf.Cos(phi);
			float x = xzRad*Mathf.Cos(theta);
			float z = xzRad*Mathf.Sin(theta);
			Vector3 offset = new Vector3(x,y,z)*r;
			
			Vector3 targetPos=_player.transform.position+offset;

			transform.position=targetPos;

			float pitch=transform.eulerAngles.x;
			transform.forward=-offset;
			Vector3 eulers=transform.eulerAngles;
			eulers.x=pitch;
			transform.eulerAngles=eulers;
			return true;
		}

		return false;
	}
}
