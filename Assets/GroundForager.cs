using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GroundForager : MonoBehaviour
{
	public int _state;
	float _theta;
	public Vector2 _dTheta;
	float _deltaTheta;
	public float _thetaChangeChance;
	Vector3 _hopDir;
	public Vector2 _listenDur;
	float _listenTime;
	float _listenTimer;
	Animator _anim;
	public float _peckDur;
	Text _debugText;
	public float _hopChance;
	public Vector2 _hopDist;
	public Vector2 _hopDur;
	public Vector2 _hopHeight;
	public float _returnToTreeChance;
	Terrain _terrain;
	Bird _player;
	public float _fleeRadius;
	TreeBehaviour _treeB;
	public int _terrainLayer;
	NPB _npb;

	void Awake(){
		_anim=GetComponent<Animator>();
		_debugText=transform.GetComponentInChildren<Text>();
		_theta = Random.value*Mathf.PI*2f;
		_hopDir=new Vector3(Mathf.Cos(_theta),0,Mathf.Sin(_theta));
		transform.forward=_hopDir;
		_terrain=transform.parent.GetComponent<BirdSpawner>()._terrain;
		_player=GameManager._player;
		_treeB=GetComponent<TreeBehaviour>();
		_npb=GetComponent<NPB>();
	}

	void OnEnable(){
		_anim.SetBool("hop",false);
		_anim.SetBool("peck",false);
		_state=0;
	}

    // Start is called before the first frame update
    void Start()
    {
		Listen();
		_deltaTheta=Random.Range(_dTheta.x,_dTheta.y);
		if(Random.value<0.5f)
			_deltaTheta*=-1f;
    }

    // Update is called once per frame
    void Update()
    {
		switch(_state){
			case 0:
				_listenTimer+=Time.deltaTime;
				if(_listenTimer>=_listenTime){
					if(!_npb._scanned)
						StartCoroutine(GetWorm());
					_listenTimer=0;
				}
				else{
				}
				break;
			case 1://pecking
				break;
			default:
				break;
		}
		//if near player
		//	flee to tree
		if((_player.transform.position-transform.position).sqrMagnitude<_fleeRadius*_fleeRadius){
			StopAllCoroutines();
			_treeB.enabled=true;
			_treeB.ScareIntoTree();
			//tell sing component to chill for a bit
			//sing.Alarm()
			enabled=false;
		}
		//_debugText.text="State: "+_state.ToString("0");
    }

	void Listen(){
		_state=0;
		_listenTime=Random.Range(_listenDur.x,_listenDur.y);
		_listenTimer=0f;
	}

	IEnumerator GetWorm(){
		_state=1;
		_anim.SetBool("peck",true);
		yield return new WaitForSeconds(_peckDur);
		if(Random.value<_hopChance){
			StartCoroutine(Hop());
			_anim.SetBool("peck",false);
		}
		else
		{
			_anim.SetBool("peck",false);
			if(Random.value<_returnToTreeChance){
				_treeB.enabled=true;
				_treeB.ScareIntoTree();
				enabled=false;
			}
			else
				Listen();
		}
	}

	IEnumerator Hop(){
		_state=2;
		if(Random.value<_thetaChangeChance){
			_deltaTheta*=-1f;
		}
		_deltaTheta=Mathf.Sign(_deltaTheta)*Random.Range(_dTheta.x,_dTheta.y);
		_theta+=_deltaTheta;
		_hopDir=new Vector3(Mathf.Cos(_theta),0,Mathf.Sin(_theta));
		transform.forward=_hopDir;
		_anim.SetBool("hop",true);
		float dist=Random.Range(_hopDist.x,_hopDist.y);
		Vector3 newPos=transform.position+_hopDir*dist;
		float y = _terrain.SampleHeight(newPos);
		newPos.y=y;
		if(PositionHasGrass(newPos)){
			Vector3 startPos=transform.position;
			Vector3 endPos=newPos;
			float timer=0;
			float dur=Random.Range(_hopDur.x,_hopDur.y);
			float height=Random.Range(_hopHeight.x,_hopHeight.y);
			while(timer<dur){
				timer+=Time.deltaTime;
				float frac=timer/dur;
				Vector3 pos=Vector3.Lerp(startPos,endPos,frac);
				float yOffset=(-4*Mathf.Pow(frac-0.5f,2)+1)*height;
				pos.y+=yOffset;
				transform.position=pos;
				yield return null;
			}
			transform.position=endPos;
			_anim.SetBool("hop",false);
			Listen();
		}
		else{
			_treeB.enabled=true;
			_treeB.ScareIntoTree();
			//tell sing component to chill for a bit
			//sing.Alarm()
			enabled=false;
			yield return null;
		}
	}

	public Vector3 GetRandomSpotOnGround(){
		TerrainData td = _terrain.terrainData;
		float [,,] alphaMaps = td.GetAlphamaps(0,0,td.alphamapWidth,td.alphamapHeight);
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
			if(alphaMaps[alphaMapX,alphaMapZ,_terrainLayer]>0.5f){
				Vector3 worldPos=new Vector3(worldX,worldY,worldZ);
				if((worldPos-_player.transform.position).sqrMagnitude>_fleeRadius*_fleeRadius){
					spotFound=true;
					return worldPos;
				}
			}
			iters++;
		}
		return Vector3.zero;
	}

	bool PositionHasGrass(Vector3 foo){
		TerrainData td = _terrain.terrainData;
		float [,,] alphaMaps = td.GetAlphamaps(0,0,td.alphamapWidth,td.alphamapHeight);
		float worldX=foo.x;
		float worldZ=foo.z;
		float xFrac=(worldX-_terrain.transform.position.x)/td.size.x;
		float zFrac=(worldZ-_terrain.transform.position.z)/td.size.z;
		int alphaMapZ=Mathf.RoundToInt(xFrac*(td.alphamapHeight-1));
		int alphaMapX=Mathf.RoundToInt(zFrac*(td.alphamapWidth-1));
		return alphaMaps[alphaMapX,alphaMapZ,_terrainLayer]>0.5f;
	}
}
