using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Footstep : MonoBehaviour
{
	public AudioClip [] _clips;
	public AudioClip [] _clipsAlt;
	public AudioClip [] _clipsAlt2;
	[Range(0f,1f)]
	public float _volume;
	[Range(0f,1f)]
	public float _altVolMult;
	[Range(0f,1f)]
	public float _alt2VolMult;
	float _walkSpeed;
	float _runSpeed;
	public UnityEvent _onPlay;
	Terrain _terrain;
	float[,,] _alphaMaps;
	TerrainData _terrainData;

	void Awake(){
		_terrain=GetComponent<Terrain>();
		if(_terrain!=null){
			_terrainData = _terrain.terrainData;
			_alphaMaps = _terrainData.GetAlphamaps(0,0,_terrainData.alphamapWidth,_terrainData.alphamapHeight);
		}
	}
    // Start is called before the first frame update
    void Start()
    {
    }

	public void AssignSynthClip(Synthesizer synth){
		_clips = new AudioClip[1];
		_clips[0]=synth._myClip;
	}

    // Update is called once per frame
    void Update()
    {
        
    }

	public void Sound(Vector3 pos,float volume=-1f,float pitch=-1f){
		if(_clips.Length==0)
			return;
		pitch = pitch<0? 1f : pitch;
		if(_terrain==null){
			Sfx.PlayOneShot3D(_clips[Random.Range(0,_clips.Length)],pos,pitch,volume<0? _volume : volume);
		}
		else{
			int terrainLayer=GetTerrainTextureIndex(pos);
			float vol=volume<0? _volume : volume;
			switch(terrainLayer){
				case 0://sand
					Sfx.PlayOneShot3D(_clipsAlt[Random.Range(0,_clipsAlt.Length)],pos,pitch,vol*_altVolMult);
					break;
				case 1://pebbles
					Sfx.PlayOneShot3D(_clipsAlt2[Random.Range(0,_clipsAlt2.Length)],pos,pitch,vol*_alt2VolMult);
					break;
				case 2://grass
					Sfx.PlayOneShot3D(_clips[Random.Range(0,_clips.Length)],pos,pitch,vol);
					break;
				default:
					break;
			}
		}
	}
	
	//#temp - duplicate method from Fly.cs
	int GetTerrainTextureIndex(Vector3 pos){
		//convert world coord to terrain space
		float xWorld=pos.x;
		float zWorld=pos.z;
		Vector3 local=pos-_terrain.transform.position;
		float xFrac=local.x/_terrainData.size.x;
		float zFrac=local.z/_terrainData.size.z;
		if(xFrac<0||xFrac>=1)
			return 0;
		if(zFrac<0||zFrac>=1)
			return 0;
		int xCoord=Mathf.FloorToInt(zFrac*_terrainData.alphamapHeight);
		int yCoord=Mathf.FloorToInt(xFrac*_terrainData.alphamapWidth);
		float max=0;
		int layer=0;
		for(int i=0;i<_terrainData.alphamapLayers;i++){
			float v = _alphaMaps[xCoord,yCoord,i];
			if(v>max)
			{
				max=v;
				layer=i;
			}
		}
		return layer;
	}
}
