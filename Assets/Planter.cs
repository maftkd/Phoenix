using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planter : MonoBehaviour
{
	public Terrain _terrain;
	public Transform _grassPrefab;
	public int _terrainLayer;
	public float _alphaThreshold;
	public float _plantChance;
	public Vector3 _minSize;
	public Vector3 _maxSize;
	public bool _offsetVert;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	[ContextMenu("Clear")]
	public void ClearGrass(){
		for(int i=transform.childCount-1;i>=0;i--){
			DestroyImmediate(transform.GetChild(i).gameObject);
		}
	}

	[ContextMenu("Plant")]
	public void PlantGrass(){
		TerrainData td = _terrain.terrainData;
		float [,,] alphaMaps = td.GetAlphamaps(0,0,td.alphamapWidth,td.alphamapHeight);
		int grassCount=0;
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
}
