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
	[Header("Water")]
	public float _breathDur;
	float _breathTimer;
	Water _water;
	[Header("Food")]
	public float _foodRadius;
	public float _fullness;
	Material _bellyMat;

	public static float _height=0.25f;
	public static float _width=0.2f;
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
		_breathTimer=_breathDur;
		_bellyMat=GetComponent<MeshRenderer>().material;
		_bellyMat.SetFloat("_Full",_fullness);
    }

    // Update is called once per frame
    void Update()
    {
		switch(_state){
			case 0:
			default:
				if(_water==null&&_breathTimer<_breathDur){
					//regain breath
					_breathTimer+=Time.deltaTime;
					float ti = 1f-(_breathTimer/_breathDur);
					_vignette.SetT(ti);
				}
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
				if(Input.anyKeyDown||Input.GetAxis("Horizontal")!=0){
					Debug.Log("First touch");
					_camera.GoFlying();
					_state=0;
					transform.rotation=Quaternion.identity;
				}
				break;
			case 3:
				//in water
				_breathTimer-=Time.deltaTime;
				float t = 1f-(_breathTimer/_breathDur);
				_vignette.SetT(t);
				break;
		}

		if(transform.position.x>5*1.7f){
			Debug.Log("End zone!");
			GameManager._instance.NextLevel();
			//enabled=false;
		}
    }

	public void EnterWater(Water w){
		_water=w;
		_state=3;
		_breathTimer=_breathDur;
	}

	public void ExitWater(){
		_water=null;
		_state=0;
	}

	public bool InWater(){
		return _water!=null;
	}

	public void Sloosh(){
		_water.Sloosh();
	}

	public void Eat(float nut){
		_fullness+=nut;
		if(_fullness>1f)
			_fullness=1f;
		_bellyMat.SetFloat("_Full",_fullness);
	}
}
