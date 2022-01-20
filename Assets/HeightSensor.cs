using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HeightSensor : MonoBehaviour
{
	Transform _player;
	Material _mat;
	Bird _bird;
	public MeshRenderer _progressBar;
	Material _progressMat;
	public float _targetTime;
	float _targetTimer;
	public float _gravityMult;
	float _targetWidth;
	float _uvPerSec;

	public UnityEvent _progressMaxed;

	void Awake(){
		_player=GameObject.FindGameObjectWithTag("Player").transform;
		_mat=GetComponent<MeshRenderer>().material;
		_bird=_player.GetComponent<Bird>();
		_targetWidth=_mat.GetFloat("_TargetWidth");
		_uvPerSec=1f/_targetTime;
		_progressMat=_progressBar.material;
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		float diff = transform.position.y-_player.position.y-_bird._size.y*2f;
		diff=-diff*2f;
		_mat.SetFloat("_HeightMark",Mathf.Max(0f,diff));
		if(Mathf.Abs(diff-0.5f)<_targetWidth*0.5f&&_bird.IsPlayerInRange(transform,2f)){
			_targetTimer+=_uvPerSec*Time.deltaTime;
			_progressMat.SetFloat("_FillAmount",_targetTimer);
		}
		else if(_targetTimer>0)
		{
			_targetTimer-=Time.deltaTime*_uvPerSec*_gravityMult;
			_progressMat.SetFloat("_FillAmount",_targetTimer);
		}

		if(_targetTimer>1f)
			_progressMaxed.Invoke();
    }
}
