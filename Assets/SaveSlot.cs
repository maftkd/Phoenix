﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlot : MonoBehaviour
{
	public int _slotNumber;
	GameManager _gm;
	string _saveFile;
	static string _divider = "---";

	void Awake(){
		Button saveButt=transform.Find("SaveButton").GetComponent<Button>();
		saveButt.onClick.AddListener(delegate {SaveGame();});
		Button loadButt=transform.Find("LoadButton").GetComponent<Button>();
		loadButt.onClick.AddListener(delegate {LoadGame();});
		Button deleteButt=transform.Find("DeleteButton").GetComponent<Button>();
		deleteButt.onClick.AddListener(delegate {DeleteGame();});
		_gm=GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
		_saveFile=Application.streamingAssetsPath+"/saveSlot"+_slotNumber+".txt";
	}
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void SaveGame(){
		Debug.Log("Saving to slot "+_slotNumber+"!");
		string saveData="";
		//get bird data
		Bird[] birds = FindObjectsOfType<Bird>();
		foreach(Bird b in birds){
			Debug.Log(b.name + " is at "+ b.transform.position);
			saveData+=b.name+"%"+b.transform.position.x+"%"+b.transform.position.y+"%"+b.transform.position.z
				+"%"+b._seeds+System.Environment.NewLine;
		}

		//add divider
		saveData+=_divider+System.Environment.NewLine;

		//get puzzle data
		List<string> puzzles=_gm.GetSolvedPuzzles();
		foreach(string p in puzzles)
			saveData+=p+System.Environment.NewLine;

		File.WriteAllText(_saveFile,saveData);
	}

	public void LoadGame(){
		if(!File.Exists(_saveFile))
		{
			Debug.Log("Save file not found");
			return;
		}
		string[] lines = File.ReadAllLines(_saveFile);
		int headerCode=0;
		float x=0;
		float y=0;
		float z=0;
		int seeds=0;
		Bird[] birds = FindObjectsOfType<Bird>();
		PuzzleBox [] puzzles = FindObjectsOfType<PuzzleBox>();
		foreach(string l in lines){
			if(l==_divider)
			{
				headerCode++;
				continue;
			}

			switch(headerCode){
				case 0://birds
					string [] parts = l.Split('%');
					string birdName = parts[0];
					float.TryParse(parts[1],NumberStyles.Float,CultureInfo.InvariantCulture,out x);
					float.TryParse(parts[2],NumberStyles.Float,CultureInfo.InvariantCulture,out y);
					float.TryParse(parts[3],NumberStyles.Float,CultureInfo.InvariantCulture,out z);
					int.TryParse(parts[4],NumberStyles.Integer,CultureInfo.InvariantCulture, out seeds);
					Vector3 pos = new Vector3(x,y,z);
					foreach(Bird b in birds){
						if(b.name==birdName)
						{
							b.transform.position=pos;
							b.SetSeeds(seeds);
						}
					}
					break;
				case 1://puzzles
					foreach(PuzzleBox pb in puzzles){
						if(pb._puzzleId==l)
							pb.SolveSilent();
					}
					break;
				default:
					break;
			}
		}

		_gm.Play();
	}

	public void DeleteGame(){
		if(File.Exists(_saveFile))
			File.Delete(_saveFile);
	}
}