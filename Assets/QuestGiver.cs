using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestGiver : MonoBehaviour
{
	Bird _player;
	bool _questGiven=false;
	public float _minDist;

	void Awake(){
		_player=GameManager._player;
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if(!_questGiven){
			if((_player.transform.position-transform.position).sqrMagnitude<_minDist*_minDist){
				Debug.Log("Giving quest");
				_questGiven=true;
			}
		}
        
    }
}
