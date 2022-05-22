using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mate : MonoBehaviour
{
	int _state;
	WayFinder _wayFinder;
	NPB _npb;

	void Awake(){
		_wayFinder=FindObjectOfType<WayFinder>();
		_npb=GetComponent<NPB>();
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void PatternSuccess(){
		switch(_state){
			case 0:
				Debug.Log("Time to deal cards!");
				_state=1;
				break;
			default:
				break;
		}

	}

	void HandleTargetChange(bool targetted){
		_wayFinder.ChangeColor(targetted?Color.white:Color.black);
	}
}
