﻿using System.Collections;
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
	int _state;
	Whistle _low;
	Whistle _mid;
	Whistle _high;

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

	[Header("Npb stuff")]
	public NPB _npb;
	public BirdSong [] _songs;
	public AudioClip [] _calls;
	public BirdSong [] _alarms;
	public Vector2 _responseRange;
	public float _responseChance;
	public float _accuracyThreshold;
	public float _earShot;
	IEnumerator _cancelRoutine;
	public UnityEvent _onPatternSuccess;
	public bool _male;
	public Vector2 _singDelayRange;
	float _singDelay;
	float _singTimer;
	BirdSpawner _birdSpawner;

	Bird _player;
	Transform _cam;

	//public delegate void SingEvent();
	//public event SingEvent _onSing;
	bool _inPattern;
	int _patternNote;
	bool _patternDir=true;//true means next step in pattern is to hear the note. False means next step is for note to stop
	BirdSong _curSong;
	int _curSongIndex;
	Sing _mate;
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
		if(_npb==null)
			_cam=GameManager._mCam.transform;
		_birdSpawner=transform.parent.parent.GetComponent<BirdSpawner>();
	}

	void Start(){
		//setup songs - this is done after awake because we need to ensure _male is set
		if(_npb!=null)
		{
			if(_male)
			{
				for(int i=0;i<_songs.Length; i++){
					Deserialize(_songs, i);
				}
				for(int i=0;i<_alarms.Length; i++){
					Deserialize(_alarms,i);
				}
				//may rework this because we don't need this to be event driven
				//the only time we are listening to notes is when we are locked in and the player
				//is in the sing state
				//Sing playerSing=_player.transform.GetComponentInChildren<Sing>();
				//playerSing._onSing+=SongHandler;
			}
			_lastSongEventTime=System.DateTime.Now;
		}
		_singDelay=Random.Range(_singDelayRange.x,_singDelayRange.y);
	}

	public void SingSong(){
		if(_state==1)
			return;
		_curSongIndex=Random.Range(0,_songs.Length);
		_curSong=_songs[_curSongIndex];
		StartCoroutine(SingR(_curSong));
	}

	IEnumerator SingR(BirdSong bs,bool delay=true){
		//wait a sec - play it cool
		if(delay)
			yield return new WaitForSeconds(Random.Range(_responseRange.x,_responseRange.y));

		//setup audio
		float pitch=Random.Range(0.95f,1.05f);
		Sfx.PlayOneShot3D(bs._clip,transform.position,pitch);

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
	}

	public void Call(){
		StartCoroutine(CallR());
	}

	IEnumerator CallR(){
		float pitch=Random.Range(0.8f,1f);
		AudioClip call=_calls[Random.Range(0,_calls.Length)];
		Sfx.PlayOneShot3D(call,transform.position,pitch);
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
		if(_npb!=null)
		{
			//#temp
			/*
			if(_male&&!_npb._listening){
				_singTimer+=Time.deltaTime;
				if(_singTimer>_singDelay){
					_singTimer=0;
					SingSong();
					_singDelay=Random.Range(_singDelayRange.x,_singDelayRange.y);
				}
			}
			*/
			if(!_male){
				switch(_state){
					case 0:
						_singTimer+=Time.deltaTime;
						if(_singTimer>_singDelay){
							_singTimer=0;
							Call();
							_singDelay=Random.Range(_singDelayRange.x,_singDelayRange.y);
						}
						break;
					case 1:
						break;
					default:
						break;
				}
			}
			return;
		}
		if(_player._state!=6)
			return;
		switch(_state){
			case 0://not singing
				if(_mIn.GetLowDown()){
					_low.Play();
					_anim.SetFloat("pitch",0.333f);
					_state=1;
					//rework this 
					//instead of firing an event
					//we can sing directly to the bird that we have engaged
					_targetBird.HandleNote(0,true);
				}
				else if(_mIn.GetMidDown()){
					_mid.Play();
					_anim.SetFloat("pitch",0.666f);
					_state=2;
					_targetBird.HandleNote(1,true);
				}
				else if(_mIn.GetHighDown()){
					_high.Play();
					_anim.SetFloat("pitch",1f);
					_state=3;
					_targetBird.HandleNote(2,true);
				}
				break;
			case 1://sing low
				if(_mIn.GetMidDown()){
					_low.Stop();
					_mid.Play();
					_anim.SetFloat("pitch",0.666f);
					_state=2;
					_targetBird.HandleNote(0,false);
					_targetBird.HandleNote(1,true);
				}
				else if(_mIn.GetHighDown()){
					_low.Stop();
					_high.Play();
					_anim.SetFloat("pitch",1f);
					_state=3;
					_targetBird.HandleNote(0,false);
					_targetBird.HandleNote(2,true);
				}
				else if(_mIn.GetLowUp()){
					_low.Stop();
					_state=0;
					_anim.SetFloat("pitch",0f);
					_targetBird.HandleNote(0,false);
				}
				break;
			case 2://sing mid
				if(_mIn.GetLowDown()){
					_mid.Stop();
					_low.Play();
					_anim.SetFloat("pitch",0.333f);
					_state=1;
					_targetBird.HandleNote(1,false);
					_targetBird.HandleNote(0,true);
				}
				else if(_mIn.GetHighDown()){
					_mid.Stop();
					_high.Play();
					_anim.SetFloat("pitch",1f);
					_state=3;
					_targetBird.HandleNote(1,false);
					_targetBird.HandleNote(2,true);
				}
				else if(_mIn.GetMidUp()){
					_mid.Stop();
					_state=0;
					_anim.SetFloat("pitch",0f);
					_targetBird.HandleNote(1,false);
				}
				break;
			case 3://sing high
				if(_mIn.GetLowDown()){
					_high.Stop();
					_low.Play();
					_anim.SetFloat("pitch",0.333f);
					_state=1;
					_targetBird.HandleNote(2,false);
					_targetBird.HandleNote(0,true);
				}
				else if(_mIn.GetMidDown()){
					_high.Stop();
					_mid.Play();
					_anim.SetFloat("pitch",0.666f);
					_state=2;
					_targetBird.HandleNote(2,false);
					_targetBird.HandleNote(1,true);
				}
				else if(_mIn.GetHighUp()){
					_high.Stop();
					_state=0;
					_anim.SetFloat("pitch",0f);
					_targetBird.HandleNote(2,false);
				}
				break;
			default:
				break;
		}
    }

	void LateUpdate(){
		if(_npb==null)
			transform.forward=_cam.forward;
	}

	public void HandleNote(int pitchId, bool singing){
		if((transform.position-_player.transform.position).sqrMagnitude>_earShot*_earShot)
			return;
		if(!_npb._listening)
			return;
		//Debug.Log("Heard note: "+pitchId+" dir: "+singing);
		if(_cancelRoutine!=null)
			StopCoroutine(_cancelRoutine);

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
		//play sound

		BirdSong alarmSong=_alarms[Random.Range(0,_alarms.Length)];
		StartCoroutine(SingR(alarmSong,false));
		
		//change state
		_state=1;
	}

	public void Chill(){
		_state=0;
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

	void OnDrawGizmos(){
		Gizmos.color=Color.magenta;
		Gizmos.DrawWireSphere(transform.position,_earShot);
	}
}
