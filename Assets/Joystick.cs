using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Joystick : MonoBehaviour
{
	Vector3 _mousePos;
	Vector3 _velocity;
	Transform _player;
	public bool _incMovement;
	float _moveTimer;
	public float _movePeriod;
	public float _moveSpeed;
	Transform _cam;

	void Awake(){
		_player=GameObject.Find("Player").transform;
		_cam=GameObject.Find("GameViewCam").transform;
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if(Input.GetMouseButton(0)){
			_mousePos=Input.mousePosition;
			Vector3 dir=(_mousePos-transform.position).normalized;
			dir.z=dir.y;
			dir.y=0;
			if(_incMovement){
				_moveTimer+=Time.deltaTime;
				if(_moveTimer>=_movePeriod){
					float theta=Mathf.Atan2(dir.z,dir.x);
					theta/=(Mathf.PI*0.5f);
					theta=Mathf.Round(theta)*(Mathf.PI*0.5f);
					dir.x=Mathf.Cos(theta);
					dir.z=Mathf.Sin(theta);
					_player.position+=dir*_moveSpeed;
					_player.forward=dir;
					_moveTimer=0f;
				}
			}
			else{
				Vector3 pos=_player.position+dir*Time.deltaTime*_moveSpeed;
				_player.forward=dir;
				if(pos.x-5>_cam.position.x)
				{
					//pos.x=_player.position.x;
					_cam.position+=Vector3.right*10f;
				}
				if(pos.x+5<_cam.position.x)
				{
					//pos.x=_player.position.x;
					_cam.position+=Vector3.left*10f;
				}
				if(pos.z-5>_cam.position.z)
				{
					//pos.z=_player.position.z;
					_cam.position+=Vector3.forward*10f;
				}
				if(pos.z+5<_cam.position.z)
				{
					//pos.z=_player.position.z;
					_cam.position+=Vector3.back*10f;
				}
				_player.position=pos;
			}
		}
		if(Input.GetMouseButtonUp(0)){
			_moveTimer=0;
		}
    }

	void OnDrawGizmos(){
		Gizmos.color=Color.blue;
		Gizmos.DrawWireSphere(_mousePos,40f);
	}
}
