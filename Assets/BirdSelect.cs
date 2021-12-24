using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BirdSelect : MonoBehaviour
{
	Animator _anim;
	public Vector2 _idleSpeedRange;
	public GameObject _nameText;
	AudioSource _audio;
	public static BirdSelect _selected;
	[HideInInspector]
	public int _index;
	public float _joyDelay;
	public static float _joyDelayTimer;
	List<BirdSelect> _birds;
	public float _inThresh;
	public Transform _player;
	public Transform _mate;
	public Transform _mainCam;
	public Material _mat;
	public Material _mateMat;
	public AudioClip _call;
	public AudioClip _mateCall;
	void Awake(){
		_anim=GetComponent<Animator>();
		_anim.SetFloat("cycleOffset",Random.value);
		_anim.SetFloat("IdleAgitated",Random.value);
		_anim.SetFloat("IdleSpeed",Random.Range(_idleSpeedRange.x,_idleSpeedRange.y));
		_audio=GetComponent<AudioSource>();
		_index=transform.GetSiblingIndex();
		//#temp have first bird selected by default. Later, maybe random or based on player's previous plays
		if(_index==0)
			Select();
		else
			Deselect();
	}
    // Start is called before the first frame update
    void Start()
    {
		Debug.Log("My index is: "+name+", index: "+_index);
		if(_selected==this)
			Debug.Log("I'm selected on start: "+name);

		BirdSelect [] birdsRaw=FindObjectsOfType<BirdSelect>();
		_birds = new List<BirdSelect>();
		int curIndex=0;
		for(int i=0; i<birdsRaw.Length; i++)
		{
			foreach(BirdSelect bs in birdsRaw){
				if(bs._index==curIndex)
				{
					_birds.Add(bs);
					curIndex++;
					break;
				}
			}
		}
    }

    // Update is called once per frame
    void Update()
    {
		if(_selected==this){
			_joyDelayTimer+=Time.deltaTime;
			if(_joyDelayTimer>=_joyDelay){
				float horIn=Input.GetAxis("Horizontal");
				if(horIn>=_inThresh)
				{
					Debug.Log("huh?");
					if(_index<_birds.Count-1)
					{
						Debug.Log("why?");
						_birds[_index+1].Select();
					}
					else
					{
						Debug.Log("wha?");
						_birds[0].Select();
					}
				}
				else if(horIn<=-_inThresh){
					Debug.Log("whoo?");
					if(_index>0)
						_birds[_index-1].Select();
					else
						_birds[_birds.Count-1].Select();
				}
			}
			if(Input.GetButtonDown("Jump")){
				ConfirmChoice();
			}
		}
    }

	void OnMouseEnter(){
		Select();
	}

	public void Select(){
		Debug.Log("Selecting: "+name);
		//deselect old
		if(_selected!=null)
			_selected.Deselect();
		//select new
		_nameText.SetActive(true);
		_anim.SetTrigger("sing");
		_audio.Play();
		_selected=this;
		_joyDelayTimer=0;
	}


	void OnMouseUpAsButton(){
		ConfirmChoice();
	}

	void OnMouseExit(){
		Deselect();
	}

	public void Deselect(){
		_nameText.SetActive(false);
		Debug.Log("Deselecting: "+name);
		_audio.Stop();
	}

	public void ConfirmChoice(){
		Debug.Log("Chose: "+name);
		foreach(BirdSelect bs in _birds)
			bs.Deselect();
		SkinnedMeshRenderer smr = transform.GetChild(1).GetComponent<SkinnedMeshRenderer>();
		Mesh mesh = smr.sharedMesh;
		//set mesh n materials
		_player.Find("Mesh").GetComponent<SkinnedMeshRenderer>().sharedMesh=mesh;
		_player.Find("Mesh").GetComponent<SkinnedMeshRenderer>().material=_mat;
		_mate.Find("Mesh").GetComponent<SkinnedMeshRenderer>().sharedMesh=mesh;
		_mate.Find("Mesh").GetComponent<SkinnedMeshRenderer>().material=_mateMat;
		//set scale
		_player.localScale=transform.localScale;
		_mate.localScale=transform.localScale;

		//set call
		_player.Find("Call").GetComponent<AudioSource>().clip=_call;
		_mate.Find("Call").GetComponent<AudioSource>().clip=_mateCall;

		//enable gameObjects
		_player.parent.gameObject.SetActive(true);
		//_mate.gameObject.SetActive(true);
		//_mainCam.gameObject.SetActive(true);
		transform.parent.gameObject.SetActive(false);
	}

}
