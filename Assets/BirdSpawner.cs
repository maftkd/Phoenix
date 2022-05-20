using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdSpawner : MonoBehaviour
{

	[System.Serializable]
	public struct SpawnGroup{
		public string _name;
		public Transform _birdPrefab;
		public Material _altMat;
		public int _numSpawn;
		public int _spawnTerrainLayer;
	}

	public int _seed;
	public SpawnGroup [] _spawnGroup;
	public Terrain _terrain;

	void Awake(){
		SpawnBirds();
	}

	void SpawnBirds(){
		Random.InitState(_seed);
		_terrain = transform.parent.GetComponentInChildren<Terrain>();
		TerrainData td = _terrain.terrainData;
		float [,,] alphaMaps = td.GetAlphamaps(0,0,td.alphamapWidth,td.alphamapHeight);
		foreach(SpawnGroup sg in _spawnGroup){
			for(int i=0; i<sg._numSpawn; i++){
				//#temp - we assume right now that we spawn birds on ground because we are starting with robins
				//but other birds should be spawned in trees, some in the water, and some in the air
				int iters=0;
				bool spotFound=false;
				while(iters<1000&&!spotFound){
					float xFrac=Random.value;
					float zFrac=Random.value;
					int alphaMapZ=Mathf.RoundToInt(xFrac*(td.alphamapHeight-1));
					int alphaMapX=Mathf.RoundToInt(zFrac*(td.alphamapWidth-1));
					float worldX=_terrain.transform.position.x+td.size.x*xFrac;
					float worldZ=_terrain.transform.position.z+td.size.z*zFrac;
					float worldY=_terrain.SampleHeight(new Vector3(worldX,0,worldZ));
					if(worldY<5)
						continue;
					if(alphaMaps[alphaMapX,alphaMapZ,sg._spawnTerrainLayer]>0.5f){
						spotFound=true;
						Transform bird = Instantiate(sg._birdPrefab,new Vector3(worldX,worldY,worldZ),Quaternion.identity,transform);
						if(i%2==0&&sg._altMat!=null)
							bird.GetChild(0).GetComponent<Renderer>().material=sg._altMat;
						//Debug.Log("spot found in: "+iters+" iters");
					}
					iters++;
				}
			}
		}
	}
}
