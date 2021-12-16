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
		float y = transform.position.y;
		float t=0;
		float phaseShift=Random.value*Mathf.PI*2f;
		float offset=Mathf.Sin(phaseShift);
		while(transform.position.x<5f*16f/9f){
			t+=Time.deltaTime;
			Vector3 pos = transform.position+Vector3.right*Time.deltaTime*_flySpeed;
			pos.y=y+Mathf.Sin(t*Mathf.PI*2f+phaseShift)-offset;
			transform.position=pos;
			yield return null;
		}
		Destroy(gameObject);
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.green;
		Gizmos.DrawWireSphere(transform.position,_range);
	}
}
