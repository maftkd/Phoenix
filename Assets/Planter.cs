using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planter : MonoBehaviour
{
	public Transform _grassPrefab;
	public int _terrainLayer;
	public float _alphaThreshold;
	public float _plantChance;
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
		ClearGrass();
		Terrain t = FindObjectOfType<Terrain>();
		Debug.Log(t.name);
		TerrainData td = t.terrainData;
		float [,,] alphaMaps = td.GetAlphamaps(0,0,td.alphamapWidth,td.alphamapHeight);
		int grassCount=0;
		for(int y=0;y<td.alphamapHeight; y++){
			for(int x=0;x<td.alphamapWidth; x++){
				if(alphaMaps[x,y,_terrainLayer]>_alphaThreshold)
				{
					grassCount++;
					if(Random.value<_plantChance){
						float xFrac=(y/(float)(td.alphamapHeight-1));
						float zFrac=(x/(float)(td.alphamapWidth-1));
						float worldX=t.transform.position.x+td.size.x*xFrac;
						float worldZ=t.transform.position.z+td.size.z*zFrac;
						float worldY = t.SampleHeight(new Vector3(worldX,0,worldZ));
						Transform grass = Instantiate(_grassPrefab, new Vector3(worldX,worldY,worldZ),Quaternion.Euler(0,Random.value*360f,0),transform);
						grass.position+=Vector3.up*grass.localScale.y*0.5f;
					}
				}
			}
		}
		Debug.Log("Grass count: "+grassCount);
	}
}
