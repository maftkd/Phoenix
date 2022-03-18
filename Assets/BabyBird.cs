using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BabyBird : MonoBehaviour
{
	int _state;
	Bird _player;
	public Fireplace _fireplace;
	Transform _egg;
	Material _eggMat;
	float _heat;
	public GameObject _hatchling;
	Transform _explosion;
	public float _flyAwayDelay;

	void Awake(){
		_player=GameManager._player;
		_egg=transform.Find("Egg");
		_eggMat=_egg.GetComponent<Renderer>().material;
		_explosion=transform.Find("Explosion");
	}

	void OnEnable(){
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

	public void GetWarm(){
		if(_state!=0)
			return;
		float h = _fireplace._fill;
		//if(h>_heat){
			_heat=h;
			_eggMat.SetFloat("_Crack",_heat);
			if(_heat>=1f)
				Hatch();
		//}
	}

	public void Hatch(){
		_state=1;
		_egg.gameObject.SetActive(false);
		_explosion.gameObject.SetActive(true);
		_hatchling.SetActive(true);
		StartCoroutine(FlyAwayAfter(_flyAwayDelay));
	}

	IEnumerator FlyAwayAfter(float del){
		yield return new WaitForSeconds(del);
		_hatchling.GetComponent<BirdSimple>().FlyAway();
	}

}
