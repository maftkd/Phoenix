using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WayFinder : MonoBehaviour
{
	Transform _mate;
	Transform _player;
	Camera _cam;
	public float _vertOffset;
	RawImage _image;
	public float _alphaThresh;
	Color _opaque;
	Color _transparent;
	public Texture2D _wholeNote;
	public Texture2D _quarterRest;

	void Awake(){
		_mate=GameManager._mate;
		_player=GameManager._player.transform;
		_cam=GameManager._mCam.GetComponent<Camera>();
		_image=GetComponent<RawImage>();
		_opaque=Color.white;
		_transparent=Color.black;
		_transparent.a=0.5f;
		//_image.texture=_quarterRest;
		_image.texture=_wholeNote;
	}
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		Vector3 screenPoint=_cam.WorldToScreenPoint(_mate.position+Vector3.up*_vertOffset);
		Vector3 screenPointPlayer=_cam.WorldToScreenPoint(_player.position+Vector3.up*0.15f);
		if(screenPoint.z>0)
		{
			transform.position=screenPoint;
			screenPoint.z=0;
			screenPointPlayer.z=0;
			if((screenPoint-screenPointPlayer).sqrMagnitude<_alphaThresh*_alphaThresh*Screen.width*Screen.width){
				_image.color=_transparent;
			}
			else
				_image.color=_opaque;
		}
    }

	public void ChangeColor(Color c){
		_transparent.r=c.r;
		_transparent.g=c.g;
		_transparent.b=c.b;
		_opaque.r=c.r;
		_opaque.g=c.g;
		_opaque.b=c.b;
	}
}
