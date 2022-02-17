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

	void Awake(){
		_mIn=GameManager._mIn;
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
			Debug.Log("Bird in the water!");
			StartCoroutine(Drown(other.GetComponent<Bird>()));
		}
	}

	[ContextMenu("Test")]
	public void DrownTest(){
		StartCoroutine(Drown(GameManager._player));
	}

	IEnumerator Drown(Bird b){
		_wet=true;
		_mIn.LockInput(true);
		b.Drown();
		Debug.Log("Let's do a splash sound");
		Sfx.PlayOneShot3D(_splash,b.transform.position);
		Debug.Log("Let's do some bubbles");
		Sfx.PlayOneShot3D(_bubbles,b.transform.position);
		Debug.Log("And some rings");
		Vector3 pos=b.transform.position;
		pos.y=transform.position.y+0.01f;
		Transform rings = Instantiate(_waterRings);
		rings.position=pos;
		Debug.Log("And some bubbly sounds");
		Instantiate(_bubbleParts,pos,Quaternion.identity);
		yield return new WaitForSeconds(_drownDur);
		b.Respawn();
		float dur = b.Ruffle();
		yield return new WaitForSeconds(dur);
		b.ResetState();
		_mIn.LockInput(false);
		_wet=false;
	}
}
