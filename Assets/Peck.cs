using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Peck : MonoBehaviour
{
	public static Peck _instance;
	[Header("pickup")]
	public float _vertPickupOffset;
	public float _pickupTime;
	[HideInInspector]
	public bool _doneTurning;
	public float _turnTime;
	[HideInInspector]
	public Transform _holding;
	public float _nullPeckDist;

	void Awake(){
		_instance=this;

	}
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void Pickup(Transform t){
		StartCoroutine(PickupR(t));
	}

	IEnumerator PickupR(Transform obj){
		Hop._instance.enabled=false;
		Vector3 startPos = transform.position;
		Vector3 endPos = obj!=null?obj.position+Vector3.up*_vertPickupOffset :
			startPos+transform.forward*_nullPeckDist;
		Quaternion startRot = transform.rotation;
		transform.Rotate(45f,0f,0f);
		Quaternion endRot=transform.rotation;
		float timer=0;
		while(timer<_pickupTime*0.5f){
			//go down
			float t = timer/(_pickupTime*0.5f);
			timer+=Time.deltaTime;
			transform.position=Vector3.Lerp(startPos,endPos,t);
			transform.rotation = Quaternion.Slerp(startRot,endRot,t);
			yield return null;
		}
		if(obj!=null)
			obj.SetParent(transform);
		_holding=obj;
		timer=0;
		while(timer<_pickupTime*0.5f){
			//come back up
			float t = timer/(_pickupTime*0.5f);
			timer+=Time.deltaTime;
			transform.position=Vector3.Lerp(endPos,startPos,t);
			transform.rotation = Quaternion.Slerp(endRot,startRot,t);
			yield return null;
		}
		transform.position=startPos;
		transform.rotation=startRot;
		Hop._instance.enabled=true;
	}

	public void TurnTo(Vector3 v){
		StartCoroutine(TurnToR(v));
	}

	IEnumerator TurnToR(Vector3 v){
		Hop._instance.enabled=false;
		_doneTurning=false;
		Quaternion startRot=transform.rotation;
		transform.LookAt(v);
		Quaternion endRot=transform.rotation;
		float timer = 0;
		while(timer<_turnTime){
			timer+=Time.deltaTime;
			transform.rotation=Quaternion.Slerp(startRot,endRot,timer/_turnTime);
			yield return null;
		}
		_doneTurning=true;
		Hop._instance.enabled=false;
	}

	//public bool HasItem
}
