using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cage : MonoBehaviour
{
	bool _open;
	public float _openAngle;
	public AudioClip _openSound;
	public AudioClip _keySound;
	Transform _door;
	public float _openDelay;
	Cardinal _cardinal;

	void Awake(){
		_door=transform.Find("CageDoor");
		_cardinal=transform.GetComponentInChildren<Cardinal>();
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void Open(){
		if(_open)
			return;
		StartCoroutine(OpenR());
	}

	IEnumerator OpenR(){
		_open=true;
		Sfx.PlayOneShot3D(_keySound,transform.position);
		yield return new WaitForSeconds(_openDelay);

		Quaternion startRot=_door.rotation;
		_door.Rotate(Vector3.up*_openAngle);
		Quaternion endRot=_door.rotation;
		Sfx.PlayOneShot3D(_openSound,transform.position);


		float timer=0;
		float dur=1f;

		while(timer<dur){
			timer+=Time.deltaTime;
			float frac=timer/dur;
			_door.rotation=Quaternion.Slerp(startRot,endRot,frac);
			yield return null;
		}

		_cardinal.FlyAway();
	}
}
