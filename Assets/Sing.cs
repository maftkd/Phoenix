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

	public bool _npb;
	AudioSource _audio;
	public float _singPeriod;
	public string _birdName;
	Note[] _notes;

	public struct Note{
		public float _startT;
		public float _endT;
		public int _pitchId;
	}

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
			Debug.Log("Loaded: "+ numNotes+ " notes");
			_notes = new Note[numNotes];
			for(int i=0; i<numNotes; i++){
				Note n;
				n._startT=System.BitConverter.ToSingle(data,i*12+4);
				n._endT=System.BitConverter.ToSingle(data,i*12+8);
				n._pitchId=System.BitConverter.ToInt32(data,i*12+12);
				_notes[i]=n;
			}

			StartCoroutine(SingR());
		}
	}

	IEnumerator SingR(){
		_audio.Play();
		float dur=_audio.clip.length;
		float curTime=0;
		for(int i=0;i<_notes.Length; i++){
			float curNoteStart=_notes[i]._startT;
			float curNoteEnd=_notes[i]._endT;
			int pitchId=_notes[i]._pitchId;
			float delta=curNoteStart-curTime;
			yield return new WaitForSeconds(delta*dur);
			curTime+=delta*dur;
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
			yield return new WaitForSeconds(delta*dur);
			curTime+=delta*dur;
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

		yield return new WaitForSeconds(_singPeriod);
		StartCoroutine(SingR());
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
				}
				else if(_mIn.GetMidDown()){
					_mid.Play();
					_anim.SetFloat("pitch",0.666f);
					_state=2;
				}
				else if(_mIn.GetHighDown()){
					_high.Play();
					_anim.SetFloat("pitch",1f);
					_state=3;
				}
				break;
			case 1://sing low
				if(_mIn.GetMidDown()){
					_low.Stop();
					_mid.Play();
					_anim.SetFloat("pitch",0.666f);
					_state=2;
				}
				else if(_mIn.GetHighDown()){
					_low.Stop();
					_high.Play();
					_anim.SetFloat("pitch",1f);
					_state=3;
				}
				else if(_mIn.GetLowUp()){
					_low.Stop();
					_state=0;
					_anim.SetFloat("pitch",0f);
				}
				break;
			case 2://sing mid
				if(_mIn.GetLowDown()){
					_mid.Stop();
					_low.Play();
					_anim.SetFloat("pitch",0.333f);
					_state=1;
				}
				else if(_mIn.GetHighDown()){
					_mid.Stop();
					_high.Play();
					_anim.SetFloat("pitch",1f);
					_state=3;
				}
				else if(_mIn.GetMidUp()){
					_mid.Stop();
					_state=0;
					_anim.SetFloat("pitch",0f);
				}
				break;
			case 3://sing high
				if(_mIn.GetLowDown()){
					_high.Stop();
					_low.Play();
					_anim.SetFloat("pitch",0.333f);
					_state=1;
				}
				else if(_mIn.GetMidDown()){
					_high.Stop();
					_mid.Play();
					_anim.SetFloat("pitch",0.666f);
					_state=2;
				}
				else if(_mIn.GetHighUp()){
					_high.Stop();
					_state=0;
					_anim.SetFloat("pitch",0f);
				}
				break;
			default:
				break;
		}
    }
}
