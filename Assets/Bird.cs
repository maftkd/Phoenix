using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour
{
	Blink _blink;
	Vignette _vignette;
	int _state;
	public MCamera _camera;
	public int _numBlinks;
	public bool _startAwake;
    // Start is called before the first frame update
    void Start()
    {
		_blink=transform.GetComponentInChildren<Blink>();
		_vignette=transform.GetComponentInChildren<Vignette>();
		if(!_startAwake){
			_blink.DoBlinks(_numBlinks);
			_vignette.SetMax();
			_state=1;
		}
		else{
			_state=0;
			transform.rotation=Quaternion.identity;
			_blink.enabled=false;
			_vignette.SetMin();
		}
    }

    // Update is called once per frame
    void Update()
    {
		switch(_state){
			case 0:
			default:

				break;
			case 1:
				//blinking
				if(_blink.DoneBlinking())
				{
					_state=2;
					_blink.Unblink();
					_vignette.OpenUp();
				}
				break;
			case 2:
				//first movement
				if(Input.anyKeyDown){
					Debug.Log("First touch");
					_camera.GoFlying();
					_state=0;
					transform.rotation=Quaternion.identity;
				}
				break;
		}
    }
}
