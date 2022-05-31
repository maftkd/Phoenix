using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGen : MonoBehaviour
{
	public RenderTexture _depth;
	public Material _mapMat;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void GenerateMap(){
		Camera cam = GetComponent<Camera>();
		cam.depthTextureMode=DepthTextureMode.Depth;
		//cam.SetTargetBuffers(_color.colorBuffer,_depth.depthBuffer);
		cam.Render();
		//_mapMat.SetTexture("_DepthTex",
		_mapMat.SetFloat("_MaxDepth",transform.position.y-5f);
		Graphics.Blit(cam.targetTexture,_depth,_mapMat);
		Debug.Log("Just generated map?");
	}
}
