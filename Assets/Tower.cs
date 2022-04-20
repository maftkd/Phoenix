using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Tower : MonoBehaviour
{
	public BirdHouse [] _houses;
	public bool _allSolved;
	public LineRenderer _beam;
	public Transform _otherBulb;
	public GameObject _forceField;
	public Renderer [] _rings;
	public UnityEvent _onDoStuff;

	void Awake(){
		CheckHouses();
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		/*
		if(Input.GetKeyDown(KeyCode.T)){
			DoStuff();
		}
		*/
    }

	public void CheckHouses(){
		_allSolved=true;
		for(int i=0;i<_houses.Length; i++){
			_rings[i].material.SetColor("_EmissionColor",_houses[i]._solved?_rings[i].material.color*2: Color.white*-1);
			if(!_houses[i]._solved)
				_allSolved=false;

		}
		if(_allSolved)
		{
			DoStuff();
		}
	}

	public void DoStuff(){
		Debug.Log("Doing stuff");
		if(_beam.positionCount==2){
			Debug.Log("Really doing stuff");
			Vector3[] positions=new Vector3[3];
			Vector3[] beamPos=new Vector3[2];
			_beam.GetPositions(beamPos);
			positions[0]=beamPos[0];
			positions[1]=beamPos[1];
			positions[2]=_otherBulb.position;
			//_beam.SetPosition(2,_otherBulb.position);
			_beam.positionCount=3;
			_beam.SetPositions(positions);
			Destroy(_forceField);

			_onDoStuff.Invoke();
		}

	}
}
