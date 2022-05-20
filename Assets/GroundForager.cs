using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GroundForager : MonoBehaviour
{
	public int _state;
	float _theta;
	public Vector2 _dTheta;
	float _deltaTheta;
	public float _thetaChangeChance;
	Vector3 _hopDir;
	public Vector2 _listenDur;
	float _listenTime;
	float _listenTimer;
	Animator _anim;
	public float _peckDur;
	Text _debugText;
	public float _hopChance;
	public Vector2 _hopDist;
	public Vector2 _hopDur;
	public Vector2 _hopHeight;
	Terrain _terrain;

	void Awake(){
		_anim=GetComponent<Animator>();
		_debugText=transform.GetComponentInChildren<Text>();
		_theta = Random.value*Mathf.PI*2f;
		_hopDir=new Vector3(Mathf.Cos(_theta),0,Mathf.Sin(_theta));
		transform.forward=_hopDir;
		_terrain=transform.parent.GetComponent<BirdSpawner>()._terrain;
	}
    // Start is called before the first frame update
    void Start()
    {
		Listen();
		_deltaTheta=Random.Range(_dTheta.x,_dTheta.y);
		if(Random.value<0.5f)
			_deltaTheta*=-1f;
    }

    // Update is called once per frame
    void Update()
    {
		switch(_state){
			case 0:
				_listenTimer+=Time.deltaTime;
				if(_listenTimer>=_listenTime){
					StartCoroutine(GetWorm());
				}
				break;
			case 1://pecking
				break;
			default:
				break;
		}
		_debugText.text="State: "+_state.ToString("0");
    }

	void Listen(){
		_state=0;
		_listenTime=Random.Range(_listenDur.x,_listenDur.y);
		_listenTimer=0f;
	}

	IEnumerator GetWorm(){
		_state=1;
		_anim.SetBool("peck",true);
		yield return new WaitForSeconds(_peckDur);
		if(Random.value<_hopChance){
			StartCoroutine(Hop());
			_anim.SetBool("peck",false);
		}
		else
		{
			_anim.SetBool("peck",false);
			Listen();
		}
	}

	IEnumerator Hop(){
		_state=2;
		if(Random.value<_thetaChangeChance){
			_deltaTheta*=-1f;
		}
		_deltaTheta=Mathf.Sign(_deltaTheta)*Random.Range(_dTheta.x,_dTheta.y);
		_theta+=_deltaTheta;
		_hopDir=new Vector3(Mathf.Cos(_theta),0,Mathf.Sin(_theta));
		transform.forward=_hopDir;
		_anim.SetBool("hop",true);
		float dist=Random.Range(_hopDist.x,_hopDist.y);
		Vector3 newPos=transform.position+_hopDir*dist;
		float y = _terrain.SampleHeight(newPos);
		newPos.y=y;
		Vector3 startPos=transform.position;
		Vector3 endPos=newPos;
		float timer=0;
		float dur=Random.Range(_hopDur.x,_hopDur.y);
		float height=Random.Range(_hopHeight.x,_hopHeight.y);
		while(timer<dur){
			timer+=Time.deltaTime;
			float frac=timer/dur;
			Vector3 pos=Vector3.Lerp(startPos,endPos,frac);
			float yOffset=(-4*Mathf.Pow(frac-0.5f,2)+1)*height;
			//float yOffset=_jumpCurve.Evaluate(timer/dur)*height;
			pos.y+=yOffset;
			transform.position=pos;
			yield return null;
		}
		transform.position=endPos;
		_anim.SetBool("hop",false);
		Listen();
	}
}
