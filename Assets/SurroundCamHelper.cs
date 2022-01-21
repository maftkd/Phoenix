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
		_player=GameObject.FindGameObjectWithTag("Player").transform;
		_mCam = Camera.main.transform.parent.GetComponent<MCamera>();
		_mate=_player.GetComponent<Bird>()._mate;
		//assume first mesh is box with 2 materials
		_mesh=transform.GetChild(0).GetComponent<MeshRenderer>();
		Material[] mats = _mesh.materials;
		mats[1]=_noDraw;
		_mesh.materials=mats;
	}

	void OnDisable(){
		if(_mCam!=null)
			_mCam.DefaultCam();
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
				Material[] mats = _mesh.materials;
				mats[1]=_outline;
				_mesh.materials=mats;
			}
		}
		else{
			if(sqrMag>_outerRadius*_outerRadius || sqrMag<_innerRadius*_innerRadius){
				_playerInZone=false;
				_mCam.DefaultCam();
				Material[] mats = _mesh.materials;
				mats[1]=_noDraw;
				_mesh.materials=mats;
			}
		}
        
    }

	public void Surround(){
		_playerInZone=true;
		_mCam.Surround(transform);
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
