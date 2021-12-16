using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class StarRating : MonoBehaviour
{
	public static int _confirmed;
	public static int _tmp;
	int _myVal;
	RawImage _fillImage;
	public Color _tmpColor;
	public AudioClip _mouseOver;
	public AudioClip _mouseClick;
	AudioSource _audio;
    // Start is called before the first frame update
    void Start()
    {
		_myVal=transform.GetSiblingIndex();
		_fillImage=transform.GetChild(0).GetComponent<RawImage>();
		_audio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
    }

	void OnMouseEnter(){
		_tmp=_myVal+1;
		//Debug.Log("temp stars: "+_tmp);
		UpdateStars();
		_audio.clip=_mouseOver;
		_audio.Play();
	}

	void OnMouseUpAsButton(){
		_confirmed=_tmp;
		_tmp=0;
		_audio.clip=_mouseClick;
		_audio.Play();
		UpdateStars();
	}

	void OnMouseExit(){
		_tmp=0;
		UpdateStars();
	}

	void UpdateStars(){
		for(int i=0;i<transform.parent.childCount;i++){
			if(_tmp!=0)
				transform.parent.GetChild(i).GetComponent<StarRating>().FillTemp(i<_tmp);
			else
				transform.parent.GetChild(i).GetComponent<StarRating>().FillTemp(i<_confirmed);
		}
	}

	public void FillTemp(bool filled){
		_fillImage.enabled=filled;
		//_fillImage.color=_tmpColor;
	}

	public void SubmitRating(){
		if(!Directory.Exists(Application.streamingAssetsPath))
			Directory.CreateDirectory(Application.streamingAssetsPath);
		string ratingsFolder=Application.streamingAssetsPath+"/ratings";
		if(!Directory.Exists(ratingsFolder))
			Directory.CreateDirectory(ratingsFolder);
		string dateTime=System.DateTime.Now.ToString("MMM-dd-yy_HH-mm-ss")+".rate";
		string ratingPath = ratingsFolder+"/"+dateTime;
		string data=_confirmed.ToString("0");
		File.WriteAllText(ratingPath,data);
		StartCoroutine(ExitAfterClick());
	}

	IEnumerator ExitAfterClick(){
		yield return new WaitForSeconds(_mouseClick.length);
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}
}
