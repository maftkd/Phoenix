using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeciesScreen : MonoBehaviour
{
	/*
	CanvasGroup _cg;
	public static SpeciesScreen _instance;
	public static bool _canSkip;
	Text _name;
	Text _scienceName;
	public float _fadeDelay;
	public float _fadeSpeed;
	Image _male;
	Image _female;

	void Awake(){
		_cg=GetComponent<CanvasGroup>();
		_cg.alpha=0f;
		_instance=this;
		_name=transform.GetChild(0).GetComponent<Text>();
		_scienceName=transform.GetChild(1).GetComponent<Text>();
		_male=transform.Find("Male").GetComponent<Image>();
		_female=transform.Find("Female").GetComponent<Image>();
		_male.enabled=false;
		_female.enabled=false;
	}
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public static void ShowSpecies(NPB npb){
		_canSkip=false;
		_instance.ShowSpeciesA(npb);
	}

	public void ShowSpeciesA(NPB npb){
		_name.text=npb._species;
		_scienceName.text=npb._speciesLatin;
		bool male=npb._sing._male;
		_male.enabled=male;
		_female.enabled=!male;
		StopAllCoroutines();
		StartCoroutine(Fade(1));
	}

	IEnumerator Fade(float target,bool delay=true){
		if(delay)
			yield return new WaitForSeconds(_fadeDelay);
		float start=_cg.alpha;
		float dir=1f;
		if(start>target)
		{
			dir=-1f;
		}
		while((dir==1f&&_cg.alpha<target)||(dir==-1f&&_cg.alpha>target)){
			_cg.alpha+=dir*Time.deltaTime*_fadeSpeed;
			yield return null;
		}
		_cg.alpha=target;
		_canSkip=true;
	}

	public static void Hide(){
		_instance.HideA();
	}

	public void HideA(){
		//_cg.alpha=0f;
		StopAllCoroutines();
		StartCoroutine(Fade(0,false));
	}

	public static bool IsActive(){
		return _instance._cg.alpha>0;
	}
	*/
}
