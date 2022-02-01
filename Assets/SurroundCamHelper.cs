using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurroundCamHelper : MonoBehaviour
{
	public float _outerRadius;
	public float _innerRadius;
	Transform _player;
	bool _playerInZone;
	MCamera _mCam;
	Bird _mate;
	MeshRenderer _mesh;
	public Material _outline;
	public Material _noDraw;

	void Awake(){
		_player=GameManager._player.transform;
		_mCam = GameManager._mCam;
		_mate=_player.GetComponent<Bird>()._mate;
		//assume first mesh is box with 2 materials
		_mesh=transform.GetChild(0).GetComponent<MeshRenderer>();
		//Outline(false);
	}

	void OnDisable(){
		if(_mCam!=null)
			_mCam.DefaultCam();
		//Outline(false);
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		float sqrMag = (_player.position-transform.position).sqrMagnitude;
		if(!_playerInZone||_mCam.IsDefaultCam()){
			if(sqrMag<_outerRadius*_outerRadius){
				Surround();
				//Outline(true);
			}
		}
		else{
			if(sqrMag>_outerRadius*_outerRadius || sqrMag<_innerRadius*_innerRadius){
				_playerInZone=false;
				_mCam.DefaultCam();
				//Outline(false);
			}
		}
        
    }

	public void Surround(){
		_playerInZone=true;
		_mCam.Surround(transform);
	}

	void Outline(bool line){
		Material[] mats = _mesh.materials;
		mats[1]=line? _outline : _noDraw;
		_mesh.materials=mats;
	}

	void OnDrawGizmos(){
		if(!enabled)
			return;
		Gizmos.color=Color.green;
		Gizmos.DrawWireSphere(transform.position,_outerRadius);
		Gizmos.color=Color.blue;
		Gizmos.DrawWireSphere(transform.position,_innerRadius);
	}
}
