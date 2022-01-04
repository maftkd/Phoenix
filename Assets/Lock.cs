using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lock : MonoBehaviour
{
	Bird _bird;
	public int _id;
	bool _opened;
	public AudioClip _unlock;

	void Awake(){
		GameObject player =GameObject.FindGameObjectWithTag("Player");
		_bird=player.GetComponent<Bird>();
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void CheckLock(){
		if(_opened)
			return;
		Transform key = _bird.GetKey();
		if(key!=null){
			int id = key.GetComponent<Key>().GetId();
			if(id==_id){
				Debug.Log("Got a match");
				StartCoroutine(OpenLock(key));
			}
			else{
				Debug.Log("Mismatch");
			}
		}
		else{
			Debug.Log("ignore");
		}
	}

	IEnumerator OpenLock(Transform key){
		_opened=true;
		Transform cylinder=transform.GetChild(0);
		key.SetParent(cylinder);
		key.localPosition=new Vector3(0,-.03f,0.015f);
		key.localEulerAngles = new Vector3(-90f,-90f,0f);
		key.localScale=Vector3.one;
		//Sfx.PlayOneShot2D(_unlock);
		_bird.UseKey();
		float timer=0;
		float dur=1f;
		while(timer<dur){
			timer+=Time.deltaTime;
			//rotate
			yield return null;
		}
	}
}
