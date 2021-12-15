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
	public LayerMask _waterMask;
	Collider [] _cols;
	Water _water;
	public float _breathDur;
	float _breathTimer;
	[Header("Food")]
	public LayerMask _foodMask;
	public float _foodRadius;
	public float _fullness;
	Material _bellyMat;
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
		_cols = new Collider[4];
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
				if(Physics.OverlapSphereNonAlloc(transform.position,0.01f,_cols,_waterMask)>0){
					//hit water
					Debug.Log("Splash");
					foreach(Collider c in _cols){
						if(c!=null&&c.GetComponent<Water>()!=null)
						{
							_water=c.GetComponent<Water>();
							_water.Splash(transform);
						}
					}
					_state=3;
					_breathTimer=_breathDur;
				}
				else{
					if(_breathTimer<_breathDur){
						_breathTimer+=Time.deltaTime;
						float t = 1f-(_breathTimer/_breathDur);
						_vignette.SetT(t);
					}
				}
				if(Physics.OverlapSphereNonAlloc(transform.position,_foodRadius,_cols,_foodMask)>0){
					//hit food
					Debug.Log("nom");
					foreach(Collider c in _cols){
						if(c!=null&&c.GetComponent<Food>()!=null)
						{
							Food f = c.GetComponent<Food>();
							_fullness+=f.GetEaten();
							if(_fullness>1f)
								_fullness=1f;
							_bellyMat.SetFloat("_Full",_fullness);
						}
					}
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
				if(Input.anyKeyDown){
					Debug.Log("First touch");
					_camera.GoFlying();
					_state=0;
					transform.rotation=Quaternion.identity;
				}
				break;
			case 3:
				//in water
				if(Physics.OverlapSphereNonAlloc(transform.position,0.01f,_cols,_waterMask)==0){
					//hit water
					Debug.Log("Out of water");
					if(_water!=null){
						_water.Unsplash();
						_water=null;
					}
					_state=0;
				}
				else{
					_breathTimer-=Time.deltaTime;
					float t = 1f-(_breathTimer/_breathDur);
					_vignette.SetT(t);
				}
				break;
		}

		if(transform.position.x>5*1.7f){
			Debug.Log("End zone!");
			GameManager._instance.NextLevel();
			//enabled=false;
		}
    }

	public bool InWater(){
		return _water!=null;
	}

	public void Sloosh(){
		_water.Sloosh();
	}
}
