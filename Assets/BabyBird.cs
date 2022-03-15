using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BabyBird : MonoBehaviour
{
	Animator _anim;
	public float _cryZone;
	int _state;
	Bird _player;
	ParticleSystem _tears;
	AudioSource _squeak;
	[Header("Animation")]
	public float _receiveTime;
	public float _eatTime;
	[Header("Audio")]
	public AudioClip _nomNom;
	public float _nomNomVol;
	public AudioClip _gulp;
	public float _gulpVol;

	void Awake(){
		_anim=GetComponent<Animator>();
		_player=GameManager._player;
		_tears = MUtility.FindRecursive(transform,"Tears").GetComponent<ParticleSystem>();
		_squeak = transform.Find("Squeak").GetComponent<AudioSource>();
	}

	void OnEnable(){
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		switch(_state){
			case 0://sleep
				if((_player.transform.position-transform.position).sqrMagnitude<_cryZone*_cryZone){
					Cry();
				}
				break;
			case 1://cry
				break;
			case 2://eat food
				break;
			default:
				break;
		}
    }

	public bool CanEat(){
		return _state==1;
	}

	public void GetFed(){
		_tears.Stop();
		_squeak.Stop();
		StartCoroutine(Eat());
	}

	IEnumerator Eat(){
		_anim.SetTrigger("receive");
		Sfx.PlayOneShot2D(_nomNom,Random.Range(0.8f,1.2f),_nomNomVol);
		yield return new WaitForSeconds(_receiveTime);
		_anim.SetTrigger("eat");
		Sfx.PlayOneShot2D(_gulp,Random.Range(0.8f,1.2f),_gulpVol);
		yield return new WaitForSeconds(_eatTime);
		Cry();
	}

	void Cry(){
		_anim.SetTrigger("cry");
		_tears.Play();
		_squeak.Play();
		_state=1;
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.green;
		Gizmos.DrawWireSphere(transform.position,_cryZone);
	}
}
