using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdHouse : MonoBehaviour
{
	Bird _player;
	public GameObject _interior;

	void Awake(){
		_player=GameManager._player;
		_interior.SetActive(false);
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void Enter(){
		_player.WalkInNestBox(transform,_interior);
	}
	public void Exit(){
		_player.WalkOutNestBox(transform,_interior);
	}
}
