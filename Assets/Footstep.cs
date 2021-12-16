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
	public static int _numSources=10;

	public class FootstepEventArgs : System.EventArgs{
		public Vector3 pos;
	}
	public delegate void EventHandler(FootstepEventArgs args);
	public event EventHandler OnFootstep;
    // Start is called before the first frame update
    void Start()
    {
		_sources = new AudioSource[_numSources];
		for(int i=0; i<_numSources; i++){
			GameObject foo = new GameObject("AudioSource");
			foo.transform.SetParent(transform);
			_sources[i]=foo.AddComponent<AudioSource>();
			_sources[i].spatialBlend=1f;
		}
		//_sources = transform.GetComponentsInChildren<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void Sound(Vector3 pos,float volume=-1f){
		if(_sources==null)
			return;
		foreach(AudioSource s in _sources){
			if(!s.isPlaying){
				s.volume=volume<0? _volume : volume;
				//s.volume=Mathf.InverseLerp(0,_runSpeed,speed)*_volume;
				//s.volume=_volume;
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
