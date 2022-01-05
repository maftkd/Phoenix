using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolPath : MonoBehaviour
{
	LineRenderer _line;
	public int _curIndex;
	float _position;
	Transform _tool;
	Vector3 _curDir;
	float _curMagnitude;

	void Awake(){
		gameObject.SetActive(false);
		_line=transform.GetComponentInChildren<LineRenderer>();
		_curIndex=0;
		_position=0;
		_tool=transform.Find("Tool");
	}

	void OnEnable(){
		_tool.position=GetPosition();
	}

	void OnDisable(){

	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void EnableCanvas(bool en){
		gameObject.SetActive(en);
	}

	public Vector3 GetPosition(){
		for(int i=0;i<_line.positionCount-1; i++){
			if(_position>=(float)i&&_position<i+1){
				_curIndex=i;
				float intPart=Mathf.FloorToInt(_position);
				float t = _position-intPart;
				Vector3 localStart=_line.GetPosition(_curIndex);
				Vector3 localEnd=_line.GetPosition(_curIndex+1);
				Vector3 startPoint = transform.TransformPoint(localStart);
				Vector3 endPoint = transform.TransformPoint(localEnd);
				_curDir=localEnd-localStart;
				_curMagnitude=_curDir.magnitude;
				_curDir/=_curMagnitude;
				return Vector3.Lerp(startPoint,endPoint,t);
			}
		}
		return Vector3.zero;
	}

	public float GetPositionF(){
		return _position;
	}

	public void MoveTool(Vector3 input){
		float dt = Vector3.Dot(input,_curDir);
		float linearMovement=dt;//*input.magnitude;
		//Vector3 movement=_curDir*dt*input.magnitude;
		_position+=linearMovement;
		if(_position>=_curIndex+1)
		{
			//next
			if(_position>=_line.positionCount-1){
				_position=_curIndex+1f;
			}
			else
			{
				_curIndex++;
				_position=_curIndex;
			}
		}
		else if(_position<_curIndex){
			//prev
			if(_curIndex>0)
			{
				_curIndex--;
				_position=_curIndex+0.999f;
			}
			else
			{
				_curIndex=0;
				_position=_curIndex;
			}
		}
		_tool.position=GetPosition();
	}
}
