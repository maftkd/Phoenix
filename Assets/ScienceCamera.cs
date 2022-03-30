using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScienceCamera : MonoBehaviour
{
	Transform _camera;
	AudioSource _audio;
	Bird _player;
	Quaternion _targetRotation;
	public float _radius;

	void Awake(){
		_camera=transform.Find("camera");
		_audio=GetComponent<AudioSource>();
		_player=GameManager._player;
	}

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
		_camera.LookAt(_player.transform);
		_targetRotation=_camera.rotation;
    }

	void OnDrawGizmos(){
		Gizmos.color=Color.red;
		Gizmos.DrawWireSphere(transform.position,_radius);
	}

}
