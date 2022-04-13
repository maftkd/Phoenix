using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planter : MonoBehaviour
{
	public Terrain _terrain;
	public Transform _grassPrefab;
	public int _terrainLayer;
	public float _alphaThreshold;
	public int _seed;
	public Vector3 _minSize;
	public Vector3 _maxSize;
	public int _grassDensity;
	public float _plantChance;
	public bool _offsetVert;
	[Header("Controls")]
	public bool _plantTerrain;
	public bool _clearTerrain;
	public bool _autoUpdate;

	void OnValidate(){
		if(_plantTerrain||_autoUpdate){
			PlantGrass();
			_plantTerrain=false;
		}
		if(_clearTerrain){
			ClearGrass();
			_clearTerrain=false;
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

	public void ClearGrass(){
		StartCoroutine(Clear());
	}

	public void PlantGrass(){
		ClearGrass();
		TerrainData td = _terrain.terrainData;
		float [,,] alphaMaps = td.GetAlphamaps(0,0,td.alphamapWidth,td.alphamapHeight);
		int grassCount=0;
		Random.InitState(_seed);
		for(int y=0;y<td.alphamapHeight; y++){
			for(int x=0;x<td.alphamapWidth; x++){
				if(alphaMaps[x,y,_terrainLayer]>_alphaThreshold)
				{
					if(Random.value<_plantChance){
						grassCount++;
						float xFrac=(y/(float)(td.alphamapHeight-1));
						float zFrac=(x/(float)(td.alphamapWidth-1));
						float worldX=_terrain.transform.position.x+td.size.x*xFrac;
						float worldZ=_terrain.transform.position.z+td.size.z*zFrac;
						float worldY = _terrain.SampleHeight(new Vector3(worldX,0,worldZ));
						Transform grass = Instantiate(_grassPrefab, new Vector3(worldX,worldY,worldZ),Quaternion.Euler(0,Random.value*360f,0),transform);
						Vector3 scale = grass.localScale;
						scale.x*=Random.Range(_minSize.x,_maxSize.x);
						scale.y*=Random.Range(_minSize.y,_maxSize.y);
						scale.z*=Random.Range(_minSize.z,_maxSize.z);
						grass.localScale=scale;
						if(_offsetVert)
							grass.position+=Vector3.up*grass.localScale.y*0.5f;
					}
				}
			}
		}
		Debug.Log("Grass count: "+grassCount);
	}

	public void PlantTerrain(){
		//ClearGrass();
		TerrainData td = _terrain.terrainData;
		float [,,] alphaMaps = td.GetAlphamaps(0,0,td.alphamapWidth,td.alphamapHeight);
		int [,] detailMap = td.GetDetailLayer(0, 0, td.detailWidth, td.detailHeight, 0);
		Random.InitState(_seed);
		for(int y=0;y<td.alphamapHeight; y++){
			float yNorm = y/(float)td.alphamapHeight;
			int yDetail=Mathf.FloorToInt(yNorm*td.detailHeight);
			for(int x=0;x<td.alphamapWidth; x++){
				float xNorm = x/(float)td.alphamapWidth;
				int xDetail=Mathf.FloorToInt(xNorm*td.detailWidth);
				if(alphaMaps[x,y,_terrainLayer]>_alphaThreshold)
				{
					detailMap[xDetail,yDetail]=_grassDensity;
				}
			}
		}
		td.SetDetailLayer(0, 0, 0, detailMap);
		//Debug.Log("Grass count: "+grassCount);
	}

	public void ClearTerrain(){
		//ClearGrass();
		TerrainData td = _terrain.terrainData;
		float [,,] alphaMaps = td.GetAlphamaps(0,0,td.alphamapWidth,td.alphamapHeight);
		int [,] detailMap = td.GetDetailLayer(0, 0, td.detailWidth, td.detailHeight, 0);
		Random.InitState(_seed);
		for(int y=0;y<td.alphamapHeight; y++){
			float yNorm = y/(float)td.alphamapHeight;
			int yDetail=Mathf.FloorToInt(yNorm*td.detailHeight);
			for(int x=0;x<td.alphamapWidth; x++){
				float xNorm = x/(float)td.alphamapWidth;
				int xDetail=Mathf.FloorToInt(xNorm*td.detailWidth);
				detailMap[xDetail,yDetail]=0;
			}
		}
		td.SetDetailLayer(0, 0, 0, detailMap);
		//Debug.Log("Grass count: "+grassCount);
	}

	IEnumerator Clear(){
		int numChildren=transform.childCount;
		Transform [] children = new Transform[numChildren];
		for(int i=0;i<numChildren; i++){
			children[i]=transform.GetChild(i);
		}

		yield return null;

		for(int i=0; i<numChildren; i++){
			DestroyImmediate(children[i].gameObject);
		}
	}
}
