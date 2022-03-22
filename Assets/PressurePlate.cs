using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
	Collider [] _cols;
	BoxCollider _box;
	Transform _button;
	Vector3 _defaultPos;
	Vector3 _halfExtents;
	public LayerMask _birdMask;
	public float _hitBoxY;
	public int _state;
	public float _pressDepth;
	public float _pressSpeed;
	public Vector4 _pitchRange;
	Bird _player;
	public AudioClip _buttonDown;
	Color _emissionColor;
	Color _defaultColor;

	bool _powered;
	bool _active;

	public Circuit [] _next;

	public bool _isToggle;

	Material _mat;
	public Transform _icon;
	Material _quad;
	bool _init;

	void Awake(){
		if(!_init)
			Init();
	}

	void Init(){
		_cols=new Collider[3];
		_button=transform.GetChild(0);
		_defaultPos=_button.localPosition;
		_box=_button.GetComponent<BoxCollider>();
		_halfExtents=_box.size*0.5f;
		_halfExtents.y=_hitBoxY;
		_player = GameManager._player;
		_mat=_button.GetComponent<MeshRenderer>().material;
		if(_icon!=null)
			_quad=_icon.GetComponent<Renderer>().material;
		_emissionColor=GameManager._color.GetButtonEmissionColor(transform);
		_defaultColor=_mat.GetColor("_Color");
		_init=true;
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		//#temp
		_halfExtents.y=_hitBoxY;
		//center is transform.position+
		int hits=Physics.OverlapBoxNonAlloc(_button.position+Vector3.up*_box.size.y,_halfExtents,_cols,Quaternion.identity,_birdMask);
		switch(_state){
			case 0://idle
				if(hits>0&&HitsAbove(hits)){
					StartCoroutine(ButtonDown());
					//_cols[0].GetComponent<Bird>().Ground();
				}
				break;
			case 1://going down
				break;
			case 2://pressed
				if(hits==0){
					StopAllCoroutines();
					StartCoroutine(ButtonUp());
					if(!_isToggle)
						Power(false);
				}
				break;
			default:
				break;
		}
    }

	IEnumerator ButtonDown(){
		_state=1;
		Sfx.PlayOneShot3D(_buttonDown,transform.position,Random.Range(_pitchRange.x,_pitchRange.y));
		float timer=0;
		Vector3 start=_button.localPosition;
		Vector3 end=_defaultPos+Vector3.down*_pressDepth;
		float dist=(end-start).magnitude;
		float dur=dist/_pressSpeed;
		while(timer<dur)
		{
			timer+=Time.deltaTime;
			_button.localPosition=Vector3.Lerp(start,end,timer/dur);
			yield return null;
		}
		_button.localPosition=end;
		_state=2;

		if(_isToggle)
			TogglePower();
		else{
			Power(true);
		}
	}

	IEnumerator ButtonUp(){
		_state=3;
		Sfx.PlayOneShot3D(_buttonDown,transform.position,Random.Range(_pitchRange.z,_pitchRange.w));
		float timer=0;
		Vector3 start=_button.localPosition;
		Vector3 end=_defaultPos;
		float dist=(end-start).magnitude;
		float dur=dist/_pressSpeed;
		while(timer<dur)
		{
			timer+=Time.deltaTime;
			_button.localPosition=Vector3.Lerp(start,end,timer/dur);
			yield return null;
		}
		_button.localPosition=end;
		_state=0;
	}

	bool HitsAbove(int numHits){
		//Debug.Log("checking "+numHits+" hits");
		for(int i=0; i<numHits; i++){
			Collider c = _cols[i];
			if(c.transform.position.y+0.001f<_button.position.y+_box.size.y)
			{
				return false;
			}
			if(Mathf.Abs(c.transform.position.x-_button.position.x)>_halfExtents.x)
			{
				return false;
			}
			if(Mathf.Abs(c.transform.position.z-_button.position.z)>_halfExtents.z)
			{
				return false;
			}
			if(_player.GoingUp())
			{
				return false;
			}
		}
		return true;
	}

	void TogglePower(){
		_powered=!_powered;
		UpdateWires();
	}

	void Power(bool p){
		_powered=p;
		UpdateWires();
	}

	void UpdateWires(){
		if(!_active)
			return;
		_mat.SetColor("_EmissionColor", _powered? _emissionColor : Color.black);
		if(_quad!=null)
			_quad.SetFloat("_Lerp",_powered?1:0);
		foreach(Circuit n in _next)
			n.Power(_powered);
	}

	public void Activate(bool active){
		if(!_init)
			Init();
		_mat.SetColor("_EmissionColor", Color.black);
		_mat.SetColor("_Color", active? _defaultColor : Color.black);
		_quad.SetFloat("_OutlineThickness",active?0.1f:0.5f);
		_active=active;
	}

	void OnDrawGizmos(){
		if(_halfExtents!=null&&_box!=null)
		{
			Gizmos.color=Color.red;
			Gizmos.DrawWireCube(_button.position+Vector3.up*_box.size.y,_halfExtents*2f);
		}
	}
}
