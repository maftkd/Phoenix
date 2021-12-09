using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
	public Canvas _foodMenu;
	public AudioClip _nomNom;
	public AudioClip _chalp;
	AudioSource _audio;
	public Transform _holdTarget;
	//#temp - probably won't keep track of individual berries outside of this prototype
	[HideInInspector]
	public int _berries;

	void Awake(){
		_instance=this;

	}
    // Start is called before the first frame update
    void Start()
    {
		_audio = transform.Find("EatAudio").GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
		if(_holding!=null && _holding.tag=="Food"){
			if(Input.GetKeyDown(KeyCode.E)){
				StartCoroutine(EatR());
			}
			else if(Input.GetKeyDown(KeyCode.R)){
				DropIt();
			}
		}
    }

	public void Pickup(Transform t){
		if(Hop._instance.enabled==false)
			return;
		if(t.tag=="Food"){
			_foodMenu.enabled=true;
		}
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
		_holding.position=_holdTarget.position;
	}

	public void TurnTo(Vector3 v){
		StartCoroutine(TurnToR(v));
	}

	IEnumerator TurnToR(Vector3 v){
		Hop._instance.enabled=false;
		_doneTurning=false;
		Quaternion startRot=transform.rotation;
		transform.LookAt(v);
		Vector3 eulers = transform.eulerAngles;
		eulers.x=0;
		transform.eulerAngles=eulers;
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

	public void LoseItem(){
		if(_holding.tag=="Food"){
			_foodMenu.enabled=false;
		}
		_holding=null;
	}

	IEnumerator EatR(){
		//nom nom
		_audio.clip=_nomNom;
		float dur = _nomNom.length;
		_audio.Play();
		yield return new WaitForSeconds(dur);
		//chalp
		_audio.clip=_chalp;
		_audio.Play();
		Debug.Log("yum");
		//#refactor - once we have a generic food, the type of food should be a var within
		if(_holding.name.Contains("Berry"))
			_berries++;
		Destroy(_holding.gameObject);
		LoseItem();
	}

	void DropIt(){
		//re-position
		RaycastHit hit;
		if(Physics.Raycast(_holding.position,Vector3.down,out hit, 100f,1)){
			//cast ray to ground
			_holding.position=hit.point;
			if(hit.transform.GetComponent<Footstep>()!=null){
				//play footstep audio
				hit.transform.GetComponent<Footstep>().Sound(hit.point);
			}
		}
		else{
			//if no hit, just put at player's foot level
			Vector3 holdingPos=_holding.position;
			holdingPos.y=Hop._instance.transform.position.y;
			_holding.position=holdingPos;
		}
		//#refactor
		if(_holding.GetComponent<Seed>()!=null){
			_holding.GetComponent<Seed>().Reset();
		}
		else if(_holding.GetComponent<Worm>()!=null){
			_holding.GetComponent<Worm>().Reset();
		}
		
		//clear parent
		_holding.SetParent(null);

		LoseItem();
	}

	//public bool HasItem
}
