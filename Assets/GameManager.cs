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
	public static MInput _mIn;
	public static MCamera _mCam;
	public static ColorPalette _color;
	public static GameObject _islands;
	public static GameObject _sky;
	public static PuzzleCounter _counter;

	void Awake(){
		_sfx=FindObjectOfType<Sfx>();
		_pauseScreen=transform.GetChild(0).gameObject;
		_solvedPuzzles=new List<string>();
		_instance=this;
		_player=GameObject.FindGameObjectWithTag("Player").GetComponent<Bird>();
		_mIn=FindObjectOfType<MInput>();
		_mCam=FindObjectOfType<MCamera>();
		_color = FindObjectOfType<ColorPalette>();
		_counter = FindObjectOfType<PuzzleCounter>();
		_islands=GameObject.Find("Islands");
		_sky=GameObject.Find("Sky");
	}

    // Start is called before the first frame update
    void Start()
    {
		//Play();
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
		/*
		if(_mIn.GetTabDown()&&Time.timeScale>0){
			_counter.Show();
		}
		else if(_mIn.GetTabUp()){
			_counter.Show(false);
		}
		*/
        
    }

	public void Pause(){
		_mCam.EnableCurrentShot(false);
		Time.timeScale=0;
		_sfx.Pause();
		_pauseScreen.SetActive(true);
		_mIn.LockInput(true);
		_mIn.EnableCursor(true);
		//SaveSlot [] slots = transform.GetComponentsInChildren<SaveSlot>();
		//slots[0].SelectSaveButton();
	}

	public void Play(){
		Time.timeScale=1f;
		_mCam.EnableCurrentShot(true);
		_sfx.Play();
		_pauseScreen.SetActive(false);
		_mIn.LockInput(false);
		_mIn.EnableCursor(false);
	}

	public void PuzzleSolved(PuzzleBox pb){
		if(!_solvedPuzzles.Contains(pb._puzzleId))
		{
			_solvedPuzzles.Add(pb._puzzleId);
			_counter.Increment(_solvedPuzzles.Count);
		}
	}

	public List<string> GetSolvedPuzzles(){
		return _solvedPuzzles;
	}

	public void ResetPuzzleCounter(){
		_counter.Reset();
	}
}
