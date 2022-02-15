using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	[ContextMenu("Level")]
	public void LevelTerrain(){
		Terrain t = GetComponent<Terrain>();
		TerrainData td = t.terrainData;
		int res = td.heightmapResolution;
		float[,] heights = new float[res,res];
		for(int z=0;z<res;z++){
			for(int x=0;x<res;x++){
				heights[z,x]=0;
			}
		}
		td.SetHeights(0,0,heights);
	}
}
