using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormBed : MonoBehaviour
{
	Transform _mainCam;
	bool _inZone;
	bool _prevInZone;
	List<Worm> _worms;
	public static WormBed _instance;
	public float _peckRange;
	public float _peckDot;

	void Awake(){
		_worms = new List<Worm>();
		_instance = this;
	}

    // Start is called before the first frame update
    void Start()
    {
		_mainCam=Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
		_inZone=Mathf.Abs(transform.position.x-_mainCam.position.x)<transform.localScale.x*0.5f&&
			Mathf.Abs(transform.position.z-_mainCam.position.z)<transform.localScale.z*0.5f;

		if(_inZone!=_prevInZone){
			OnMouseEnter();
		}

		_prevInZone=_inZone;
    }

	void OnMouseEnter(){
		if(_inZone)
		{
			Crosshair._instance.SetOverItem(name);
		}
	}

	void OnMouseExit(){
		Crosshair._instance.ClearOverItem(name);
	}

	void OnMouseDown(){
		if(Fly._instance.enabled)
			return;
		//Peck._instance.Pickup(transform);
		//checking
		Vector3 camForward = _mainCam.forward;
		Worm pecked = null;
		for(int i=_worms.Count-1; i>=0; i--){
			Worm w = _worms[i];
			Vector3 camToWorm = w.transform.position-_mainCam.position;
			if(camToWorm.magnitude<_peckRange){
				if(Vector3.Dot(camToWorm.normalized,camForward)>_peckDot){
					Debug.Log("We got a peck!");
					pecked=w;
					break;
				}
			}
		}
		if(pecked==null){
			Peck._instance.Pickup(null);
		}
		else{
			Peck._instance.Pickup(pecked.transform);
			pecked.enabled=false;
			pecked.transform.GetChild(1).gameObject.SetActive(false);
		}
	}

	public void RegisterWorm(Worm w){
		_worms.Add(w);
	}
}
