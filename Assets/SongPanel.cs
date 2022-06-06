using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SongPanel : MonoBehaviour
{
	Bird _player;
	public Transform _menuItem;
	GameObject _noSongs;
	Transform _scrollList;
	public static SongPanel _instance;
	public RenderTexture _maleTex;
	public RenderTexture _femaleTex;
	Text _mnemonic;

	/*
	void Awake(){
		_player=GameManager._player;
		Transform songList=transform.Find("SongList");
		_noSongs=songList.Find("NoSongs").gameObject;
		_noSongs.SetActive(false);
		_scrollList=songList.Find("Scroll View").Find("Viewport").Find("Content");
		_instance=this;
		_mnemonic=transform.Find("Mnemonic").Find("Panel").GetChild(0).GetComponent<Text>();
	}

	void OnEnable(){
		int numSongs=0;
		for(int i=_scrollList.childCount-1;i>=0;i--){
			Destroy(_scrollList.GetChild(i).gameObject);
		}
		foreach(Sing.BirdSong song in _player._songs){
			Debug.Log("know song: "+song._fileName);
			Transform menuItem = Instantiate(_menuItem,_scrollList);
			menuItem.GetChild(0).GetComponent<Text>().text=song._species;
			Button butt = menuItem.GetComponent<Button>();
			if(numSongs==0)
				Select(menuItem);
			butt.onClick.AddListener(delegate {SelectSong(song);});
			numSongs++;
		}
		_noSongs.SetActive(numSongs==0);
	}

	void OnDisable(){
		if(this==null)
			return;
		foreach(Sing.BirdSong song in _player._songs){
			if(song._male!=null){
				Camera maleCam=song._male.transform.Find("Camera").GetComponent<Camera>();
				Camera femaleCam=song._female.transform.Find("Camera").GetComponent<Camera>();
				maleCam.enabled=false;
				femaleCam.enabled=false;
			}
		}

	}

	void SelectSong(Sing.BirdSong song){
		Debug.Log("Clicked song: "+song._species);
		//unpause
		GameManager.Play();
		//
		//player sing song(song)
		_player.SingSong(song);
	}

	public static void Select(Transform item){
		_instance.SelectA(item);
	}

	void SelectA(Transform item){
		foreach(Transform t in _scrollList){
			if(t==item){
				t.GetComponent<Button>().Select();
				foreach(Sing.BirdSong song in _player._songs){
					Camera maleCam=song._male.transform.Find("Camera").GetComponent<Camera>();
					Camera femaleCam=song._female.transform.Find("Camera").GetComponent<Camera>();
					bool isSelected=song._species==t.GetChild(0).GetComponent<Text>().text;
					maleCam.enabled=isSelected;
					femaleCam.enabled=isSelected;
					if(isSelected){
						maleCam.targetTexture=_maleTex;
						femaleCam.targetTexture=_femaleTex;
						_mnemonic.text=song._mnemonic;
					}
				}
				return;
			}
		}

	}
*/
}
