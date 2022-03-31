using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ocean : MonoBehaviour
{
	bool _wet;
	MInput _mIn;

	public AudioClip _splash;
	public AudioClip _bubbles;
	public Transform _waterRings;
	public Transform _bubbleParts;
	public float _drownDur;
	public bool _drown;
	Bird _player;

	void Awake(){
		_mIn=GameManager._mIn;
		_player=GameManager._player;
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	void OnTriggerEnter(Collider other){
		if(_wet)
			return;
		if(other.GetComponent<Bird>()!=null){
			if(_drown)
				StartCoroutine(Drown(other.GetComponent<Bird>()));
			else{
				Sfx.PlayOneShot3D(_splash,_player.transform.position);
				Vector3 pos=_player.transform.position;
				pos.y=transform.position.y+0.01f;
				Transform rings = Instantiate(_waterRings);
				rings.position=pos;
				_player.InWater(true);
			}
		}
	}

	void OnTriggerExit(Collider other){
		if(!_drown)
			_player.InWater(false);
	}

	[ContextMenu("Test")]
	public void DrownTest(){
		StartCoroutine(Drown(GameManager._player));
	}

	IEnumerator Drown(Bird b){
		_wet=true;
		_mIn.LockInput(true);
		b.Drown();
		Sfx.PlayOneShot3D(_splash,b.transform.position);
		Sfx.PlayOneShot3D(_bubbles,b.transform.position);
		Vector3 pos=b.transform.position;
		pos.y=transform.position.y+0.01f;
		Transform rings = Instantiate(_waterRings);
		rings.position=pos;
		Instantiate(_bubbleParts,pos,Quaternion.identity);
		yield return new WaitForSeconds(_drownDur);
		b.Respawn();
		float dur = b.Ruffle();
		//yield return new WaitForSeconds(dur);
		b.ResetState();
		_mIn.LockInput(false);
		_wet=false;
	}
}
