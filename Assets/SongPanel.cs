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

	void Awake(){
		_player=GameManager._player;
		Transform songList=transform.Find("SongList");
		_noSongs=songList.Find("NoSongs").gameObject;
		_noSongs.SetActive(false);
		_scrollList=songList.Find("Scroll View").Find("Viewport").Find("Content");
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
				butt.Select();
			butt.onClick.AddListener(delegate {SelectSong(song);});
			numSongs++;
		}
		_noSongs.SetActive(numSongs==0);
	}

	void SelectSong(Sing.BirdSong song){
		Debug.Log("Clicked song: "+song._species);
	}
}
