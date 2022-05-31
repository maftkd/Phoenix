using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostProcess : MonoBehaviour
{
	public Material _outlineMat;
	public bool _outlinesEnabled;
    // Start is called before the first frame update
    void Start()
    {
		GetComponent<Camera>().depthTextureMode=DepthTextureMode.DepthNormals;
        
    }

	void OnRenderImage(RenderTexture src, RenderTexture tgt){
		if(_outlinesEnabled)
			Graphics.Blit(src,tgt,_outlineMat);
		else
			Graphics.Blit(src,tgt);
	}
}
