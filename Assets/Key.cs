using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
	public Transform _groundEffectsPrefab;
	Transform _groundEffects;
	public AudioClip _choir;
	Transform _player;
	public int _id;
	void Awake(){
		RaycastHit hit;
		if(Physics.Raycast(transform.position,Vector3.down, out hit, 1f, 1)){
			_groundEffects = Instantiate(_groundEffectsPrefab,hit.point,Quaternion.identity);
		}
		else
			_groundEffects = Instantiate(_groundEffectsPrefab,transform.position,Quaternion.identity);
		_player=GameObject.FindGameObjectWithTag("Player").transform;
	}
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void CollectKey(){
		Destroy(_groundEffects.gameObject);
		//play some sound
		Sfx.PlayOneShot2D(_choir);
		_player.GetComponent<Bird>().CollectKey(transform);
	}

	public int GetId(){
		return _id;
	}
}
