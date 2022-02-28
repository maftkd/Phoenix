using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
	public float _baseHeight;
	static int  _maxPuzzlesPerRow=5;
	static float _puzzleSpacing=3f;

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
		Vector3 center=transform.position+Vector3.right*td.size.x*0.5f+Vector3.forward*td.size.z*0.5f+Vector3.up*_baseHeight;
		ResetPuzzleBoxes(center);
	}

	void ResetPuzzleBoxes(Vector3 center){
		Transform puzzleParent=transform.parent.Find("Puzzles");
		if(puzzleParent==null){
			Debug.Log("Couldn't find puzzles, returning");
			return;
		}
		Debug.Log("Resetting puzzle boxes");
		for(int i=0; i<puzzleParent.childCount; i++){
			int row=i/_maxPuzzlesPerRow;
			int col=i%_maxPuzzlesPerRow;
			Transform t = puzzleParent.GetChild(i);
			t.position=center+Vector3.right*col*_puzzleSpacing+Vector3.forward*row*_puzzleSpacing;
			t.rotation=Quaternion.identity;
			t.Rotate(Vector3.up*180f);
		}
	}
}
