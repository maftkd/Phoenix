using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlot : MonoBehaviour
{
	public int _slotNumber;
	GameManager _gm;
	void Awake(){
		Button saveButt=transform.Find("SaveButton").GetComponent<Button>();
		saveButt.onClick.AddListener(delegate {SaveGame();});
		_gm=GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
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
		//get bird data
		Bird[] birds = FindObjectsOfType<Bird>();
		foreach(Bird b in birds){
			Debug.Log(b.name + " is at "+ b.transform.position);
		}

		//get puzzle data
	}
}
