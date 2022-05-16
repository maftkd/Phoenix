using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Sing : MonoBehaviour
{
	MInput _mIn;
	Animator _anim;
	int _state;
	Whistle _low;
	Whistle _mid;
	Whistle _high;

	[Header("Npb stuff")]
	public bool _npb;
	AudioSource _audio;
	public float _singPeriod;
	public string _birdName;
	public float _accuracyThreshold;
	public float _earShot;
	IEnumerator _cancelRoutine;
	Note[] _notes;
	float _songLength;

	Bird _player;

	public struct Note{
		public float _startT;
		public float _endT;
		public int _pitchId;
	}

	public delegate void SingEvent(int pitch, bool singing);
	public event SingEvent _onSing;
	bool _inPattern;
	int _patternNote;
	bool _patternDir=true;//true means next step in pattern is to hear the note. False means next step is for note to stop
	System.DateTime _lastSongEventTime;

	void Awake(){
		_mIn=GameManager._mIn;
		_low=transform.Find("Low").GetComponent<Whistle>();
		_mid=transform.Find("Mid").GetComponent<Whistle>();
		_high=transform.Find("High").GetComponent<Whistle>();
		
		_anim=transform.GetComponentInParent<Animator>();

		if(_npb)
		{
			_audio=GetComponent<AudioSource>();
			//get data
			string path = Application.streamingAssetsPath+"/Songs/"+_birdName+".song";
			byte[] data = File.ReadAllBytes(path);
			int numNotes=System.BitConverter.ToInt32(data,0);
			_songLength = System.BitConverter.ToSingle(data,4);
			Debug.Log("Loaded: "+ numNotes+ " notes");
			_notes = new Note[numNotes];
			for(int i=0; i<numNotes; i++){
				Note n;
				n._startT=System.BitConverter.ToSingle(data,i*12+8);
				n._endT=System.BitConverter.ToSingle(data,i*12+12);
				n._pitchId=System.BitConverter.ToInt32(data,i*12+16);
				_notes[i]=n;
			}

			_player=GameManager._player;
			Sing playerSing=_player.transform.GetComponentInChildren<Sing>();
			playerSing._onSing+=SongHandler;
			_lastSongEventTime=System.DateTime.Now;
		}
	}

	public void SingSong(){
		StartCoroutine(SingR());
	}

	IEnumerator SingR(){
		_audio.Play();
		float pitch=Random.Range(0.8f,1f);
		_audio.pitch=pitch;
		float dur=_audio.clip.length/pitch;
		float curTime=0;
		for(int i=0;i<_notes.Length; i++){
			float curNoteStart=_notes[i]._startT*dur;
			float curNoteEnd=_notes[i]._endT*dur;
			int pitchId=_notes[i]._pitchId;
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

    // Update is called once per frame
    void Update()
    {
		if(_npb)
			return;
		switch(_state){
			case 0://not singing
				if(_mIn.GetLowDown()){
					_low.Play();
					_anim.SetFloat("pitch",0.333f);
					_state=1;
					if(_onSing!=null)
						_onSing.Invoke(0,true);
				}
				else if(_mIn.GetMidDown()){
					_mid.Play();
					_anim.SetFloat("pitch",0.666f);
					_state=2;
					if(_onSing!=null)
						_onSing.Invoke(1,true);
				}
				else if(_mIn.GetHighDown()){
					_high.Play();
					_anim.SetFloat("pitch",1f);
					_state=3;
					if(_onSing!=null)
						_onSing.Invoke(2,true);
				}
				break;
			case 1://sing low
				if(_mIn.GetMidDown()){
					_low.Stop();
					_mid.Play();
					_anim.SetFloat("pitch",0.666f);
					_state=2;
					if(_onSing!=null)
						_onSing.Invoke(0,false);
					if(_onSing!=null)
						_onSing.Invoke(1,true);
				}
				else if(_mIn.GetHighDown()){
					_low.Stop();
					_high.Play();
					_anim.SetFloat("pitch",1f);
					_state=3;
					if(_onSing!=null)
						_onSing.Invoke(0,false);
					if(_onSing!=null)
						_onSing.Invoke(2,true);
				}
				else if(_mIn.GetLowUp()){
					_low.Stop();
					_state=0;
					_anim.SetFloat("pitch",0f);
					if(_onSing!=null)
						_onSing.Invoke(0,false);
				}
				break;
			case 2://sing mid
				if(_mIn.GetLowDown()){
					_mid.Stop();
					_low.Play();
					_anim.SetFloat("pitch",0.333f);
					_state=1;
					if(_onSing!=null)
						_onSing.Invoke(1,false);
					if(_onSing!=null)
						_onSing.Invoke(0,true);
				}
				else if(_mIn.GetHighDown()){
					_mid.Stop();
					_high.Play();
					_anim.SetFloat("pitch",1f);
					_state=3;
					if(_onSing!=null)
						_onSing.Invoke(1,false);
					if(_onSing!=null)
						_onSing.Invoke(2,true);
				}
				else if(_mIn.GetMidUp()){
					_mid.Stop();
					_state=0;
					_anim.SetFloat("pitch",0f);
					if(_onSing!=null)
						_onSing.Invoke(1,false);
				}
				break;
			case 3://sing high
				if(_mIn.GetLowDown()){
					_high.Stop();
					_low.Play();
					_anim.SetFloat("pitch",0.333f);
					_state=1;
					if(_onSing!=null)
						_onSing.Invoke(2,false);
					if(_onSing!=null)
						_onSing.Invoke(0,true);
				}
				else if(_mIn.GetMidDown()){
					_high.Stop();
					_mid.Play();
					_anim.SetFloat("pitch",0.666f);
					_state=2;
					if(_onSing!=null)
						_onSing.Invoke(2,false);
					if(_onSing!=null)
						_onSing.Invoke(1,true);
				}
				else if(_mIn.GetHighUp()){
					_high.Stop();
					_state=0;
					_anim.SetFloat("pitch",0f);
					if(_onSing!=null)
						_onSing.Invoke(2,false);
				}
				break;
			default:
				break;
		}
    }

	public void SongHandler(int pitchId, bool singing){
		if((transform.position-_player.transform.position).sqrMagnitude>_earShot*_earShot)
			return;
		Debug.Log("Heard note: "+pitchId+" dir: "+singing);
		if(_cancelRoutine!=null)
			StopCoroutine(_cancelRoutine);

		if(pitchId==_notes[_patternNote]._pitchId){
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
						perfectTime=(_notes[_patternNote]._endT-_notes[_patternNote]._startT)*_songLength;
					}
					else{
						perfectTime=(_notes[_patternNote]._startT-_notes[_patternNote-1]._endT)*_songLength;
					}
					if(Mathf.Abs(timeDelta-perfectTime)<_accuracyThreshold){
						NoteHandler(true);
						_cancelRoutine = CancelAfterMaxDelay(perfectTime+_accuracyThreshold);
						StartCoroutine(_cancelRoutine);
					}
					else{
						Debug.Log("time delta: "+timeDelta);
						Debug.Log("perfect time: "+perfectTime);
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
			Debug.Log("Success!");
			if(_patternNote==_notes.Length-1&&_patternDir==false){
				//check for last note in pattern
				Debug.Log("Full pattern success!");
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
		}
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.magenta;
		Gizmos.DrawWireSphere(transform.position,_earShot);
	}
}
