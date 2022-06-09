using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Herald : MonoBehaviour
{
	public float _radius;
	int _state;
	Transform _player;
	Transform _joystick;
	Transform _dialog;
	NPB _npb;

	void Awake(){
		_player=GameObject.Find("Player").transform;
		_joystick=GameObject.Find("Joystick").transform;
		_dialog=GameObject.Find("Dialog").transform;
		_dialog.gameObject.SetActive(false);
		_npb=GetComponent<NPB>();
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		switch(_state){
			case 0:
				if((_player.position-transform.position).sqrMagnitude<_radius*_radius){
					_state=1;
					Debug.Log("Herald time");
					//disable joystick
					_joystick.gameObject.SetActive(false);
					//enable dialog
					_dialog.gameObject.SetActive(true);
					//disble calling
					_npb.enabled=false;
				}
				break;
			case 1:
				break;
			default:
				break;
		}
    }
}
