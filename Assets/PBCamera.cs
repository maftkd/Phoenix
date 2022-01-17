using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBCamera : PuzzleBox
{
	/*
	Transform _box;
	*/
	public float _liftDelay;
	public float _liftDur;
	public AnimationCurve _liftCurve;
	public AudioSource _gearsAudio;
	public MeshRenderer _mouth;
	Material _mouthMat;
	protected override void Awake(){
		base.Awake();
		//your code here
		_mouthMat=_mouth.material;
	}

	protected override void OnEnable(){
		base.OnEnable();
		//your code here
	}

	protected override void OnDisable(){
		base.OnDisable();
		//your code here
	}

    // Start is called before the first frame update
    protected override void Start()
    {
		base.Start();
		//your code here
    }

    // Update is called once per frame
    protected override void Update()
    {
		base.Update();
		//your code here
    }

	public override void PuzzleSolved(){
		base.PuzzleSolved();
		//your code here
	}

	protected override IEnumerator OpenBox(){
		yield return new WaitForSeconds(_liftDelay);
		_gearsAudio.Play();
		float timer=0;
		float mouthStart=_mouthMat.GetFloat("_OpenAmount");
		float mouthEnd=1f;
		while(timer<_liftDur){
			timer+=Time.deltaTime;
			_mouthMat.SetFloat("_OpenAmount", Mathf.Lerp(mouthStart,mouthEnd,timer/_liftDur));
			yield return null;
		}
		Debug.Log("Time to roll out the seeds");
		if(_unlockBird!=null)
		{
			Transform seed = Instantiate(_seedPrefab,_player.position-_player.forward*0.3f,Quaternion.identity);
			UnlockBird(seed);
		}
		Transform mouth=_mouth.transform;
		for(int i=0;i<_numRewards; i++){
			float waitTime=Random.Range(_seedDispenseDelayRange.x,_seedDispenseDelayRange.y);
			yield return new WaitForSeconds(waitTime);
			Transform seed = Instantiate(_seedPrefab,mouth.position+
					new Vector3(Random.Range(0f,mouth.localScale.x*0.5f)*MRandom.RandSign(),
						Random.Range(0f,mouth.localScale.y*0.5f)*MRandom.RandSign(),0),
					Quaternion.identity);
			Sfx.PlayOneShot3D(_dispenseSound,seed.position,Random.Range(0.9f,1.1f));
			Rigidbody rb = seed.GetComponent<Rigidbody>();
			Vector3 forceVector=-mouth.forward*Random.Range(_ejectForceRange.x,_ejectForceRange.y);
			forceVector+=mouth.right*MRandom.RandSign()*Random.value*_ejectForceX;
			forceVector+=Vector3.up*Random.value*_ejectForceY;
			rb.AddForce(forceVector);
		}
	}
}
