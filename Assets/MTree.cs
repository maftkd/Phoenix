using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MTree : MonoBehaviour
{
	List<List<Vector3>> _branches;
	Dictionary<int, int> _branchLevels;
	//trunk
	public int _minRings;
	public int _maxRings;
	public float _baseRingSpacing;
	public bool _useSeed;
	public int _seed;
	public bool _incSeed;
	public bool _decSeed;
	[Range(-.1f,0.5f)]
	public float _seekSun;

	//branch
	[Range(0,1)]
	public float _branchPoint;
	public Vector2 _branchRotate;
	public int _maxLevel;
	public bool _incLevel;
	public bool _decLevel;
	[Range(0,1)]
	public float _branchShorteningFactor;
	[Range(0,1)]
	public float _branchHorizontalFactor;
	public int _branchFactor;
	public bool _incBranch;
	public bool _decBranch;
	public Transform _capsule;
	[Range(0.1f,1)]
	public float _cylinderWidth;
	[Range(0f,1f)]
	public float _branchWidthFactor;
	[Range(1f,2.5f)]
	public float _oversize;
	public Transform _leaf;
	public Vector3 _minLeafSize;
	public Vector3 _maxLeafSize;
	[Range(0f,1f)]
	public float _leafRotation;
	public Material _trunkMat;

	//buttons
	public bool _genTree;
	public bool _autoGen;

	void OnValidate(){
		if(Application.isPlaying&&Time.time<1f)
			return;
		if(_incSeed)
		{
			_seed++;
			_incSeed=false;
		}
		if(_decSeed){
			_seed--;
			_decSeed=false;
		}
		if(_incLevel){
			_maxLevel++;
			_incLevel=false;
		}
		if(_decLevel){
			_maxLevel--;
			_decLevel=false;
		}
		if(_incBranch){
			_branchFactor++;
			_incBranch=false;
		}
		if(_decBranch){
			_branchFactor--;
			_decBranch=false;
		}
		if(_genTree||_autoGen){
			_genTree=false;
			GenTree();
		}
	}

	public void GenTree(){
		if(_minRings<2||_capsule==null||_leaf==null||!gameObject.activeInHierarchy)
			return;
		if(_useSeed)
			Random.InitState(_seed);
		_branches = new List<List<Vector3>>();
		_branchLevels = new Dictionary<int,int>();
		GenerateBranch(Vector3.zero,Vector3.zero);//growth dir is determined in GenBranch when level==0

		//add some cylinders - maybe temp
		Transform [] children = new Transform[transform.childCount];
		for(int i=0; i<transform.childCount;i++)
			children[i]=transform.GetChild(i);
		StartCoroutine(DestroyNextFrame(children));
		//gen cylinders
		int branchIndex=0;
		foreach(List<Vector3> branch in _branches){
			for(int i=1;i<branch.Count;i++){
				int level=_branchLevels[branchIndex];
				Transform cylinder = Instantiate(_capsule,transform);
				Vector3 a = transform.TransformPoint(branch[i-1]);
				Vector3 b = transform.TransformPoint(branch[i]);
				cylinder.position=Vector3.Lerp(a,b,0.5f);
				cylinder.up=(b-a);
				float widthFactor=1f-level*_branchWidthFactor;
				cylinder.localScale=new Vector3(_cylinderWidth*widthFactor,(a-b).magnitude*0.5f*_oversize,_cylinderWidth*widthFactor);
				if(i==branch.Count-1&&level==_maxLevel){
					Transform leaf = Instantiate(_leaf,transform);
					leaf.position=b;

					leaf.rotation=Quaternion.Slerp(Quaternion.identity,Random.rotation,_leafRotation);
					leaf.localScale=new Vector3(Random.Range(_minLeafSize.x,_maxLeafSize.x),
							Random.Range(_minLeafSize.y,_maxLeafSize.y),
							Random.Range(_minLeafSize.z,_maxLeafSize.z));
				}
				cylinder.GetComponent<Renderer>().material=_trunkMat;
			}
			branchIndex++;
		}
	}

	void GenerateBranch(Vector3 startPos, Vector3 growthDir, int level=0){
		List<Vector3> branch = new List<Vector3>();
		if(level==0)//root tree
		{
			Vector2 offsetDir = Random.insideUnitCircle.normalized;
			Vector3 rootOffset=Vector3.down+new Vector3(offsetDir.x,0,offsetDir.y);
			rootOffset.Normalize();
			Vector3 root=startPos+rootOffset*_baseRingSpacing;
			branch.Add(root);
			//get growth dir
			growthDir=-rootOffset;
		}
		//add first node
		branch.Add(startPos);
		//determine spacing
		int numRings=Random.Range(_minRings,_maxRings+1);
		float ringMult=1f-level*_branchShorteningFactor;
		numRings=Mathf.RoundToInt(numRings*ringMult);
		if(numRings<2)
			return;
		float ringSpacing=_baseRingSpacing;
		Vector3 curPos=startPos;
		//determine branch point
		int branchPoint=Mathf.RoundToInt(Mathf.Lerp(1,numRings-1,_branchPoint));
		Vector3 initialGrowthDir=growthDir;
		for(int i=1;i<numRings; i++){
			growthDir=Vector3.Lerp(growthDir,Vector3.up,_seekSun);
			growthDir.Normalize();
			curPos+=growthDir*ringSpacing;
			branch.Add(curPos);
			//check for branch
			if(i==branchPoint&&level<_maxLevel){
				float branchAngle=360f/(_branchFactor+1);
				for(int j=0;j<_branchFactor;j++){
					Vector3 branchDir=Quaternion.Euler(0,branchAngle*(j+1)+Random.Range(_branchRotate.x,_branchRotate.y),0)*initialGrowthDir;
					if(branchDir.x!=0||branchDir.z!=0)
					{
						branchDir.y=Mathf.Lerp(branchDir.y,0,_branchHorizontalFactor);
						branchDir.Normalize();
					}
					GenerateBranch(curPos,branchDir,level+1);
				}
			}
		}
		_branches.Add(branch);
		_branchLevels.Add(_branches.Count-1,level);
	}

	IEnumerator DestroyNextFrame(Transform[] ts){
		yield return null;
		foreach(Transform t in ts)
			DestroyImmediate(t.gameObject);
	}

	void OnDrawGizmos(){
		if(_branches!=null){
			Gizmos.color=Color.magenta;
			foreach(List<Vector3> b in _branches){
				for(int i=0;i<b.Count; i++){
					Gizmos.DrawSphere(transform.TransformPoint(b[i]),0.1f);
					if(i>0){
						Gizmos.DrawLine(transform.TransformPoint(b[i]),transform.TransformPoint(b[i-1]));
					}
					/*
					if(i==b.Count-1)
						Gizmos.DrawSphere(transform.TransformPoint(b[i]),1f);
						*/
				}
			}
		}
	}
}
