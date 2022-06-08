using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Sing : MonoBehaviour
{
	[System.Serializable]
	public struct Note{
		public float _startT;
		public float _endT;
		public int _pitchId;
	}

	[System.Serializable]
	public struct BirdSong{
		public AudioClip _clip;
		public string _fileName;
		[HideInInspector]
		public Note[] _notes;
		[HideInInspector]
		public float _length;
	}

	public BirdSong _song;
	Transform _low;
	Transform _mid;
	Transform _high;
	Transform _bar;
	public float _yPerSec;

	void Awake(){
		DeserializeSong();
		Debug.Log(_song._notes.Length);
		_low=transform.Find("Low");
		_mid=transform.Find("Mid");
		_high=transform.Find("High");
		_low.gameObject.SetActive(false);
		_mid.gameObject.SetActive(false);
		_high.gameObject.SetActive(false);
		_bar=transform.Find("Bar");
	}
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if(Input.GetKeyDown(KeyCode.Space)){
			StartCoroutine(PlaySong());
		}
    }

	IEnumerator PlaySong(){
		//setup notes
		Transform[] notes = new Transform[_song._notes.Length];
		float dur=_song._clip.length;
		float startHeight=_low.position.y;
		for(int i=0;i<_song._notes.Length; i++){
			float curNoteStart=_song._notes[i]._startT*dur;
			float curNoteEnd=_song._notes[i]._endT*dur;
			int pitchId=_song._notes[i]._pitchId;
			float y=startHeight+curNoteStart*_yPerSec;
			float delta=curNoteEnd-curNoteStart;
			Transform note;
			if(pitchId==0)
				note=Instantiate(_low,transform);
			else if(pitchId==1)
				note=Instantiate(_mid,transform);
			else
				note=Instantiate(_high,transform);
			note.gameObject.SetActive(true);
			Vector3 pos=note.position;
			pos.y=y;
			note.position=pos;
			LineRenderer line=note.GetChild(0).GetComponent<LineRenderer>();
			line.SetPosition(1,Vector3.up*delta*_yPerSec);
			notes[i]=note;
		}

		float distToBar=startHeight-_bar.position.y;
		float delay=distToBar/_yPerSec;
		StartCoroutine(PlayAfterDelay(delay));
		float overallDur=dur+delay;
		float timer=0f;
		while(timer<overallDur){
			timer+=Time.deltaTime;
			foreach(Transform n in notes){
				n.position+=Vector3.down*Time.deltaTime*_yPerSec;
			}
			yield return null;
		}

	}

	IEnumerator PlayAfterDelay(float d){
		yield return new WaitForSeconds(d);
		Sfx.PlayOneShot2D(_song._clip);
	}

	public void DeserializeSong(){
		string path = Application.streamingAssetsPath+"/Songs/"+_song._fileName+".song";
		byte[] data = File.ReadAllBytes(path);
		int numNotes=System.BitConverter.ToInt32(data,0);
		_song._length = System.BitConverter.ToSingle(data,4);
		//Debug.Log("Loaded: "+ numNotes+ " notes");
		Note[] notes = new Note[numNotes];
		for(int j=0; j<numNotes; j++){
			Note n;
			n._startT=System.BitConverter.ToSingle(data,j*12+8);
			n._endT=System.BitConverter.ToSingle(data,j*12+12);
			n._pitchId=System.BitConverter.ToInt32(data,j*12+16);
			notes[j]=n;
		}
		_song._notes=notes;
	}
}
