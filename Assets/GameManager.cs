using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	Sfx _sfx;
	GameObject _pauseScreen;
	List<string> _solvedPuzzles;
	public static GameManager _instance;
	public static Bird _player;
	public static MCamera _mCam;
	public static MInput _mIn;

	void Awake(){
		_sfx=FindObjectOfType<Sfx>();
		_pauseScreen=transform.GetChild(0).gameObject;
		_solvedPuzzles=new List<string>();
		_instance=this;
		_player=GameObject.FindGameObjectWithTag("Player").GetComponent<Bird>();
		_mCam=Camera.main.transform.parent.GetComponent<MCamera>();
		_mIn=_mCam.GetComponent<MInput>();
	}

    // Start is called before the first frame update
    void Start()
    {
		Play();
    }

    // Update is called once per frame
    void Update()
    {
		if(Input.GetButtonDown("Pause"))
		{
			if(Time.timeScale==0)
				Play();
			else
				Pause();
			/*
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
*/
		}
        
    }

	public void Pause(){
		Time.timeScale=0;
		_sfx.Pause();
		_pauseScreen.SetActive(true);
		_mIn.LockInput(true);
		SaveSlot [] slots = transform.GetComponentsInChildren<SaveSlot>();
		slots[0].SelectSaveButton();
	}

	public void Play(){
		Time.timeScale=1f;
		_sfx.Play();
		_pauseScreen.SetActive(false);
		_mIn.LockInput(false);
	}

	public void PuzzleSolved(PuzzleBox pb){
		_solvedPuzzles.Add(pb._puzzleId);
	}

	public List<string> GetSolvedPuzzles(){
		return _solvedPuzzles;
	}
}
