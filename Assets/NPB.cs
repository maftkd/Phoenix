using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPB : MonoBehaviour
{
	public AudioClip _call;
	float _callDelay;
	public Vector2 _callDelayRange;
	float _callTimer;
	public int _minCalls;
	public int _maxCalls;
	int _callCounter;
	public Vector2 _chillRange;
	int _state;
	public bool _scanned;
	bool _targetted;
	public string _species;
	QuestManager _quest;

	void Awake(){
		_callDelay=Random.Range(_callDelayRange.x,_callDelayRange.y);
		_callCounter=Random.Range(_minCalls,_maxCalls+1);
		_quest=FindObjectOfType<QuestManager>();
	}
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
		switch(_state){
			case 0:
				_callTimer+=Time.deltaTime;
				if(_callTimer>=_callDelay){
					if(_call!=null)
						Sfx.PlayOneShot3D(_call,transform.position,Random.Range(0.95f,1.05f));
					_callTimer=0f;
					_callCounter--;
					if(_callCounter<=0){
						_state=1;
						_callDelay=Random.Range(_chillRange.x,_chillRange.y);
					}
					else
						_callDelay=Random.Range(_callDelayRange.x,_callDelayRange.y);
				}
				break;
			case 1:
				_callTimer+=Time.deltaTime;
				if(_callTimer>=_callDelay){
					_callDelay=Random.Range(_chillRange.x,_chillRange.y);
					_callCounter=Random.Range(_minCalls,_maxCalls+1);
					_state=0;
				}
				break;
		}
    }

	public void Targeted(bool foo){
		_targetted=foo;
	}

	void OnTriggerEnter(Collider other){
		Debug.Log("yoyoyoyo");
		_quest.FoundBird(this);
		//quest man - found bird
		//tree view
		//game logic somewhere
		//animations and stuff
	}

	public void TurnAround(){
		StartCoroutine(TurnAroundR());
	}

	IEnumerator TurnAroundR(){
		Quaternion startRot=transform.rotation;
		transform.forward=-transform.forward;
		Quaternion endRot=transform.rotation;
		Vector3 startPos=transform.position;
		Vector3 jumpPos=startPos+Vector3.up*0.1f;
		float timer=0;
		float dur=0.5f;
		while(timer<dur){
			timer+=Time.deltaTime;
			float frac=timer/dur;
			transform.rotation=Quaternion.Slerp(startRot,endRot,frac);
			float lerp=Mathf.Pow(frac-0.5f,2)*-4f+1;
			transform.position=Vector3.Lerp(startPos,jumpPos,lerp);
			yield return null;
		}

		//yield return new WaitForSeconds(1f);

		_quest.GoNext();
	}
}
