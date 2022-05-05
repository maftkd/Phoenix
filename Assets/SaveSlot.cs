using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SaveSlot : MonoBehaviour
{
	public int _slotNumber;
	string _saveFile;
	static string _divider = "---";

	void Awake(){
		/*
		Button saveButt=transform.Find("SaveButton").GetComponent<Button>();
		saveButt.onClick.AddListener(delegate {SaveGame();});
		Button loadButt=transform.Find("LoadButton").GetComponent<Button>();
		loadButt.onClick.AddListener(delegate {LoadGame();});
		Button deleteButt=transform.Find("DeleteButton").GetComponent<Button>();
		deleteButt.onClick.AddListener(delegate {DeleteGame();});
		*/
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
		Bird b = GameManager._player;
		saveData+=b.name+"%"+b.transform.position.x+"%"+b.transform.position.y+"%"+b.transform.position.z
			+System.Environment.NewLine;

		//add divider
		saveData+=_divider+System.Environment.NewLine;

		//get puzzle data
		List<BirdHouse> puzzles=GameManager._instance.GetSolvedPuzzles();
		foreach(BirdHouse bh in puzzles)
		{
			string name = bh._houseId;
			bool intActive = bh.GetInteriorActive();
			saveData+=name+"%"+intActive+System.Environment.NewLine;
		}

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
		Bird[] birds = FindObjectsOfType<Bird>();
		BirdHouse [] houses = FindObjectsOfType<BirdHouse>();
		foreach(string l in lines){
			if(l==_divider)
			{
				headerCode++;
				continue;
			}

			string[] parts;
			switch(headerCode){
				case 0://birds
					parts = l.Split('%');
					string birdName = parts[0];
					float.TryParse(parts[1],NumberStyles.Float,CultureInfo.InvariantCulture,out x);
					float.TryParse(parts[2],NumberStyles.Float,CultureInfo.InvariantCulture,out y);
					float.TryParse(parts[3],NumberStyles.Float,CultureInfo.InvariantCulture,out z);
					Vector3 pos = new Vector3(x,y,z);
					Bird b = GameManager._player;
					b.transform.position=pos;
					b.ResetState();
					break;
				case 1://puzzles
					parts = l.Split('%');
					string houseName = parts[0];
					bool intActive=false;
					bool.TryParse(parts[1],out intActive);
					foreach(BirdHouse bh in houses){
						if(bh._houseId==houseName)
						{
							bh.Solve(true);
							bh.SetInteriorActive(intActive);
						}
					}
					break;
				default:
					break;
			}
		}

		GameManager._instance.Play();
	}

	public void DeleteGame(){
		if(File.Exists(_saveFile))
			File.Delete(_saveFile);
	}

	public void SelectSaveButton(){
		//EventSystem.current.SetSelectedGameObject(transform.Find("SaveButton").gameObject);
		//transform.Find("SaveButton").GetComponent<Button>().Select();
	}
	/*
	public Button GetSaveButton(){
		Button saveButt=transform.Find("SaveButton").GetComponent<Button>();
	}
	*/
}
