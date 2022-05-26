using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BirdSpawner : MonoBehaviour
{

	[System.Serializable]
	public struct SpawnGroup{
		public string _name;
		public Transform _birdPrefab;
		public Material _altMat;
		public int _pairSpawn;
		public int _spawnTerrainLayer;
	}

	Dictionary <string,int> _firstSpawn;

	public int _seed;
	public SpawnGroup [] _spawnGroup;
	public Terrain _terrain;
	public static BirdSpawner _activeList;
	public static Transform _cards;
	public Transform _cardPrefab;

	//#temp?
	Dictionary<string,int> _birdCount;

	void Awake(){
		if(_cards==null)
			_cards=GameObject.FindGameObjectWithTag("Cards").transform;
		_birdCount = new Dictionary<string,int>();
		_firstSpawn=new Dictionary<string,int>();
	}

	void Start(){
		StartCoroutine(SpawnBirdsNextFrame());
	}

	//cuz have to wait a frame for trees, because of this editor compatibility thing
	IEnumerator SpawnBirdsNextFrame(){
		yield return null;
		SpawnBirds();
	}

	void SpawnBirds(){
		MTree[] trees = transform.parent.Find("Planters").GetComponentsInChildren<MTree>();
		Random.InitState(_seed);
		_terrain = transform.parent.GetComponentInChildren<Terrain>();
		TerrainData td = _terrain.terrainData;
		float [,,] alphaMaps = td.GetAlphamaps(0,0,td.alphamapWidth,td.alphamapHeight);
		int counter=0;
		foreach(SpawnGroup sg in _spawnGroup){
			for(int i=0; i<sg._pairSpawn*2; i++){
				//#temp - we assume right now that we spawn birds on ground because we are starting with robins
				//but other birds should be spawned in trees, some in the water, and some in the air
				int iters=0;
				bool spotFound=false;
				while(iters<1000&&!spotFound){
					if(sg._spawnTerrainLayer>=0){
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
							Sing curSing = bird.GetComponentInChildren<Sing>();
							if(i%2==0&&sg._altMat!=null)
								bird.GetComponentInChildren<SkinnedMeshRenderer>().material=sg._altMat;
							GroundForager gf = bird.GetComponent<GroundForager>();
							if(gf!=null)
								gf._terrainLayer=sg._spawnTerrainLayer;
							if(i%2!=0){
								curSing.SetMale();
								gf.enabled=false;
								MTree tree = trees[Random.Range(0,trees.Length)];
								Vector3 perch = tree.GetRandomPerch();
								bird.position=perch;
								TreeBehaviour tb = bird.GetComponent<TreeBehaviour>();
								tb.enabled=true;
							}
							if(i%2==1){
								Sing prevBird=transform.GetChild(counter-1).GetComponentInChildren<Sing>();
								prevBird.SetMate(curSing);
								curSing.SetMate(prevBird);
							}
							if(i==1)
							{
								_firstSpawn.Add(sg._name,counter);
							}
							curSing.SetName(sg._name);
							counter++;
						}
					}
					else{
						Debug.Log("Not really expecting to get here yet...");
						//#todo maybe
						//other spawning stuff like waterfowl
						/*
						//spawn in tree
						MTree tree = trees[Random.Range(0,trees.Length)];
						Vector3 perch = tree.GetRandomPerch();
						//Debug.Log("perching at: "+perch);
						spotFound=true;
						Transform bird = Instantiate(sg._birdPrefab,perch,Quaternion.identity,transform);
						if(i%2==0&&sg._altMat!=null)
							bird.GetComponentInChildren<SkinnedMeshRenderer>().material=sg._altMat;
							bird.GetChild(0).GetComponent<Renderer>().material=sg._altMat;
							*/
					}
					iters++;
				}
			}
			if(!_birdCount.ContainsKey(sg._name))
				_birdCount.Add(sg._name,0);
		}
	}

	public void ShowList(){
		if(_activeList==this)
			return;
		_activeList=this;
		//clear old cards
		//#todo
		//maybe instead of destroying, we just set to not active
		//and then when we spawn instead of instantiating always, we can also just check for de-activated
		for(int i=_cards.childCount-1;i>=0;i--)
			Destroy(_cards.GetChild(i).gameObject);
		//draw new cards
		int counter=0;
		foreach(SpawnGroup sg in _spawnGroup)
		{
			Transform card = Instantiate(_cardPrefab,_cards);
			//find relevant bird - perhaps based on sg._name
			Transform bird = transform.GetChild(_firstSpawn[sg._name]);
			card.name=sg._name;
			//set card's render texture - this may have to be a coroutine - which may be fine because we would like to animate
			//this thing anyway
			StartCoroutine(SetupCard(card,bird,counter));
			counter++;
		}
	}

	IEnumerator SetupCard(Transform card, Transform bird, int index){
		Debug.Log("Setting up: "+bird.name);
		RawImage cardImage = card.Find("Rt").GetComponent<RawImage>();
		Camera birdCam = bird.Find("Camera").GetComponent<Camera>();
		CanvasGroup cg = card.GetComponent<CanvasGroup>();
		Renderer smr = bird.GetComponentInChildren<SkinnedMeshRenderer>();
		smr.receiveShadows=false;
		cg.alpha=0f;
		//setup camera
		birdCam.enabled=true;
		Debug.Log("tex name: "+birdCam.targetTexture.name);
		cardImage.texture=birdCam.targetTexture;
		yield return null;
		birdCam.enabled=false;
		smr.receiveShadows=true;
		//wait
		yield return new WaitForSeconds(1f*index);
		//animate cg
		float timer=0f;
		while(timer<1f){
			timer+=Time.deltaTime;
			cg.alpha=timer;
			yield return null;
		}
		cg.alpha=1f;
	}

	public void SongSuccess(string n, int songIndex){
		foreach(Transform t in _cards){
			if(t.name==n){
				Transform soundsList=t.Find("SoundsList");
				RawImage songCheckBox=soundsList.GetChild(songIndex).GetChild(0).GetComponent<RawImage>();
				StartCoroutine(CheckSongBox(songCheckBox));
				//here maybe we check for the full card?
				//also what happens if we switch islands... The card loses all its progress... rip
			}
		}
	}

	float pulseDel=0.1f;
	IEnumerator CheckSongBox(RawImage checkBox){
		for(int i=0;i<10;i++){
			checkBox.color=Color.green;
			yield return new WaitForSeconds(pulseDel);
			checkBox.color=Color.white;
			yield return new WaitForSeconds(pulseDel);
		}
		checkBox.color=Color.green;
	}
	IEnumerator CheckSongBox(Image checkBox){
		for(int i=0;i<10;i++){
			checkBox.color=Color.green;
			yield return new WaitForSeconds(pulseDel);
			checkBox.color=Color.white;
			yield return new WaitForSeconds(pulseDel);
		}
		checkBox.color=Color.green;
	}

	public void IncBirdCount(string n,bool m){
		foreach(Transform t in _cards){
			if(t.name==n){
				Transform rt = t.Find("Rt");
				Text birdCount=rt.GetChild(0).GetChild(0).GetComponent<Text>();
				StartCoroutine(IncBirdCount(birdCount,n));
				if(m){
					Image male = rt.Find("Male").GetChild(0).GetComponent<Image>();
					if(male.color!=Color.green){
						StartCoroutine(CheckSongBox(male));
					}
				}
				else
				{
					Image female = rt.Find("Female").GetChild(0).GetComponent<Image>();
					if(female.color!=Color.green){
						StartCoroutine(CheckSongBox(female));
					}
				}
			}
		}
	}

	IEnumerator IncBirdCount(Text t, string n){
		_birdCount[n]++;
		t.text=_birdCount[n].ToString("0");
		for(int i=0;i<10;i++){
			t.color=Color.green;
			yield return new WaitForSeconds(pulseDel);
			t.color=Color.white;
			yield return new WaitForSeconds(pulseDel);
		}
	}
}
