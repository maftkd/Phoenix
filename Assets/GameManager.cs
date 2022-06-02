using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	//Sfx _sfx;
	public GameObject _pauseScreen;
	List<BirdHouse> _solvedPuzzles;
	public static GameManager _instance;
	public static Bird _player;
	public static Transform _mate;
	public static MInput _mIn;
	public static MCamera _mCam;
	public static ColorPalette _color;
	public static GameObject _islands;
	public static GameObject _sky;
	bool _editMode;
	bool _cli;
	MEditor _editor;

	void Awake(){
		//_sfx=FindObjectOfType<Sfx>();
		_solvedPuzzles=new List<BirdHouse>();
		_instance=this;
		_player=GameObject.FindGameObjectWithTag("Player").GetComponent<Bird>();
		//_mate=GameObject.FindGameObjectWithTag("Mate").transform;
		_mIn=FindObjectOfType<MInput>();
		_mCam=FindObjectOfType<MCamera>();
		_color = FindObjectOfType<ColorPalette>();
		_islands=GameObject.Find("Islands");
		_sky=GameObject.Find("Sky");
		_editor=FindObjectOfType<MEditor>();
	}

    // Start is called before the first frame update
    void Start()
    {
		//Play();
    }

    // Update is called once per frame
    void Update()
    {
		/*
		if(Input.GetKeyDown(KeyCode.F1)){
			ToggleEditMode();
		}
		*/
    }

	void LateUpdate(){
		if(Input.GetButtonDown("Interact"))
		{
			if(_pauseScreen.activeSelf)
				PlayA();
			else
			{
				//cannot pause when we are zoomed in species screen
				if(!SpeciesScreen.IsActive()&&!_cli)
					PauseA();
			}
			/*
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
*/
		}
		if(Input.GetKeyDown(KeyCode.F1)){
			ToggleCli();
		}
	}

	public static void Pause(){
		_instance.PauseA();
	}

	public void PauseA(){
		_mCam.EnableCurrentShot(false);
		//Time.timeScale=0;
		Sfx.PauseBg();
		_pauseScreen.SetActive(true);
		_mIn.LockInput(true);
		_mIn.EnableCursor(true);
		//SaveSlot [] slots = transform.GetComponentsInChildren<SaveSlot>();
		//slots[0].SelectSaveButton();
		_player.EnterMenuState();
		Sfx.SetFloat("BirdVol",-80f);
	}

	public static void Play(){
		_instance.PlayA();
	}

	public void PlayA(){
		//Time.timeScale=1f;
		_mCam.EnableCurrentShot(true);
		Sfx.PlayBg();
		_pauseScreen.SetActive(false);
		_mIn.LockInput(false);
		_mIn.EnableCursor(false);
		Sfx.SetFloat("BirdVol",0f);
		_player.ExitMenuState();
	}

	public void PuzzleSolved(BirdHouse bh){
		if(!_solvedPuzzles.Contains(bh))
		{
			_solvedPuzzles.Add(bh);
		}
	}

	public List<BirdHouse> GetSolvedPuzzles(){
		return _solvedPuzzles;
	}

	public void ResetPuzzleCounter(){
	}

	void ToggleEditMode(){
		_editMode=!_editMode;
		Debug.Log("Edit Mode: "+_editMode);
		_editor.enabled=_editMode;
	}

	public void ToggleCli(){
		_cli=!_cli;
		DebugScreen.EnableCli(_cli);
	}
}
