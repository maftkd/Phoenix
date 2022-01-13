using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightSensor : MonoBehaviour
{
	Transform _player;
	Material _mat;
	Bird _bird;

	void Awake(){
		_player=GameObject.FindGameObjectWithTag("Player").transform;
		_mat=GetComponent<MeshRenderer>().material;
		_bird=_player.GetComponent<Bird>();
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		float diff = transform.position.y-_player.position.y-_bird._size.y*2f;
		diff=-diff*2f;
		_mat.SetFloat("_HeightMark",Mathf.Max(0f,diff));
    }
}
