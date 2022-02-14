using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clouds : MonoBehaviour
{
	public Transform [] _cloudPrefabs;
	public int _minClouds;
	public int _maxClouds;
	public Vector2 _distanceRange;
	public Vector2 _verticalRange;
	public Vector3 _minScale;
	public Vector3 _maxScale;
	public bool _onlyCloudOne;

	void Awake(){
		int numClouds = Random.Range(_minClouds,_maxClouds+1);
		float thetaSlice=Mathf.PI*2f/numClouds;
		for(float theta=0;theta<Mathf.PI*2;theta+=thetaSlice){
			float radius = Random.Range(_distanceRange.x,_distanceRange.y);
			float x = Mathf.Cos(theta)*radius;
			float z = Mathf.Sin(theta)*radius;
			float y = Random.Range(_verticalRange.x,_verticalRange.y);
			Vector3 pos = new Vector3(x,y,z);
			int index=_onlyCloudOne? 0 : Random.Range(0,_cloudPrefabs.Length);
			Transform cloud = Instantiate(_cloudPrefabs[index],pos,Quaternion.identity,transform);
			cloud.localScale=new Vector3(Random.Range(_minScale.x,_maxScale.x),
					Random.Range(_minScale.y,_maxScale.y),
					Random.Range(_minScale.z,_maxScale.z));
		}
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
