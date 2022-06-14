using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuestManager : MonoBehaviour
{
	public string _targetSpecies;
	public Camera _gameViewCam;
	public Camera _birdViewCam;
	public Transform _perch;
	public Material _vignette;
	public int _nextScene;

	void Awake(){
		_perch.GetChild(0).gameObject.SetActive(false);
	}

    // Start is called before the first frame update
    void Start()
    {
		StartCoroutine(Reveal());
    }

	public void FoundBird(NPB bird){
		if(bird._species==_targetSpecies){
			Debug.Log("Yo! Quest complete!");
			//switch cams
			//disable game view cam
			_gameViewCam.enabled=false;
			//enable bird view cam
			_birdViewCam.enabled=true;
			bird.transform.SetParent(_perch);
			bird.transform.localPosition=Vector3.zero;
			//bird face player
			bird.TurnAround();
		}
	}

	public void GoNext(){
		Debug.Log("Going Next");
		StartCoroutine(GoNextR());
	}

	IEnumerator GoNextR(){
		float timer=0f;
		float dur=3f;
		while(timer<dur){
			timer+=Time.deltaTime;
			if(timer>dur)
				timer=dur;
			float frac = timer/dur;
			_vignette.SetFloat("_Amount",frac);
			yield return null;
		}
		Debug.Log("Loading next scene");
		if(_nextScene>=0)
			SceneManager.LoadScene(_nextScene);
	}

	IEnumerator Reveal(){
		float timer=0f;
		float dur=3f;
		while(timer<dur){
			timer+=Time.deltaTime;
			if(timer>dur)
				timer=dur;
			float frac = 1-timer/dur;
			_vignette.SetFloat("_Amount",frac);
			yield return null;
		}
		_vignette.SetFloat("_Amount",0);
	}
}
