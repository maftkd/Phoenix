using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Herald : MonoBehaviour
{
	public float _radius;
	int _state;
	Transform _player;
	Transform _joystick;
	NPB _npb;
	Dialog _dialog;

	void Awake(){
		_player=GameObject.Find("Player").transform;
		_joystick=GameObject.Find("Joystick").transform;
		_dialog=GameObject.Find("Dialog").transform.GetComponent<Dialog>();;
		_dialog.Hide();
		//_dialog.gameObject.SetActive(false);
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
					//disable joystick
					_joystick.gameObject.SetActive(false);
					//enable dialog
					//_dialog.gameObject.SetActive(true);
					_dialog.ShowText("Hello World");
					//disble calling
					_npb.enabled=false;
				}
				break;
			case 1:
				if(Input.GetMouseButtonDown(0)){
					Debug.Log("Time for more text");
					_dialog.ShowText("Moar Text!");
				}
				break;
			default:
				break;
		}
    }
}
