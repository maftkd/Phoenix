using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footstep : MonoBehaviour
{
	public AudioClip [] _clips;
	AudioSource [] _sources;
	[Range(0f,1f)]
	public float _volume;
	float _walkSpeed;
	float _runSpeed;

	public class FootstepEventArgs : System.EventArgs{
		public Vector3 pos;
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

	public void Sound(Vector3 pos,float speed=1f){
		if(_sources==null)
			return;
		foreach(AudioSource s in _sources){
			if(!s.isPlaying){
				s.volume=Mathf.InverseLerp(0,_runSpeed,speed)*_volume;
				s.volume=_volume;
				s.transform.position=pos;
				s.clip=_clips[Random.Range(0,_clips.Length)];
				s.Play();
				if(OnFootstep!=null)
				{
					FootstepEventArgs args= new FootstepEventArgs();
					args.pos=pos;
					OnFootstep.Invoke(args);
				}
				return;
			}
		}
	}
}
