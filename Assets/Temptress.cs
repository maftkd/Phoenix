using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Temptress : MonoBehaviour
{
	public float _range;
	public AudioClip _flee;
	AudioSource _audio;
	public float _flySpeed;
    // Start is called before the first frame update
    void Start()
    {
		Footstep [] feet = FindObjectsOfType<Footstep>();
		foreach(Footstep f in feet){
			f.OnFootstep+=HearFootstep;
		}
		_audio=GetComponent<AudioSource>();
    }

	void OnDestroy(){
		Footstep [] feet = FindObjectsOfType<Footstep>();
		foreach(Footstep f in feet){
			f.OnFootstep-=HearFootstep;
		}
	}

    // Update is called once per frame
    void Update()
    {

    }

	void HearFootstep(Footstep.FootstepEventArgs args){
		if((args.pos-transform.position).sqrMagnitude<_range*_range){
			StartCoroutine(FlyAwayR());
		}
	}

	IEnumerator FlyAwayR(){
		_audio.clip=_flee;
		_audio.Play();
		while(transform.position.x<5f*16f/9f){
			transform.position+=Vector3.right*Time.deltaTime*_flySpeed;
			yield return null;
		}
		Destroy(gameObject);
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.green;
		Gizmos.DrawWireSphere(transform.position,_range);
	}
}
