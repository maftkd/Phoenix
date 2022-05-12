using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Woodpecker : MonoBehaviour
{
	public Vector2 _delayRange;
	public AudioClip _sound;

    // Start is called before the first frame update
    void Start()
    {
		StartCoroutine(MakeSound());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	IEnumerator MakeSound(){
		float delay = Random.Range(_delayRange.x,_delayRange.y);
		yield return new WaitForSeconds(delay);
		Sfx.PlayOneShot3D(_sound,transform.position);
		StartCoroutine(MakeSound());
	}
}
