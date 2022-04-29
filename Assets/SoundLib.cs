using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundLib : MonoBehaviour
{
	public AudioClip[] _sounds;
	public Texture2D[] _textures;
	public RawImage _image;
	public Text _counter;
	public AudioSource _speakerLeft;
	public AudioSource _speakerRight;
	int _curSound;

	void Awake(){
		_image.texture=_textures[_curSound];
		_speakerLeft.clip=_sounds[_curSound];
		_speakerRight.clip=_sounds[_curSound];
		_counter.text=(_curSound+1)+"/"+_sounds.Length;
	}

	public void PlayCurSound(){
		_speakerLeft.Play();
		_speakerRight.Play();
	}

	public void RotateSound(int dir){
		_curSound+=dir;
		if(_curSound<0)
			_curSound=_sounds.Length-1;
		else if(_curSound>=_sounds.Length)
			_curSound=0;
		_image.texture=_textures[_curSound];
		_speakerLeft.clip=_sounds[_curSound];
		_speakerRight.clip=_sounds[_curSound];
		_counter.text=(_curSound+1)+"/"+_sounds.Length;
		PlayCurSound();
	}
}
