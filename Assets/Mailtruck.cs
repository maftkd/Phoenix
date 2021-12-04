using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mailtruck : MonoBehaviour
{
	public Transform _tunnelWest;
	public Transform _tunnelEast;
	public bool _deliverOnStart;
	public float _driveSpeed;
	public float _dropOffTime;
	AudioSource _driveSound;
	AudioSource _honkSound;
    // Start is called before the first frame update
    void Start()
    {
		_driveSound = transform.GetChild(0).GetComponent<AudioSource>();
		_honkSound = transform.GetChild(1).GetComponent<AudioSource>();
		Vector3 startPos=_tunnelWest.position;
		startPos.y=0;
		transform.position = startPos;
		if(_deliverOnStart)
			StartCoroutine(DeliverMailR());
    }

	IEnumerator DeliverMailR(){
		_driveSound.Play();
		while(transform.position.x<0){
			//go to mailbox
			transform.position+=Vector3.right*Time.deltaTime*_driveSpeed;
			yield return null;
		}
		_driveSound.Stop();
		//drop off mail
		yield return new WaitForSeconds(_dropOffTime);
		_driveSound.Play();
		StartCoroutine(HonkR());
		while(transform.position.x<_tunnelEast.position.x){
			//speed off
			transform.position+=Vector3.right*Time.deltaTime*_driveSpeed;
			yield return null;
		}
	}

	IEnumerator HonkR(){
		_honkSound.Play();
		yield return new WaitForSeconds(_honkSound.clip.length);
		_honkSound.Play();
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
