using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapScreen : MonoBehaviour
{
	public Island _curIsland;

	void Awake(){
		RegenMap();
	}
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
		if(Input.GetKeyDown(KeyCode.F2)){
			RegenMap();
		}
    }

	void RegenMap(){
		MapGen mGen=_curIsland.transform.Find("MapCam").GetComponent<MapGen>();
		mGen.GenerateMap();
	}
}
