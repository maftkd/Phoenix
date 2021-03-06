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
	public bool _incSeed;
	public bool _decSeed;
	public int _plantCount;
	public bool _incCount;
	public bool _decCount;
	public Vector3 _minSize;
	public Vector3 _maxSize;
	public float _minSpacing;
	public int _grassDensity;
	public int _detailLayer;
	public float _minHeight;
	public float _maxHeight;
	public bool _offsetVert;
	[Header("Controls")]
	public bool _plantTransform;
	public bool _clearTransform;
	public bool _plantDetail;
	public bool _clearDetails;
	public bool _autoUpdate;

	void OnValidate(){
		if(_incSeed)
		{
			_seed++;
			_incSeed=false;
		}
		if(_decSeed){
			_seed--;
			_decSeed=false;
		}
		if(_incCount)
		{
			_plantCount++;
			_incCount=false;
		}
		if(_decCount){
			_plantCount--;
			_decCount=false;
		}
		if(_plantTransform||_autoUpdate){
			PlantGrass();
			_plantTransform=false;
		}
		if(_plantDetail){
			ClearDetail();
			PlantDetail();
			_plantDetail=false;
		}
		if(_clearTransform){
			ClearGrass();
			_clearTransform=false;
		}
		if(_clearDetails){
			ClearDetail();
			_clearDetails=false;
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
		if(!gameObject.activeSelf)
			return;
		StartCoroutine(ClearAndPlant());
	}

	public void PlantTransform(){
		TerrainData td = _terrain.terrainData;
		float [,,] alphaMaps = td.GetAlphamaps(0,0,td.alphamapWidth,td.alphamapHeight);
		int grassCount=0;
		//Debug.Log("Initting random state with seed");
		Random.InitState(_seed);
		//Debug.Log("Planting: "+_grassPrefab.name+", seed is: "+_seed);
		for(int i=0;i<_plantCount; i++){
			int iters=0;
			bool spotFound=false;
			while(iters<100&&!spotFound){
				iters++;
				//get random position
				float xFrac=Random.value;
				float zFrac=Random.value;
				int alphaMapZ=Mathf.RoundToInt(xFrac*(td.alphamapHeight-1));
				int alphaMapX=Mathf.RoundToInt(zFrac*(td.alphamapWidth-1));
				//check terrain layer
				if(alphaMaps[alphaMapX,alphaMapZ,_terrainLayer]<_alphaThreshold)
					continue;
				float worldX=_terrain.transform.position.x+td.size.x*xFrac;
				float worldZ=_terrain.transform.position.z+td.size.z*zFrac;
				float worldY = _terrain.SampleHeight(new Vector3(worldX,0,worldZ));
				//check height
				if(worldY<_minHeight||worldY>_maxHeight)
					continue;
				Vector3 p = new Vector3(worldX,worldY,worldZ);
				bool canPlant=true;
				//check spacing with other trees
				foreach(Transform t in transform){
					float sqrDist=(p-t.position).sqrMagnitude;
					if(sqrDist<_minSpacing*_minSpacing)
					{
						canPlant=false;
						break;
					}

				}
				if(canPlant){
					grassCount++;
					Transform grass = Instantiate(_grassPrefab, p,Quaternion.Euler(0,Random.value*360f,0),transform);
					if(_offsetVert)
						grass.position+=Vector3.up*grass.localScale.y*0.5f;
					if(grass.GetComponent<MTree>()!=null)
						grass.GetComponent<MTree>().GenTree();
					spotFound=true;
				}
			}
		}
		/*
		for(int y=0;y<td.alphamapHeight; y++){
			for(int x=0;x<td.alphamapWidth; x++){
				if(alphaMaps[x,y,_terrainLayer]>_alphaThreshold)
				{
					if(Random.value<_plantChance){
						float xFrac=(y/(float)(td.alphamapHeight-1));
						float zFrac=(x/(float)(td.alphamapWidth-1));
					}
				}
			}
		}
		*/
		//Debug.Log("Grass count: "+grassCount);
	}

	public void PlantDetail(){
		TerrainData td = _terrain.terrainData;
		float [,,] alphaMaps = td.GetAlphamaps(0,0,td.alphamapWidth,td.alphamapHeight);
		int [,] detailMap = td.GetDetailLayer(0, 0, td.detailWidth, td.detailHeight, _detailLayer);
		for(int y=0;y<td.alphamapHeight; y++){
			float yNorm = y/(float)td.alphamapHeight;
			int yDetail=Mathf.FloorToInt(yNorm*td.detailHeight);
			for(int x=0;x<td.alphamapWidth; x++){
				float xNorm = x/(float)td.alphamapWidth;
				int xDetail=Mathf.FloorToInt(xNorm*td.detailWidth);
				if(x>0&&x<td.alphamapWidth-1&&y>0&&y<td.alphamapWidth-1)
				{
					float worldX=_terrain.transform.position.x+td.size.x*yNorm;
					float worldZ=_terrain.transform.position.z+td.size.z*xNorm;
					float worldY = _terrain.SampleHeight(new Vector3(worldX,0,worldZ));
					if(worldY>_minHeight&&worldY<_maxHeight&&alphaMaps[x-1,y,_terrainLayer]>_alphaThreshold&&
							alphaMaps[x+1,y,_terrainLayer]>_alphaThreshold&&
							alphaMaps[x,y-1,_terrainLayer]>_alphaThreshold&&
							alphaMaps[x,y+1,_terrainLayer]>_alphaThreshold&&
							alphaMaps[x-1,y-1,_terrainLayer]>_alphaThreshold&&
							alphaMaps[x+1,y-1,_terrainLayer]>_alphaThreshold&&
							alphaMaps[x-1,y+1,_terrainLayer]>_alphaThreshold&&
							alphaMaps[x+1,y+1,_terrainLayer]>_alphaThreshold&&
							alphaMaps[x,y,_terrainLayer]>_alphaThreshold){
						detailMap[xDetail,yDetail]=_grassDensity;

					}
				}
			}
		}
		td.SetDetailLayer(0, 0, _detailLayer, detailMap);
		//Debug.Log("Grass count: "+grassCount);
	}

	public void ClearDetail(){
		TerrainData td = _terrain.terrainData;
		float [,,] alphaMaps = td.GetAlphamaps(0,0,td.alphamapWidth,td.alphamapHeight);
		int [,] detailMap = td.GetDetailLayer(0, 0, td.detailWidth, td.detailHeight, _detailLayer);
		for(int y=0;y<td.alphamapHeight; y++){
			float yNorm = y/(float)td.alphamapHeight;
			int yDetail=Mathf.FloorToInt(yNorm*td.detailHeight);
			for(int x=0;x<td.alphamapWidth; x++){
				float xNorm = x/(float)td.alphamapWidth;
				int xDetail=Mathf.FloorToInt(xNorm*td.detailWidth);
				detailMap[xDetail,yDetail]=0;
			}
		}
		td.SetDetailLayer(0, 0, _detailLayer, detailMap);
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
			if(children[i]!=null)
				DestroyImmediate(children[i].gameObject);
		}
	}

	IEnumerator ClearAndPlant(){
		int numChildren=transform.childCount;
		Transform [] children = new Transform[numChildren];
		for(int i=0;i<numChildren; i++){
			children[i]=transform.GetChild(i);
		}

		yield return null;

		for(int i=0; i<numChildren; i++){
			DestroyImmediate(children[i].gameObject);
		}
		PlantTransform();
	}
}
