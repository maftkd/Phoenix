using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footstep : MonoBehaviour
{
	public AudioClip [] _clips;
	AudioSource [] _sources;
	public float _volume;

	public class FootstepEventArgs : System.EventArgs{
		public Vector3 pos;
		public float speed;
	}
	public delegate void EventHandler(FootstepEventArgs args);
	public event EventHandler OnFootstep;
    // Start is called before the first frame update
    void Start()
    {
		_sources = transform.GetComponentsInChildren<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void Sound(Vector3 pos,float speed){
		foreach(AudioSource s in _sources){
			if(!s.isPlaying){
				s.volume=_volume;
				s.transform.position=pos;
				s.clip=_clips[Random.Range(0,_clips.Length)];
				s.Play();
				if(OnFootstep!=null){
					FootstepEventArgs args = new FootstepEventArgs();
					args.pos=pos;
					args.speed=speed;
					OnFootstep.Invoke(args);
				}
				return;
			}
		}
	}
}
