using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speaker : MonoBehaviour
{
	public AudioClip [] _clips;
	AudioSource _audio;
	public static Speaker _instance;

	public class SpeakerEventArgs : System.EventArgs{
		public string name;
		public float dur;
	}

	public delegate void EventHandler(SpeakerEventArgs args);
	public event EventHandler OnSpeakerPlay;

	void Awake(){
		_instance=this;
	}
    // Start is called before the first frame update
    void Start()
    {
		_audio=transform.GetChild(0).GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
		if(Input.GetKeyDown(KeyCode.Alpha1)){
			_audio.clip=_clips[0];
			_audio.Play();
			SpeakerEventArgs args = new SpeakerEventArgs();
			args.name=_audio.clip.name;
			args.dur=_audio.clip.length;
			if(OnSpeakerPlay!=null)
				OnSpeakerPlay.Invoke(args);
		}
    }
}
