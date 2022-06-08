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
		float pitch=1f;
		AudioSource audio = Sfx.PlayOneShot3D(_song._clip,transform.position,pitch,"Birds");

		//setup notes
		float dur=_song._clip.length/pitch;
		for(int i=0;i<_song._notes.Length; i++){
			float curNoteStart=_song._notes[i]._startT*dur;
			float curNoteEnd=_song._notes[i]._endT*dur;
			int pitchId=_song._notes[i]._pitchId;
			float y=1.14f+curNoteStart*_yPerSec;
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
		}
		float timer=0f;
		float curTime=0;
		for(int i=0;i<_song._notes.Length; i++){
			float curNoteStart=_song._notes[i]._startT*dur;
			float curNoteEnd=_song._notes[i]._endT*dur;
			int pitchId=_song._notes[i]._pitchId;
			float delta=curNoteStart-curTime;
			yield return new WaitForSeconds(delta);
			//note start
			curTime+=delta;
			//_anim.SetFloat("pitch",0.333f);
			if(pitchId==0){
				//_low.Play();
				Debug.Log("L");
			}
			else if(pitchId==1){
				//_mid.Play();
				Debug.Log("M");
			}
			else{
				//_high.Play();
				Debug.Log("H");
			}
			delta=curNoteEnd-curNoteStart;
			yield return new WaitForSeconds(delta);
			//note end
			curTime+=delta;
			//_anim.SetFloat("pitch",0f);
			if(pitchId==0){
				//_low.Stop();
			}
			else if(pitchId==1){
				//_mid.Stop();
			}
			else{
				//_high.Stop();
			}
		}
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
