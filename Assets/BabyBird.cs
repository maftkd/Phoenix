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
					_anim.SetTrigger("cry");
					_tears.Play();
					_squeak.Play();
					_state=1;
				}
				break;
			case 1://cry
				break;
			default:
				break;
		}
    }

	void OnDrawGizmos(){
		Gizmos.color=Color.green;
		Gizmos.DrawWireSphere(transform.position,_cryZone);
	}
}
