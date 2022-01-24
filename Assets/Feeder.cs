using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Feeder : MonoBehaviour
{
	public Transform _seedPrefab;
	public int _numRewards;
	public Vector2 _ejectForceRange;
	public float _ejectForceX;
	public float _ejectForceY;
	public Vector2 _seedDispenseDelayRange;
	public AudioClip _dispenseSound;
	Material _mat;
	public float _liftDur;
	public AudioClip _gearsAudio;

	void Awake(){
		_mat=GetComponent<MeshRenderer>().material;
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void Feed(){
		StartCoroutine(FeedR());
	}

	IEnumerator FeedR(){
		Sfx.PlayOneShot3D(_gearsAudio,transform.position);
		float timer=0;
		float mouthStart=_mat.GetFloat("_OpenAmount");
		float mouthEnd=1f;
		while(timer<_liftDur){
			timer+=Time.deltaTime;
			_mat.SetFloat("_OpenAmount", Mathf.Lerp(mouthStart,mouthEnd,timer/_liftDur));
			yield return null;
		}
		Debug.Log("Time to roll out the seeds");

		Transform mouth=transform;
		for(int i=0;i<_numRewards; i++){
			float waitTime=Random.Range(_seedDispenseDelayRange.x,_seedDispenseDelayRange.y);
			yield return new WaitForSeconds(waitTime);
			Transform seed = Instantiate(_seedPrefab,mouth.position+
					mouth.right*Random.Range(0f,mouth.localScale.x*0.5f)*MRandom.RandSign()+
					mouth.up*Random.Range(0f,mouth.localScale.y*0.5f)*MRandom.RandSign(),
					Quaternion.identity);
			Sfx.PlayOneShot3D(_dispenseSound,seed.position,Random.Range(0.9f,1.1f));
			Rigidbody rb = seed.GetComponent<Rigidbody>();
			Vector3 forceVector=-mouth.forward*Random.Range(_ejectForceRange.x,_ejectForceRange.y);
			forceVector+=mouth.right*MRandom.RandSign()*Random.value*_ejectForceX;
			forceVector+=Vector3.up*Random.value*_ejectForceY;
			rb.AddForce(forceVector);
		}
		GameManager._player.PartySnacks();
	}
}
