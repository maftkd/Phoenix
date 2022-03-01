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
	public float _maxDist01;
	public bool _autoGen;
	public bool _hideRidgeMap;
	public bool _generateGradient;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	void OnValidate(){
		if(!_generateGradient&&_autoGen){
			GenerateRidge();
		}
		if(_generateGradient)
		{
			GenerateRidge(true);
			_generateGradient=false;
		}
		GetComponent<MeshRenderer>().enabled=!_hideRidgeMap;
	}

	public void GenerateRidge(bool genGrad=false){
		Debug.Log("Generating Ridge");
		_ridgeTex = new Texture2D(_texRes,_texRes);
		Random.InitState(_seed);
		float offset=Random.value;

		//draw base line
		int baseX=_texRes/2;
		float[] line = new float[_texRes];
		float scale=_noiseScale;
		float amp=_amp;
		for(int i=0; i<_texRes;i++){
			float t01=i/(float)(_texRes-1);
			line[i]=Mathf.PerlinNoise(0,(t01+offset)*scale)-0.5f;
			line[i]*=amp;
		}
		for(int o=0; o<_octaves; o++){
			scale*=_lacunarity;
			amp*=_gain;
			for(int i=0; i<_texRes;i++){
				float t01=i/(float)(_texRes-1);
				float newNoise = Mathf.PerlinNoise(0,(t01+offset)*scale)-0.5f;
				line[i]+=newNoise*amp;
			}
		}
		for(int y=0; y<_texRes; y++){
			line[y]=baseX+Mathf.FloorToInt(line[y]*_texRes);
			if(line[y]<0)
				line[y]=0;
			else if(line[y]>=_texRes)
				line[y]=_texRes-1;
			for(int x=0; x<_texRes; x++){
				if(x==line[y])
					_ridgeTex.SetPixel(x,y,Color.white);
				else
					_ridgeTex.SetPixel(x,y,Color.black);
			}
		}

		if(genGrad){
			//now that we have our line - we can calc min distance to the line
			for(int y=0; y<_texRes; y++){
				for(int x=0;x<_texRes;x++){
					Vector2 pixel=new Vector2(x,y);
					float minSqrDst=1000000;
					int minIndex=-1;
					for(int i=0;i<_texRes;i++){
						Vector2 lPos=new Vector2(line[i],i);
						float sqrDst=(lPos-pixel).sqrMagnitude;
						if(sqrDst<minSqrDst){
							minSqrDst=sqrDst;
							minIndex=i;
						}
					}
					float minDst=(new Vector2(line[minIndex],minIndex)-pixel).magnitude;
					float dst01=minDst/_texRes;
					dst01=Mathf.InverseLerp(_maxDist01,0,dst01);
					dst01*=dst01;
					//dst01=Mathf.SmoothStep(0,1,dst01);
					//dst01=Mathf.SmoothStep(_maxDist01,0,dst01);
					_ridgeTex.SetPixel(x,y,Color.white*dst01);
				}
			}
		}

		_ridgeTex.Apply();

		GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MainTex",_ridgeTex);
	}

	public float GetHeight(Vector3 worldPos){
		Vector3 center=transform.position;
		Vector3 cornerOffset=transform.localScale*0.5f;
		Vector3 minWorld=center-cornerOffset;
		Vector3 maxWorld=center+cornerOffset;
		float xPos=Mathf.InverseLerp(minWorld.x,maxWorld.x,worldPos.x);
		float yPos=Mathf.InverseLerp(minWorld.z,maxWorld.z,worldPos.z);
		int xPix=Mathf.FloorToInt(xPos*_texRes);
		int yPix=Mathf.FloorToInt(yPos*_texRes);
		return _ridgeTex.GetPixel(xPix,yPix).r;
	}
}
