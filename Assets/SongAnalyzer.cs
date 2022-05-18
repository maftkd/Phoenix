using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SongAnalyzer : MonoBehaviour
{
	public int _plotMod;
	public Vector3 _plotDim;
	List<Vector2> _peaks;
	int _samples;
	float _length;
	List<float> _freqs;
	public Vector2 _cutoffs;

	public bool _plotSong;
	public bool _playSong;
	public bool _calcFreq;
	public bool _serializeSong;

	void OnValidate(){
		if(_plotSong){
			_plotSong=false;
			PlotSong();
		}
		if(_playSong){
			_playSong=false;
			GetComponent<AudioSource>().Play();
		}
		if(_calcFreq){
			_calcFreq=false;
			CalcFreq();
		}
		if(_serializeSong){
			_serializeSong=false;
			SerializeSong();
		}
	}
    // Start is called before the first frame update
    void Start()
    {
		gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	void PlotSong(){
		AudioClip clip = GetComponent<AudioSource>().clip;
		_samples = clip.samples;
		_length=clip.length;
		float[] data = new float[_samples];
		clip.GetData(data,0);
		_peaks = new List<Vector2>();
		float val=0;
		bool goingUp=true;
		for(int i=0; i<_samples; i++){
			float cur = data[i];
			if(goingUp&&cur<val){
				goingUp=false;
				_peaks.Add(new Vector2(i,cur));
			}
			else if(!goingUp&&cur>val){
				goingUp=true;
			}
			val=cur;
		}
	}

	void CalcFreq(){
		if(_peaks==null||_samples==0||_length==0){
			Debug.Log("Cannot calc frequency. Song must be plotted first");
			return;
		}
		_freqs = new List<float>();
		foreach(Transform t in transform){
			float leftT=(t.localPosition.x-t.localScale.x*0.5f)/_plotDim.x;
			float rightT=(t.localPosition.x+t.localScale.x*0.5f)/_plotDim.x;
			if(leftT<0||rightT>=1f){
				Debug.Log("Cannot calc frqeuency. Transform: "+t.name+" does not fit within the plot bounds");
				return;
			}
			float diffs=0;
			int points=0;
			Vector2 prev = _peaks[0];
			for(int i=1; i<_peaks.Count; i++){
				Vector2 p = _peaks[i];
				float t01=p.x/_samples;
				if(t01>leftT&&t01<rightT){
					diffs+=(p.x-prev.x);
					points++;
				}
				prev=p;
			}
			float avgDiff=diffs/points;
			float sampleRate=_samples/_length;
			float avgDiffInSeconds=avgDiff/sampleRate;
			float avgFreq=1f/avgDiffInSeconds;
			_freqs.Add(avgFreq);
		}
	}

	void SerializeSong(){
		if(_freqs==null ||_freqs.Count==0){
			Debug.Log("Cannot serialize song. Please generate frequencies first.");
			return;
		}
		byte[] data = new byte[transform.childCount*12+8];
		int numNotes=transform.childCount;
		byte[] nNotes = System.BitConverter.GetBytes(numNotes);
		System.Array.Copy(nNotes,0,data,0,4);
		byte[] songLength = System.BitConverter.GetBytes(_length);
		System.Array.Copy(songLength,0,data,4,4);
		for(int i=0; i<transform.childCount; i++){
			Transform t = transform.GetChild(i);
			float leftT=(t.localPosition.x-t.localScale.x*0.5f)/_plotDim.x;
			float rightT=(t.localPosition.x+t.localScale.x*0.5f)/_plotDim.x;
			if(leftT<0||rightT>=1f){
				Debug.Log("Cannot calc frqeuency. Transform: "+t.name+" does not fit within the plot bounds");
				return;
			}
			Debug.Log("Start: "+leftT);
			Debug.Log("End: "+rightT);
			Debug.Log("Freq: "+_freqs[i]);
			int pitchId=0;
			if(_freqs[i]<_cutoffs.x)
				pitchId=0;//low
			else if(_freqs[i]>_cutoffs.x&&_freqs[i]<_cutoffs.y)
				pitchId=1;//mid
			else
				pitchId=2;//high
			byte[] startT=System.BitConverter.GetBytes(leftT);
			System.Array.Copy(startT,0,data,i*12+8,4);
			byte[] endT=System.BitConverter.GetBytes(rightT);
			System.Array.Copy(endT,0,data,i*12+12,4);
			byte[] pId = System.BitConverter.GetBytes(pitchId);
			System.Array.Copy(pId,0,data,i*12+16,4);
			Debug.Log("Serializing note: "+i+", start: "+leftT+", end: "+rightT);
		}
		string path=Application.streamingAssetsPath+"/Songs/"+name+".song";
		Debug.Log("song path: "+path);
		File.WriteAllBytes(path,data);
	}

	void OnDrawGizmos(){
		Gizmos.DrawSphere(transform.position,0.5f);
		if(_peaks!=null){
			for(int i=0;i<_peaks.Count; i++){
				if(i%_plotMod==0){
					Vector2 p = _peaks[i];
					float t01=p.x/_samples;
					Vector3 point = transform.position+transform.right*_plotDim.x*t01;
					point.y+=p.y*_plotDim.y;
					Gizmos.DrawSphere(point,_plotDim.z);
				}
			}
		}
		if(_freqs!=null){
			Handles.BeginGUI();
			GUIStyle style = new GUIStyle();
			for(int i=0;i<_freqs.Count; i++){
				if(_freqs[i]<_cutoffs.x)
					style.normal.textColor = Color.blue;
				else if(_freqs[i]>_cutoffs.x&&_freqs[i]<_cutoffs.y)
					style.normal.textColor = Color.yellow;
				else
					style.normal.textColor = Color.red;

				Transform t =transform.GetChild(i);
				Handles.Label(t.position+Vector3.up*t.localScale.y*0.6f,_freqs[i].ToString("0")+"\nHz",style);
			}
			Handles.EndGUI();
		}
	}
}
