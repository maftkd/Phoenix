using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TerrainGenerator : MonoBehaviour
{
	public float _baseHeight;
	static int  _maxPuzzlesPerRow=5;
	static float _puzzleSpacing=3f;
	public bool _levelTerrain;

	[Header("Base map")]
	public Texture2D _baseMap;
	public int _texRes;
	public int _seed;
	public float _shoreScale;
	public float _shoreNoiseAmplitude;
	public float _shoreCutoff;
	public float _shoreSmooth;
	public bool _autoGenBaseMap;
	public bool _applyBaseMap;
	public bool _hideBaseMap;

	[Header("Ridge map")]
	public Texture2D _ridgeMap;
	public bool _genRidgeMap;
	public bool _hideRidgeMap;
	public bool _genRidgeOnTransform;

	[Header("Height map")]
	public Texture2D _heightMap;
	public int _heightSeed;
	public float _heightNoiseScale;
	public float _minHeightNoise;
	public bool _autoGenHeightMap;
	public bool _autoApplyHeightMap;
	public bool _hideHeightMap;

	[Header("Distance map")]
	public Texture2D _distanceMap;
	public float _distMult;
	public bool _genDistanceMap;
	public bool _hideDistanceMap;

	[Header("Combine Maps")]
	public float _heightAmplitude;
	public float _mountainStart;
	public AnimationCurve _mountainCurve;
	public float _noiseAmplitude;
	public bool _applyCombined;
	public bool _autoApplyCombined;

	void OnValidate(){
		if(_levelTerrain){
			LevelTerrain();
			_levelTerrain=false;
		}
		//base map
		if(_autoGenBaseMap)
			GenBaseMap();
		if(_applyBaseMap)
		{
			ApplyBaseMap();
			_applyBaseMap=false;
		}

		//ridge map
		if(_genRidgeMap)
		{
			GenRidgeMap();
			_genRidgeMap=false;
		}

		//height map
		if(_autoGenHeightMap)
			GenHeightMap();

		//distance map
		if(_genDistanceMap){
			GenDistanceMap();
			_genDistanceMap=false;
		}

		if(_applyCombined||_autoApplyCombined){
			ApplyCombinedMaps();
			_applyCombined=false;
		}

		transform.GetChild(0).GetComponent<MeshRenderer>().enabled=!_hideBaseMap;
		transform.GetChild(1).GetComponent<MeshRenderer>().enabled=!_hideRidgeMap;
		transform.GetChild(2).GetComponent<MeshRenderer>().enabled=!_hideHeightMap;
		transform.GetChild(3).GetComponent<MeshRenderer>().enabled=!_hideDistanceMap;
	}

	void Update(){
		if(transform.hasChanged){
			if(_genRidgeOnTransform)
				GenRidgeMap();
			transform.hasChanged=false;
		}
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

	[ContextMenu("Generate BaseMap")]
	public void GenBaseMap(){
		_baseMap = new Texture2D(_texRes,_texRes);

		Random.InitState(_seed);
		float offset=Random.value;

		for(int y=0; y<_texRes; y++){
			float y01=y/(float)(_texRes-1);
			float yDiff=y01-0.5f;
			for(int x=0; x<_texRes; x++){
				float x01=x/(float)(_texRes-1);
				float xDiff=x01-0.5f;
				float rSqr=(xDiff*xDiff+yDiff*yDiff);
				float rSqrNoise = Mathf.PerlinNoise((x01+offset)*_shoreScale,(y01+offset)*_shoreScale);
				rSqr+=rSqrNoise*_shoreNoiseAmplitude;
				float height01=Mathf.InverseLerp(_shoreCutoff,_shoreCutoff-_shoreSmooth,rSqr);
				_baseMap.SetPixel(x,y,Color.red*height01);
			}
		}

		_baseMap.Apply();

		transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MainTex",_baseMap);
	}

	void ApplyBaseMap(){
		Terrain t = GetComponent<Terrain>();
		TerrainData td = t.terrainData;
		int res = td.heightmapResolution;
		float[,] heights = new float[res,res];
		float maxHeight = td.size.y;
		float b = _baseHeight/maxHeight;
		for(int z=0;z<res;z++){
			float zNorm = z/(float)(res-1);
			for(int x=0;x<res;x++){
				float xNorm=x/(float)(res-1);
				int pixX=Mathf.FloorToInt(xNorm*_texRes);
				int pixY=Mathf.FloorToInt(zNorm*_texRes);
				float baseMult = _baseMap.GetPixel(pixX,pixY).r;
				heights[z,x]=baseMult*b;
			}
		}
		td.SetHeights(0,0,heights);
		//Vector3 center=transform.position+Vector3.right*td.size.x*0.5f+Vector3.forward*td.size.z*0.5f+Vector3.up*_baseHeight;
		//ResetPuzzleBoxes(center);
	}

	public void GenRidgeMap(){
		_ridgeMap = new Texture2D(_texRes,_texRes);
		RidgeLine ridge = FindObjectOfType<RidgeLine>();
		Vector3 minWorld=transform.position;
		Terrain t = GetComponent<Terrain>();
		TerrainData td = t.terrainData;
		Vector3 maxWorld=minWorld+td.size;
		for(int y=0; y<_texRes;y++){
			float yNorm=y/(float)(_texRes-1);
			for(int x=0; x<_texRes; x++){
				float xNorm=x/(float)(_texRes-1);
				//convert pixel coordinate to world space coordinate - based on pixel center
				Vector3 worldPos = Vector3.up*_baseHeight;
				worldPos.x=Mathf.Lerp(minWorld.x,maxWorld.x,xNorm);
				worldPos.z=Mathf.Lerp(minWorld.z,maxWorld.z,yNorm);
				float height=ridge.GetHeight(worldPos);

				_ridgeMap.SetPixel(x,y,Color.green*height);
			}
		}
		MImage.Blur(_ridgeMap,4,false);
		_ridgeMap.Apply();

		transform.GetChild(1).GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MainTex",_ridgeMap);
	}

	public void GenHeightMap(){
		_heightMap = new Texture2D(_texRes,_texRes);

		Random.InitState(_heightSeed);
		float offset=Random.value;

		for(int y=0; y<_texRes; y++){
			float yNorm=y/(float)(_texRes-1);
			for(int x=0; x<_texRes; x++){
				float xNorm=x/(float)(_texRes-1);
				float noise = Mathf.PerlinNoise((xNorm+offset)*_heightNoiseScale,(yNorm+offset)*_heightNoiseScale);
				_heightMap.SetPixel(x,y,Color.blue*noise);
			}
		}
		_heightMap.Apply();
		transform.GetChild(2).GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MainTex",_heightMap);
	}

	public void ApplyHeightMap(){
	}

	public void GenDistanceMap(){
		_distanceMap = new Texture2D(_texRes,_texRes);

		Random.InitState(_heightSeed);
		float offset=Random.value;

		for(int y=0; y<_texRes; y++){
			float yNorm=y/(float)(_texRes-1);
			for(int x=0; x<_texRes; x++){
				float xNorm=x/(float)(_texRes-1);
				float baseHeight=_baseMap.GetPixel(x,y).r;
				if(baseHeight>=1f){
					//_distanceMap.SetPixel(x,y,baseHeight*Color.magenta);
					//calc distance from nearest water
					int minDist=_texRes;
					//check horizontal
					for(int x1=0;x1<_texRes;x1++){
						float h = _baseMap.GetPixel(x1,y).r;
						if(h<1f){
							int dist=Mathf.Abs(x1-x);
							if(dist<minDist)
							{
								minDist=dist;
							}
						}
					}
					//check vertical
					for(int y1=0;y1<_texRes;y1++){
						float h = _baseMap.GetPixel(x,y1).r;
						if(h<1f){
							int dist=Mathf.Abs(y1-y);
							if(dist<minDist)
							{
								minDist=dist;
							}
						}
					}
					//check diagonal slope 1
					int xStart=x-y;
					if(xStart<0)
						xStart=0;
					int yStart=y-x;
					if(yStart<0)
						yStart=0;
					int y2=yStart;
					for(int x2=xStart;x2<_texRes&&y2<_texRes;x2++){
						float h = _baseMap.GetPixel(x2,y2).r;
						if(h<1f){
							float dstSqr=(x2-x)*(x2-x)+(y2-y)*(y2-y);
							int dist=dstSqr==0? 0 : Mathf.RoundToInt(Mathf.Sqrt(dstSqr));
							if(dist<minDist)
							{
								minDist=dist;
							}
						}
						y2++;
					}
					//check diagonal slope -1 
					xStart=x-(_texRes-y);
					if(xStart<0)
						xStart=0;
					yStart=(_texRes-y)-x;
					if(yStart<0)
						yStart=0;
					y2=yStart;
					for(int x2=xStart;x2<_texRes&&y2>0;x2++){
						float h = _baseMap.GetPixel(x2,y2).r;
						if(h<1f){
							float dstSqr=(x2-x)*(x2-x)+(y2-y)*(y2-y);
							int dist=dstSqr==0? 0 : Mathf.RoundToInt(Mathf.Sqrt(dstSqr));
							if(dist<minDist)
							{
								minDist=dist;
							}
						}
						y2--;
					}


					float distFrac=minDist/(float)_texRes;
					distFrac=Mathf.Clamp01(distFrac*_distMult);
					_distanceMap.SetPixel(x,y,distFrac*Color.magenta);

				}
				else{
					_distanceMap.SetPixel(x,y,Color.white*0f);
				}
			}
		}

		_distanceMap.Apply();
		transform.GetChild(3).GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MainTex",_distanceMap);
	}

	public void ApplyCombinedMaps(){
		Terrain t = GetComponent<Terrain>();
		TerrainData td = t.terrainData;
		int res = td.heightmapResolution;
		float[,] heights = new float[res,res];
		float maxHeight = td.size.y;
		float b = _baseHeight/maxHeight;
		for(int z=0;z<res;z++){
			float zNorm = z/(float)(res-1);
			for(int x=0;x<res;x++){
				float xNorm=x/(float)(res-1);
				int pixX=Mathf.FloorToInt(xNorm*_texRes);
				int pixY=Mathf.FloorToInt(zNorm*_texRes);

				//get base height and distance to shore
				float baseMult = _baseMap.GetPixel(pixX,pixY).r;
				float height=baseMult*_baseHeight;
				float distance=_distanceMap.GetPixel(pixX,pixY).r;

				//add mountains
				float hOffset=Mathf.InverseLerp(_mountainStart,1f,distance);
				hOffset=_mountainCurve.Evaluate(hOffset)*_heightAmplitude;

				//add random noise
				float noise = _heightMap.GetPixel(pixX,pixY).b;
				hOffset+=noise*distance*_noiseAmplitude;

				//get ridge map
				float ridge = _ridgeMap.GetPixel(pixX,pixY).g;
				height+=hOffset*ridge;
				
				heights[z,x]=height/maxHeight;
			}
		}
		td.SetHeights(0,0,heights);

	}
}
