using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
	public float _baseHeight;
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
		float r = 0.5f;
		float maxHeight = td.size.y;
		float b = _baseHeight/maxHeight;
		for(int z=0;z<res;z++){
			float zNorm = z/(float)(res-1);
			for(int x=0;x<res;x++){
				float xNorm=x/(float)(res-1);
				Vector2 diff = new Vector2(xNorm,zNorm)-Vector2.one*0.5f;
				float dstSqr = Vector2.Dot(diff,diff);
				heights[z,x]=dstSqr<r*r ? b: 0;
			}
		}
		td.SetHeights(0,0,heights);
	}
}
