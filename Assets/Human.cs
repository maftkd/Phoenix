using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human : MonoBehaviour
{
	float _hopTimer=0;
	[Range(0f,1f)]
	public float _inThresh;
	Vector3 _hopTarget;
	Vector3 _hopStart;
	Vector3 _midPos;
	public float _hopDist;
	public float _hopSpeed;
	float _hopTime;
	public float _hopHeight;
	Footstep _footstep;
	public Transform _endZone;

    // Start is called before the first frame update
    void Start()
    {
		_hopTime=_hopDist/_hopSpeed;
    }

    // Update is called once per frame
    void Update()
    {
		float horIn = Input.GetAxis("Horizontal");
		if(_hopTimer<=0 && Mathf.Abs(horIn)>_inThresh){
			Vector3 rayStart=transform.position+Vector3.up*10f;
			if(horIn>0){
				//go right
				rayStart.x+=_hopDist;
			}
			else{
				//go left
				rayStart.x-=_hopDist;
			}
			RaycastHit hit;
			if(Physics.Raycast(rayStart,Vector3.down, out hit, 15f,1)){
				_hopTarget=hit.point;
				_hopTimer=_hopTime;
				_hopStart=transform.position;
				_midPos=Vector3.Lerp(_hopStart,_hopTarget,0.5f);
				_midPos+=Vector3.up*_hopHeight;
				_footstep=hit.transform.GetComponent<Footstep>();
			}
		}
		if(_hopTimer>0)
		{
			float t = 1f-_hopTimer/_hopTime;
			if(t<0.5f)
				transform.position=Vector3.Lerp(_hopStart,_midPos,t*2f);
			else
				transform.position=Vector3.Lerp(_midPos,_hopTarget,(t-0.5f)*2f);

			_hopTimer-=Time.deltaTime;
			if(_hopTimer<=0){
				if(_footstep!=null)
					_footstep.Sound(transform.position);
				transform.position=_hopTarget;
			}
			if((transform.position-_endZone.position).sqrMagnitude<1f)
			{
				Debug.Log("End zone!");
				GameManager._instance.NextLevel();
				enabled=false;
			}
		}
    }

	void OnDrawGizmos(){
		Gizmos.color=Color.blue;
		if(_hopTarget!=null){
			Gizmos.DrawSphere(_hopTarget,0.25f);
		}
		Gizmos.color=Color.green;
		if(_endZone!=null){
			Gizmos.DrawWireSphere(_endZone.position,1.0f);
		}
	}
}
