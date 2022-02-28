using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RidgeLine : MonoBehaviour
{

	public int _texRes;
	public Texture2D _ridgeTex;
	public int _seed;
	public float _noiseScale;
	public float _amp;
	public int _octaves;
	public float _lacunarity;
	public float _gain;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	void OnValidate(){
		GenerateRidgeTex();
	}

	[ContextMenu("Generate Ridge Tex")]
	public void GenerateRidgeTex(){
		Debug.Log("Generating Ridge");
		_ridgeTex = new Texture2D(_texRes,_texRes);
		Random.InitState(_seed);
		float offset=Random.value;

		//draw base line
		int baseX=_texRes/2;
		int distortX=0;
		float[] line = new float[_texRes];
		float scale=_noiseScale;
		float amp=_amp;
		for(int i=0; i<_texRes;i++){
			float t01=i/(float)(_texRes-1);
			line[i]=Mathf.PerlinNoise(0,t01*scale+offset)-0.5f;
			line[i]*=amp;
		}
		for(int o=0; o<_octaves; o++){
			scale*=_lacunarity;
			amp*=_gain;
			for(int i=0; i<_texRes;i++){
				float t01=i/(float)(_texRes-1);
				float newNoise = Mathf.PerlinNoise(0,t01*scale+offset)-0.5f;
				line[i]+=newNoise*amp;
			}
		}
		for(int y=0; y<_texRes; y++){
			distortX=baseX+Mathf.FloorToInt(line[y]*_texRes);
			if(distortX<0)
				distortX=0;
			else if(distortX>=_texRes)
				distortX=_texRes-1;
			for(int x=0; x<_texRes; x++){
				if(x==distortX)
					_ridgeTex.SetPixel(x,y,Color.white);
				else
					_ridgeTex.SetPixel(x,y,Color.black);
			}
		}

		_ridgeTex.Apply();

		GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MainTex",_ridgeTex);
	}
}
