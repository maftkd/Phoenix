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
	[Tooltip("Perlin noise scale")]
	public float _shoreScale;
	[Tooltip("Multiplied by radius from center. Affects how much the shore is distorted")]
	[Range(0,1)]
	public float _shoreNoiseAmplitude;
	[Range(0,1)]
	public float _shoreCutoff;
	[Tooltip("Affects the steepness of the shore, I think. Plays closely with shore Noise Amplitude. Todo, make this more descriptive")]
	[Range(0,1)]
	public float _shoreSmooth;
	public bool _autoGenBaseMap;
	public bool _autoApplyBaseMap;
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
	public bool _autoGenHeightMap;
	public bool _hideHeightMap;

	[Header("Distance map")]
	public Texture2D _distanceMap;
	[Tooltip("An arbitrary multiplier of the distance field to achieve desired gradient. Can be used for flat tops")]
	public float _distMult;
	public bool _genDistanceMap;
	public bool _hideDistanceMap;

	[Header("River map")]
	public Texture2D _riverMap;
	public int _riverSeed;
	[Tooltip("The minimum height at which a river head can spawn")]
	public float _minRiverHeight;
	[Tooltip("The maximum height at which a river head can spawn")]
	public float _maxRiverHeight;
	[Tooltip("The height at which the river stops generation. The mouth of the river...")]
	public float _seaLevel;
	public int _numRivers;
	[Tooltip("How much can a river flow upwards before it seeks a new direction")]
	public float _riseCapacity;
	public Material _riverMat;
	public float _riverWidth;
	[Tooltip("The step size of each river point calculation")]
	public float _riverInc;
	[Tooltip("The amount that the river geometry is lowered from the earth surface level")]
	public float _riverRecession;
	public AudioClip _riverClip;
	public bool _genRiverMap;
	public bool _hideRiverMap;
	public bool _clearRiverMap;

	[Header("Combine Maps")]
	[Tooltip("Mountain height multiplier. Should be called mountain amplitude")]
	public float _mountainAmplitude;
	[Range(0,1)]
	[Tooltip("Distance from shore at which mountains start")]
	public float _mountainStart;
	public AnimationCurve _mountainCurve;
	public float _noiseAmplitude;
	[Tooltip("Max depth cut into terrain for rivers")]
	public float _riverDepression;
	[Tooltip("X = min river dep. Y = max. Provides a gradient from higher up on terrain to sea level")]
	public Vector2 _riverDepressionRange;
	public float _puzzleRadius;
	public int _blurAmount;
	public bool _applyCombined;
	public bool _autoApplyCombined;

	[Header("Texture Pass")]
	public int _textureSeed;
	public float _textureNoiseScale;
	public int _numLayers;
	public int _sandLayer;
	public float _pebbleDot;
	public int _pebbleLayer;
	public float _grassDot;
	public int _grassLayer;
	public float _grassPerlinCutoff;
	public float _mountainHeight;
	public int _rockLayer;
	public float _rockPerlinCutoff;
	public int _dirtLayer;
	public int _puzzleSurfaceLayer;
	public int _puzzleSurfaceLayerHigh;
	public float _puzzleSurfaceLine;
	public bool _resetTexture;
	public bool _updateTexture;
	public bool _autoUpdateTexture;

	[Header("Rock Pass")]
	public int _rockSeed;
	public Transform [] _rocks;
	public int _numRocks;
	public Vector2 _rockHeightRange;
	public Vector3 _minRockScale;
	public Vector3 _maxRockScale;
	public bool _clearRocks;
	public bool _updateRocks;
	public bool _autoUpdateRocks;

	void OnValidate(){
		if(_levelTerrain){
			LevelTerrain();
			_levelTerrain=false;
		}
		//base map
		if(_autoGenBaseMap)
			GenBaseMap();
		if(_applyBaseMap||_autoApplyBaseMap)
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

		//river map
		if(_genRiverMap){
			GenRiverMap();
			_genRiverMap=false;
		}
		if(_clearRiverMap){
			ClearRiverMap();
			_clearRiverMap=false;
		}

		//apply combined
		if(_applyCombined||_autoApplyCombined){
			ApplyCombinedMaps();
			_applyCombined=false;
		}

		//texturing
		if(_resetTexture){
			ResetTexture();
			_resetTexture=false;
		}
		else if(_updateTexture||_autoUpdateTexture){
			UpdateTexture();
			_updateTexture=false;
		}

		//rocks
		if(_clearRocks){
			ClearRocks();
			_clearRocks=false;
		}
		else if(_updateRocks||_autoUpdateRocks){
			GenRocks();
			_updateRocks=false;
		}

		transform.GetChild(0).GetComponent<MeshRenderer>().enabled=!_hideBaseMap;
		transform.GetChild(1).GetComponent<MeshRenderer>().enabled=!_hideRidgeMap;
		transform.GetChild(2).GetComponent<MeshRenderer>().enabled=!_hideHeightMap;
		transform.GetChild(3).GetComponent<MeshRenderer>().enabled=!_hideDistanceMap;
		transform.GetChild(4).GetComponent<MeshRenderer>().enabled=!_hideRiverMap;
	}

	void Update(){
		//check for movement of terrain as whole
		if(transform.hasChanged){
			if(_genRidgeOnTransform)
				GenRidgeMap();
			transform.hasChanged=false;
		}

		//check for movement of puzzles
		Transform puzzleParent = transform.parent.Find("Puzzles");
		bool puzzleMoved=false;
		foreach(Transform puzzle in puzzleParent){
			if(!puzzle.gameObject.activeSelf)
				continue;
			if(puzzle.hasChanged){
				puzzleMoved=true;
			}
		}
		if(puzzleMoved&&_autoApplyCombined){
			ApplyCombinedMaps();
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

	public void ApplyCombinedMaps(bool ignorePuzzles=false){
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
				hOffset=_mountainCurve.Evaluate(hOffset)*_mountainAmplitude;

				//add random noise
				float noise = _heightMap.GetPixel(pixX,pixY).b;
				hOffset+=noise*distance*_noiseAmplitude;

				//get ridge map
				float ridge = _ridgeMap.GetPixel(pixX,pixY).g;
				height+=hOffset*ridge;

				//cut out river
				float river = _riverMap.GetPixel(pixX,pixY).g;
				float riverEffect=Mathf.InverseLerp(_riverDepressionRange.y,_riverDepressionRange.x,height);
				height-=river*_riverDepression*riverEffect;
				
				heights[z,x]=height/maxHeight;
			}
		}
		
		//handle snapping of puzzles to base terrain
		Transform puzzleParent = transform.parent.Find("Puzzles");
		foreach(Transform puzzle in puzzleParent){
			if(!puzzle.gameObject.activeSelf)
				continue;
			if(puzzle.hasChanged){
				Vector3 worldSpace=puzzle.position;
				if(worldSpace.x>transform.position.x&&worldSpace.x<transform.position.x+td.size.x &&
						worldSpace.z>transform.position.z&&worldSpace.z<transform.position.z+td.size.z){
					float yPos=t.SampleHeight(worldSpace);
					float xNorm=(worldSpace.x-transform.position.x)/td.size.x;
					float zNorm=(worldSpace.z-transform.position.z)/td.size.z;
					worldSpace.y=heights[Mathf.RoundToInt(zNorm*res),Mathf.RoundToInt(xNorm*res)]*maxHeight;
					puzzle.position=worldSpace;
				}
				puzzle.hasChanged=false;
			}
		}

		if(!ignorePuzzles){
			//flatten puzzle regions
			for(int z=0;z<res;z++){
				float zNorm = z/(float)(res-1);
				for(int x=0;x<res;x++){
					float xNorm=x/(float)(res-1);
					Vector3 worldPos=transform.position+Vector3.right*xNorm*td.size.x+Vector3.forward*zNorm*td.size.z;
					worldPos.y=heights[z,x]*maxHeight;
					//check each puzzle against each terrain point
					foreach(Transform puzzle in puzzleParent){
						if(!puzzle.gameObject.activeInHierarchy||puzzle.tag=="Float")
							continue;
						float sqrDst=(worldPos-puzzle.position).sqrMagnitude;
						if(sqrDst<_puzzleRadius*_puzzleRadius)
						{
							heights[z,x]=puzzle.position.y/maxHeight;
						}
					}
				}
			}
		}

		//smooth the heights
		float[,] heightsSmoothed = new float[res,res];
		MImage.Blur(heights,heightsSmoothed,_blurAmount,res);

		td.SetHeights(0,0,heightsSmoothed);
	}

	//river gen
	public void GenRiverMap(){
		ClearRiverMap();
		_riverMap = new Texture2D(_texRes,_texRes);

		//get terrain data
		Terrain ter = GetComponent<Terrain>();
		TerrainData td = ter.terrainData;
		int res = td.heightmapResolution;
		float maxHeight = td.size.y;
		Random.InitState(_riverSeed);

		//generate river data
		List<Vector2> river = new List<Vector2>();
		List<Vector3> riverPoints= new List<Vector3>();
		List<Vector3> riverNormals= new List<Vector3>();
		for(int nr=0;nr<_numRivers;nr++){
			riverPoints.Clear();
			riverNormals.Clear();
			//get river head
			float[,] heights=td.GetHeights(0,0,res,res);
			Vector2 riverHead=Vector2.zero;
			Vector2 riverPos=Vector2.zero;
			float riverHeight=0;
			int maxSteps=1000;
			float h=0;
			int i=0;
			while(riverHead==Vector2.zero&&i<maxSteps){
				//get random coord on terrain
				riverPos=new Vector2(Random.value*res,Random.value*res);
				int x = Mathf.FloorToInt(riverPos.x);
				int z = Mathf.FloorToInt(riverPos.y);
				h=heights[z,x];
				h*=maxHeight;
				if(h>_minRiverHeight&&h<_maxRiverHeight)
				{
					//convert terrain riverHead to texture coords
					float xNorm=x/(float)res;
					float zNorm=z/(float)res;
					riverHead = new Vector2(xNorm*_texRes,zNorm*_texRes);
					riverPoints.Add(new Vector3(transform.position.x+xNorm*td.size.x,
								h,
								transform.position.z+zNorm*td.size.z));
					riverNormals.Add(td.GetInterpolatedNormal(xNorm,zNorm));
					riverHeight=h;
				}
				i++;
			}
			if(i>=maxSteps){
				Debug.Log("Failed to find a start point in: "+maxSteps +" steps. Please optimize algorithm ;)");
				return;
			}

			//generate river - texture space
			bool canFlow=true;
			float theta=-1;
			i=0;
			int maxIters=50;
			Vector2 offset=Vector2.zero;

			int riverSize=0;
			while(canFlow&&i<maxSteps){
				//iterate over theta
				int thetaIter=0;
				if(theta!=-1){
					//if theta already set - check h
					int xPos=Mathf.RoundToInt(riverPos.x+offset.x);
					int zPos=Mathf.RoundToInt(riverPos.y+offset.y);

					//make sure position is within terrain bounds
					//todo also check if height is at water level
					if(xPos>=0&&xPos<res&&zPos>=0&&zPos<res){
						h=heights[zPos,xPos];
						h*=maxHeight;
					}
					else
						canFlow=false;
				}
				while(theta==-1 || canFlow&&h>riverHeight+_riseCapacity&&thetaIter<maxIters){
					//h is too high or theta has not been set
					theta=Mathf.PI*2f*Random.value;
					offset = new Vector2(Mathf.Cos(theta),Mathf.Sin(theta))*_riverInc;
					int xPos=Mathf.RoundToInt(riverPos.x+offset.x);
					int zPos=Mathf.RoundToInt(riverPos.y+offset.y);
					h=heights[zPos,xPos];
					h*=maxHeight;
					thetaIter++;
				}
				if(canFlow&&thetaIter>=maxIters){
					canFlow=false;
				}
				else if(canFlow){
					//update riverPos - XZ terrain space
					riverPos+=offset;
					if(h<riverHeight)//only update river height if h has actually fallen
						riverHeight=h;
					riverSize++;
					float xNorm=riverPos.x/(float)res;
					float zNorm=riverPos.y/(float)res;
					Vector2 rPos=new Vector2(xNorm*_texRes,zNorm*_texRes);
					if(!river.Contains(rPos))
					{
						river.Add(rPos);
						riverPoints.Add(new Vector3(transform.position.x+xNorm*td.size.x,
									h,
									transform.position.z+zNorm*td.size.z));
						riverNormals.Add(td.GetInterpolatedNormal(xNorm,zNorm));
					}
					if(riverHeight<=_seaLevel)
						canFlow=false;
				}
				i++;
			}
			Debug.Log("River size: "+riverSize);
			//create new river
			GameObject riverGo = new GameObject("River");
			riverGo.transform.SetParent(transform);
			River riverComp = riverGo.AddComponent<River>();
			riverComp.Setup(riverPoints,riverNormals,_riverMat,_riverWidth,_riverRecession,_riverClip);
		}

		//Paint river map
		for(int y=0; y<_texRes; y++){
			for(int x=0; x<_texRes; x++){
				bool inRiver=false;
				foreach(Vector2 v in river){
					if(x==Mathf.RoundToInt(v.x)&&y==Mathf.RoundToInt(v.y))
					{
						_riverMap.SetPixel(x,y,Color.cyan);
						inRiver=true;
					}
				}
				if(!inRiver)
					_riverMap.SetPixel(x,y,Color.white*0);
			}
		}
		_riverMap.Apply();
		transform.GetChild(4).GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MainTex",_riverMap);
	}

	//Clear rivers
	public void ClearRiverMap(){
		_riverMap = new Texture2D(_texRes,_texRes);
		for(int y=0; y<_texRes; y++){
			for(int x=0; x<_texRes; x++){
				_riverMap.SetPixel(x,y,Color.white*0);
			}
		}
		_riverMap.Apply();
		transform.GetChild(4).GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MainTex",_riverMap);

		//clear rivers
		River [] rivers = transform.GetComponentsInChildren<River>();
		StartCoroutine(DestroyRivers(rivers));

		ApplyCombinedMaps(true);
	}

	//destroy rivers - done in coroutine because editor doesn't like destroying onValidate
	IEnumerator DestroyRivers(River[] rivers){
		yield return null;
		for(int i=0; i<rivers.Length; i++){
			DestroyImmediate(rivers[i].gameObject);
		}
	}

	void ResetTexture(){
		Terrain ter = GetComponent<Terrain>();
		TerrainData td = ter.terrainData;
		int mapSize=_numLayers;
		float [,,] alphaMaps = new float[td.alphamapWidth,td.alphamapHeight,mapSize];
		for(int y=0;y<td.alphamapHeight;y++){
			for(int x=0;x<td.alphamapWidth;x++){
				for(int m=0;m<mapSize;m++){
					if(m==_sandLayer)
						alphaMaps[x,y,m]=1f;
					else
						alphaMaps[x,y,m]=0f;
				}
			}
		}
		td.SetAlphamaps(0,0,alphaMaps);

	}

	void UpdateTexture(){

		Random.InitState(_textureSeed);
		float offset=Random.value;
		float offset2=Random.value;

		//Get puzzle data
		Transform puzzleParent = transform.parent.Find("Puzzles");

		//get terrain data
		Terrain ter = GetComponent<Terrain>();
		TerrainData td = ter.terrainData;
		int mapSize=_numLayers;
		int res = td.heightmapResolution;
		float maxHeight = td.size.y;
		float[,] heights=td.GetHeights(0,0,res,res);
		float [,,] alphaMaps = new float[td.alphamapWidth,td.alphamapHeight,mapSize];
		for(int y=0;y<td.alphamapHeight;y++){
			float yNorm=y/(float)(td.alphamapHeight-1);
			for(int x=0;x<td.alphamapWidth;x++){
				float xNorm=x/(float)(td.alphamapWidth-1);
				
				//get height
				float height = 0;
				if(yNorm <1 && xNorm<1)
					height=heights[Mathf.FloorToInt(xNorm*res),Mathf.FloorToInt(yNorm*res)]*maxHeight;

				Vector3 worldPos=transform.position+xNorm*td.size.x*Vector3.forward+Vector3.right*yNorm*td.size.z;
				worldPos.y=height;

				bool puzzleZone=false;
				foreach(Transform puzzle in puzzleParent){
					if(!puzzle.gameObject.activeSelf||puzzle.tag=="Float")
						continue;
					float sqrDist = (puzzle.position-worldPos).sqrMagnitude;
					if(sqrDist<_puzzleRadius*_puzzleRadius)
					{
						puzzleZone=true;
						break;
					}
				}

				//get random
				float perlin = Mathf.PerlinNoise((xNorm+offset)*_textureNoiseScale,(yNorm+offset)*_textureNoiseScale);
				float perlin2 = Mathf.PerlinNoise((xNorm+offset2)*_textureNoiseScale,(yNorm+offset2)*_textureNoiseScale);

				//get dot and world normal
				Vector3 worldNormal = td.GetInterpolatedNormal(yNorm,xNorm);
				float upness = Vector3.Dot(worldNormal,Vector3.up);

				int layer=_sandLayer;
				if(upness>_grassDot&&height>_seaLevel&&height<_mountainHeight)
					layer=_grassLayer;
				else if(upness>_pebbleDot)
					layer=_pebbleLayer;
				if(height>_mountainHeight&&layer==_sandLayer){
					if(perlin<_rockPerlinCutoff)
						layer=_rockLayer;
					else
						layer=_pebbleLayer;
				}
				if(layer==_grassLayer){
					if(perlin2<1-_grassPerlinCutoff||puzzleZone)
						layer=_sandLayer;
				}

				for(int m=0;m<mapSize;m++){
					if(m==layer)
						alphaMaps[x,y,m]=1f;
					else
						alphaMaps[x,y,m]=0f;
				}
			}
		}
		td.SetAlphamaps(0,0,alphaMaps);
	}

	void ClearRocks(){
		Transform rockParent=transform.Find("Rocks");
		if(rockParent.childCount==0)
			return;
		Transform [] rocks = new Transform[rockParent.childCount];
		for(int i=0;i<rockParent.childCount; i++)
			rocks[i]=rockParent.GetChild(i);
		StartCoroutine(ClearRocksR(rocks));
	}

	IEnumerator ClearRocksR(Transform[] rocks){
		yield return null;
		foreach(Transform t in rocks)
			DestroyImmediate(t.gameObject);
	}

	void GenRocks(){
		Random.InitState(_rockSeed);
		ClearRocks();
		Transform rockParent=transform.Find("Rocks");
		//get terrain data
		Terrain ter = GetComponent<Terrain>();
		TerrainData td = ter.terrainData;
		int mapSize=_numLayers;
		int res = td.heightmapResolution;
		float maxHeight = td.size.y;
		float[,] heights=td.GetHeights(0,0,res,res);

		int maxIters=100;
		for(int i=0;i<_numRocks; i++){

			//find a spot where height is within range
			float height=-1f;
			int iter=0;
			float xNorm=0;
			float yNorm=0;
			while((height<_rockHeightRange.x||height>_rockHeightRange.y) && iter<maxIters){
				xNorm=Random.value;
				yNorm=Random.value;
				int xPos=Mathf.FloorToInt(xNorm*res);
				int yPos=Mathf.FloorToInt(yNorm*res);
				height = heights[yPos,xPos]*maxHeight;
				iter++;
			}

			Transform rock = Instantiate(_rocks[Random.Range(0,_rocks.Length)],rockParent);
			rock.position = transform.position+Vector3.right*xNorm*td.size.x+Vector3.forward*yNorm*td.size.z+
				Vector3.up*height;
			rock.localScale = new Vector3(Random.Range(_minRockScale.x,_maxRockScale.x),
					Random.Range(_minRockScale.y,_maxRockScale.y),
					Random.Range(_minRockScale.z,_maxRockScale.z));
			rock.rotation = Random.rotation;
		}
	}
}
