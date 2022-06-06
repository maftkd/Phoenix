using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class Sing : MonoBehaviour
{
	MInput _mIn;
	[HideInInspector]
	public string _name;
	Animator _anim;
	public int _state;
	Whistle _low;
	Whistle _mid;
	Whistle _high;
	AudioSource _audio;

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
		public string _mnemonic;
		[HideInInspector]
		public string _species;
		[HideInInspector]
		public Note[] _notes;
		[HideInInspector]
		public float _length;
		[HideInInspector]
		public NPB _female;
		[HideInInspector]
		public NPB _male;
	}

	[Header("Npb stuff")]
	public NPB _npb;
	public BirdSong [] _songs;
	public BirdSong _playSong;
	public AudioClip [] _calls;
	public BirdSong [] _alarms;
	public Vector2 _responseRange;
	public float _responseChance;
	public float _accuracyThreshold;
	public float _earShot;
	IEnumerator _cancelRoutine;
	public UnityEvent _onPatternSuccess;
	public bool _male;
	public bool _femaleSings;
	public Vector2 _singDelayRange;
	float _singDelay;
	float _singTimer;
	BirdSpawner _birdSpawner;
	public float _switchTreeChance;
	TreeBehaviour _tb;

	Bird _player;
	Transform _cam;

	//public delegate void SingEvent();
	//public event SingEvent _onSing;
	bool _inPattern;
	int _patternNote;
	bool _patternDir=true;//true means next step in pattern is to hear the note. False means next step is for note to stop
	BirdSong _curSong;
	int _curSongIndex;
	int _orderedIndex;
	public Sing _mate;
	Sing _targetBird;
	System.DateTime _lastSongEventTime;

	void Awake(){
		_mIn=GameManager._mIn;
		_low=transform.Find("Low").GetComponent<Whistle>();
		_mid=transform.Find("Mid").GetComponent<Whistle>();
		_high=transform.Find("High").GetComponent<Whistle>();
		
		_anim=transform.parent.GetComponent<Animator>();

		_npb=transform.GetComponentInParent<NPB>();
		_player=GameManager._player;
		_tb=_npb.GetComponent<TreeBehaviour>();
		_birdSpawner=transform.parent.parent.GetComponent<BirdSpawner>();
	}

	void Start(){
		//setup songs - this is done after awake because we need to ensure _male is set
		if(_male||_femaleSings)
		{
			for(int i=0;i<_songs.Length; i++){
				Deserialize(_songs, i);
			}
			for(int i=0;i<_alarms.Length; i++){
				Deserialize(_alarms,i);
			}
		}
		_lastSongEventTime=System.DateTime.Now;
		_singDelay=Random.Range(_singDelayRange.x,_singDelayRange.y);
	}

	public int SingSong(){
		if(_state!=0)
			return 0;
		if(!_male&&!_femaleSings)
			return 0;

		_curSongIndex=Random.Range(0,_songs.Length);
		_curSong=_songs[_curSongIndex];
		StartCoroutine(SingR(_curSong));
		return _curSongIndex;
	}

	IEnumerator SingR(BirdSong bs,bool delay=true){
		if(!_npb._scanned)
			_state=2;//2 = singing
		//wait a sec - play it cool
		if(delay)
			yield return new WaitForSeconds(Random.Range(_responseRange.x,_responseRange.y));

		//setup audio
		float pitch=Random.Range(0.95f,1.05f);
		_audio = Sfx.PlayOneShot3D(bs._clip,transform.position,pitch,"Birds");

		//setup notes
		float dur=bs._clip.length/pitch;
		float curTime=0;
		for(int i=0;i<bs._notes.Length; i++){
			float curNoteStart=bs._notes[i]._startT*dur;
			float curNoteEnd=bs._notes[i]._endT*dur;
			int pitchId=bs._notes[i]._pitchId;
			float delta=curNoteStart-curTime;
			yield return new WaitForSeconds(delta);
			//note start
			curTime+=delta;
			_anim.SetFloat("pitch",0.333f);
			if(pitchId==0){
				_low.Play();
			}
			else if(pitchId==1){
				_mid.Play();
			}
			else{
				_high.Play();
			}
			delta=curNoteEnd-curNoteStart;
			yield return new WaitForSeconds(delta);
			//note end
			curTime+=delta;
			_anim.SetFloat("pitch",0f);
			if(pitchId==0){
				_low.Stop();
			}
			else if(pitchId==1){
				_mid.Stop();
			}
			else{
				_high.Stop();
			}
		}
		if(!_npb._scanned)
			_state=0;//0 = ready to sing

		/*
		if(_femaleSings&&!_male){
			if(Random.value<_responseChance){
				_mate.SingSong();
			}
		}
		if(!_femaleSings){
			//_mate.
			_mate.Reset();
		}
		*/

		if(!_npb._scanned){
			if(Random.value<_switchTreeChance)
			{
				_tb.ScareIntoTree(true);
			}
		}
	}

	public void Call(){
		StartCoroutine(CallR());
	}

	IEnumerator CallR(){
		_state=2;//2 is singing for male / calling for female
		float pitch=Random.Range(0.8f,1f);
		AudioClip call=_calls[Random.Range(0,_calls.Length)];
		Sfx.PlayOneShot3D(call,transform.position,pitch,"Birds");
		float dur=call.length/pitch;
		_anim.SetFloat("pitch",0.666f);
		_mid.Play();
		yield return new WaitForSeconds(dur);
		_anim.SetFloat("pitch",0f);
		_mid.Stop();

		if(Random.value<_responseChance){
			_mate.SingSong();
		}
	}


    // Update is called once per frame
    void Update()
    {
		_singTimer+=Time.deltaTime;
		if(_singTimer>_singDelay){
			_singTimer=0;
			_singDelay=Random.Range(_singDelayRange.x,_singDelayRange.y);
			SingSong();
		}
    }

	void LateUpdate(){
		/*
		if(_npb==null)
			transform.forward=_cam.forward;
			*/
	}

	public void HandleNote(int pitchId, bool singing){
		if((transform.position-_player.transform.position).sqrMagnitude>_earShot*_earShot)
			return;
		if(_curSong._notes==null)//catches cases like alarm and female calls
			return;
		if(_npb==null||!_npb._scanned)
			return;
		//Debug.Log("Heard note: "+pitchId+" dir: "+singing);
		if(_cancelRoutine!=null)
			StopCoroutine(_cancelRoutine);

		Debug.Log("pattern note: "+_patternNote+", "+name);
		if(pitchId==_curSong._notes[_patternNote]._pitchId){
			if(singing==_patternDir){
				System.DateTime time=System.DateTime.Now;
				System.TimeSpan ts = time-_lastSongEventTime;
				float timeDelta=(float)ts.TotalSeconds;
				//pattern start
				if(_patternNote==0&&_patternDir==true){
					NoteHandler(true);
				}
				else{
					//compare timeDelta to required
					//if patternDir = false
					//	check the notes endT-startT to get the idea note duration
					//else if patternDir==true
					//	check the previous notes endT - current note's startT to get note duration
					//if time delta is within range
					//	fire successful pattern note event
					//else
					//	fire pattern broken event
					float perfectTime=0f;
					if(_patternDir==false){
						perfectTime=(_curSong._notes[_patternNote]._endT-_curSong._notes[_patternNote]._startT)*_curSong._length;
					}
					else{
						perfectTime=(_curSong._notes[_patternNote]._startT-_curSong._notes[_patternNote-1]._endT)*_curSong._length;
					}
					if(Mathf.Abs(timeDelta-perfectTime)<_accuracyThreshold){
						NoteHandler(true);
						_cancelRoutine = CancelAfterMaxDelay(perfectTime+_accuracyThreshold);
						StartCoroutine(_cancelRoutine);
					}
					else{
						//Debug.Log("time delta: "+timeDelta);
						//Debug.Log("perfect time: "+perfectTime);
						NoteHandler(false);
					}
				}
				return;
			}
		}
		//got a wrong note
		NoteHandler(false);
	}

	IEnumerator CancelAfterMaxDelay(float maxDelay){
		yield return new WaitForSeconds(maxDelay);
		if(_patternNote==0&&_patternDir==true)
		{
			//already are reset
		}
		else
			NoteHandler(false);
	}

	public void NoteHandler(bool success){
		if(success)
		{
			//handle a successful note
			//Debug.Log("Success!");
			if(_patternNote==_curSong._notes.Length-1&&_patternDir==false){
				//check for last note in pattern
				//Debug.Log("Full pattern success!");
				_onPatternSuccess.Invoke();
				if(_npb!=null)
				{
					_npb.FullPatternSuccess(_curSongIndex);
					_birdSpawner.SongSuccess(_name,_curSongIndex);
					if(_orderedIndex<_songs.Length)
						_orderedIndex++;
				}
				_patternNote=0;
				_patternDir=true;
				if(_cancelRoutine!=null)
					StopCoroutine(_cancelRoutine);
			}
			else{
				if(_patternDir==true)
					_patternDir=false;
				else{
					_patternDir=true;
					_patternNote++;
				}
				_lastSongEventTime=System.DateTime.Now;
			}
		}
		else{
			Debug.Log("Fail!");
			_patternNote=0;
			_patternDir=true;
			_npb.PatternFail();
		}
	}

	public void SetMale(){
		_male=true;
	}

	public void Reset(){
		_low.Stop();
		_mid.Stop();
		_high.Stop();
		_state=0;
		_anim.SetFloat("pitch",0f);
	}

	public void SetTargetBird(Sing target){
		_targetBird=target;
	}

	public void SetMate(Sing mate){
		_mate=mate;
	}

	public void SetName(string n){
		_name=n;
	}

	public void FleeAlarm(){
		StopAllCoroutines();
		if(_audio!=null)
			_audio.Stop();
		_low.Stop();
		_mid.Stop();
		_high.Stop();
		//play sound

		BirdSong alarmSong=_alarms[Random.Range(0,_alarms.Length)];
		StartCoroutine(SingR(alarmSong,false));
		
		//change state
		_state=1;//1 = alarm
	}


	public void Deserialize(BirdSong[] files, int index){
		BirdSong bs=files[index];
		//get data
		string path = Application.streamingAssetsPath+"/Songs/"+bs._fileName+".song";
		byte[] data = File.ReadAllBytes(path);
		int numNotes=System.BitConverter.ToInt32(data,0);
		files[index]._length = System.BitConverter.ToSingle(data,4);
		//Debug.Log("Loaded: "+ numNotes+ " notes");
		Note[] notes = new Note[numNotes];
		for(int j=0; j<numNotes; j++){
			Note n;
			n._startT=System.BitConverter.ToSingle(data,j*12+8);
			n._endT=System.BitConverter.ToSingle(data,j*12+12);
			n._pitchId=System.BitConverter.ToInt32(data,j*12+16);
			notes[j]=n;
		}
		files[index]._notes = notes;
	}

	public void Engage(){
		//stop existing singing
		StopAllCoroutines();
		if(_audio!=null)
			_audio.Stop();
		_low.Stop();
		_mid.Stop();
		_high.Stop();
		//start new singing
		SingSong();
		_state=3;//engaged state
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.magenta;
		Gizmos.DrawWireSphere(transform.position,_earShot);
	}
}
