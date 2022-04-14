using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour
{
	float _animDur=0.25f;
	Vector3 _defaultScale;
	Vector3 _squishedScale;

	void Awake(){
		_defaultScale=transform.localScale;
		_squishedScale=_defaultScale;
		_squishedScale.y*=0.5f;
	}

	public void React(){
		if(GameManager._player.IsFlying())
		{
			StopAllCoroutines();
			StartCoroutine(Shrink());
		}
	}
	IEnumerator Shrink(){
		float timer=0;
		Vector3 startScale=transform.localScale;
		while(timer<_animDur){
			timer+=Time.deltaTime;
			float frac=timer/_animDur;
			transform.localScale=Vector3.Lerp(startScale,_squishedScale,frac);
			yield return null;
		}
	}

	public void Relax(){
		StopAllCoroutines();
		StartCoroutine(RelaxR());
	}

	IEnumerator RelaxR(){
		float timer=0;
		Vector3 startScale=transform.localScale;
		while(timer<_animDur){
			timer+=Time.deltaTime;
			float frac=timer/_animDur;
			transform.localScale=Vector3.Lerp(startScale,_defaultScale,frac);
			yield return null;
		}
	}
}
