using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseSparrow : MonoBehaviour
{

	[Header("Fly in")]
	public float _flyInDist;
	public float _flySpeed;
	Animator _anim;

	[Header("Audio")]
	public AudioClip _song;
	public Transform _songParts;
	public float _songVolume;
	public AudioClip _flapSound;
	public float _flapDur;

	[Header("Turn 180")]
	public float _turnDur;
	public float _turnDelay;
	public float _jumpHeight;
	public AnimationCurve _jumpCurve;

	void Awake(){
		_anim=GetComponent<Animator>();
	}

    // Start is called before the first frame update
    void Start()
    {
		StartCoroutine(ArriveAtHouse());
    }

	IEnumerator ArriveAtHouse(){
		Vector3 endPos=transform.position;
		Vector3 dir=transform.forward-Vector3.up;
		dir.Normalize();

		Vector3 startPos=endPos-dir*_flyInDist;
		float dist = _flyInDist;
		float travelDist=0;
		float timer=0;
		float speed=_flySpeed;
		float flapTimer=0;

		transform.position=startPos;
		_anim.SetTrigger("flyLoop");
		while(travelDist<dist){
			timer+=Time.deltaTime;
			flapTimer+=Time.deltaTime;
			if(flapTimer>_flapDur){
				flapTimer=0;
				Sfx.PlayOneShot3D(_flapSound,transform.position);
			}
			Vector3 v = dir*Time.deltaTime*speed;
			travelDist+=v.magnitude;
			transform.position+=v;
			yield return null;
		}
		transform.position=endPos;
		_anim.SetTrigger("land");

		//wait
		yield return new WaitForSeconds(_turnDelay);

		//180
		timer=0;
		float dur=_turnDur;
		Quaternion startRot=transform.rotation;
		transform.Rotate(Vector3.up*179f*MRandom.RandSign());
		Quaternion endRot=transform.rotation;
		startPos=transform.position;
		Vector3 jumpPos=startPos+Vector3.up*_jumpHeight;
		Sfx.PlayOneShot3DVol(_flapSound,transform.position,0.1f);
		_anim.SetTrigger("sing");
		Transform parts = Instantiate(_songParts);
		parts.position=jumpPos;
		while(timer<dur){
			timer+=Time.deltaTime;
			float frac=timer/dur;
			transform.rotation=Quaternion.Slerp(startRot,endRot,frac);
			transform.position=Vector3.Lerp(startPos,jumpPos,_jumpCurve.Evaluate(frac));
			yield return null;
		}

		//sing
		Sfx.PlayOneShot3D(_song,transform.position,Random.Range(0.8f,1.2f),_songVolume);
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
