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
	public int _ringRes;
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
	public Transform _sphere;
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
	public bool _buttonsEnabled;

	void OnValidate(){
		if(!_buttonsEnabled)
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

	void Awake(){
#if UNITY_EDITOR
		//nada
#else
		GenTree();
#endif
	}

	public void GenTree(){
		if(_minRings<2||_sphere==null||_leaf==null||!gameObject.activeInHierarchy)
			return;
		if(_useSeed)
			Random.InitState(_seed);
		_branches = new List<List<Vector3>>();
		_branchLevels = new Dictionary<int,int>();
		GenerateBranch(Vector3.zero,Vector3.zero);//growth dir is determined in GenBranch when level==0

		//todo meshify them branches
		//add some cylinders - maybe temp
		Transform [] children = new Transform[transform.childCount];
		for(int i=0; i<transform.childCount;i++)
			children[i]=transform.GetChild(i);
		StartCoroutine(DestroyNextFrame(children));
		int ringCenters=0;
		foreach(List<Vector3> branch in _branches){
			foreach(Vector3 b in branch){
				ringCenters++;
			}
		}
		Vector3[] vertices = new Vector3[ringCenters*(_ringRes+1)];
		int[] tris = new int[(ringCenters-1)*(_ringRes)*6];
		Vector3[] norms = new Vector3[vertices.Length];
		Vector2[] uvs = new Vector2[vertices.Length];
		Transform ringCenter = new GameObject("ring center").transform;
		int vertexCounter=0;
		int triCounter=0;
		int baseV=0;
		for(int i=0; i<_branches.Count; i++){
			List<Vector3> branch = _branches[i];
			Vector3 dir=Vector3.up;
			int level=_branchLevels[i];
			float widthFactor=1f-level*_branchWidthFactor;
			float radius=_cylinderWidth*widthFactor;
			//float radius=0.1f;
			for(int j=0; j<branch.Count; j++){
				float r01=j/(float)(branch.Count-1);
				if(j<branch.Count-1)
					dir=branch[j+1]-branch[j];
				ringCenter.up=dir;
				ringCenter.position=branch[j];
				for(int n=0;n<=_ringRes;n++){
					float t01 = n/(float)_ringRes;
					float ang=t01*Mathf.PI*2f;
					Vector3 pos=ringCenter.position;
					pos+=ringCenter.right*radius*Mathf.Cos(ang);
					pos+=ringCenter.forward*radius*Mathf.Sin(ang);
					vertices[vertexCounter]=pos;
					norms[vertexCounter]=pos-ringCenter.position;
					uvs[vertexCounter] = new Vector2(t01,r01);
					vertexCounter++;
				}
				if(j==branch.Count-1)
				{
					Transform sphere = Instantiate(_sphere,transform.TransformPoint(ringCenter.position),Quaternion.identity,transform);
					sphere.localScale=Vector3.one*2f*radius;
					if(level==_maxLevel){
						Transform leaf = Instantiate(_leaf,transform);
						leaf.position=sphere.position;

						leaf.rotation=Quaternion.Slerp(Quaternion.identity,Random.rotation,_leafRotation);
						leaf.localScale=new Vector3(Random.Range(_minLeafSize.x,_maxLeafSize.x),
								Random.Range(_minLeafSize.y,_maxLeafSize.y),
								Random.Range(_minLeafSize.z,_maxLeafSize.z));
					}
				}
			}

			for(int j=0;j<branch.Count-1;j++){
				//int baseV=(_ringRes+1)*i;//trust //double trust
				for(int n=0;n<_ringRes;n++){//trust
					//fist tri
					tris[triCounter]=baseV;
					tris[triCounter+2]=baseV+1;
					tris[triCounter+1]=baseV+(_ringRes+1);
					//second tri
					tris[triCounter+3]=baseV+(_ringRes+1);
					tris[triCounter+5]=baseV+1;
					tris[triCounter+4]=baseV+(_ringRes+1)+1;
					triCounter+=6;
					baseV++;
				}
				baseV++;//inc extra vertex cuz there's an extra for uv wrapping
			}
			baseV+=(_ringRes+1);
		}

		Mesh m = new Mesh();
		m.vertices=vertices;
		m.triangles=tris;
		m.normals = norms;
		m.uv=uvs;
		m.RecalculateBounds();
		//_meshF.sharedMesh=m;
		GetComponent<MeshFilter>().sharedMesh=m;

		StartCoroutine(DestroyNextFrame(ringCenter.gameObject));

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
		{
			if(t!=null)
				DestroyImmediate(t.gameObject);
		}
	}

	IEnumerator DestroyNextFrame(GameObject go){
		yield return null;
		DestroyImmediate(go);
	}

	public Vector3 GetRandomPerch(){
		if(_branches==null)
			Debug.Log("oopsies");
		int branchIndex=Random.Range(0,_branches.Count-1);//disclude last branch, that's the trunk
		//Debug.Log("branch index: "+branchIndex);
		List<Vector3> branch = _branches[branchIndex];
		int branchPart=Random.Range(0,branch.Count-1);
		Vector3 a=transform.TransformPoint(branch[branchPart]);
		Vector3 b=transform.TransformPoint(branch[branchPart+1]);
		Vector3 spot=Vector3.Lerp(a,b,Mathf.Lerp(0.25f,1f,Random.value));
		RaycastHit hit;
		if(Physics.Raycast(spot+Vector3.up*0.5f,Vector3.down, out hit, 1f, 1)){
			return hit.point;
		}
		return transform.TransformPoint(branch[Random.Range(0,branch.Count)]);
	}

	public Vector3 GetClosestPerch(Vector3 cur){
		float minDistSqr=100000f;
		int branchIndex=-1;
		int twigIndex=-1;
		Vector3 playerLocal=transform.InverseTransformPoint(cur);
		for(int i=0;i<_branches.Count;i++){
			for(int j=0; j<_branches[i].Count; j++){
				float sqrDist=(_branches[i][j]-playerLocal).sqrMagnitude;
				if(sqrDist<minDistSqr){
					minDistSqr=sqrDist;
					branchIndex=i;
					twigIndex=j;
				}
			}
		}
		if(branchIndex<0||branchIndex==_branches.Count-1)
			return Vector3.zero;
		Vector3 a = Vector3.zero;
		Vector3 b = Vector3.zero;
		List<Vector3> branch = _branches[branchIndex];
		if(twigIndex==0)
		{
			a=branch[0];
			b=branch[1];
		}
		else if(twigIndex==branch.Count-1){
			b=branch[branch.Count-1];
			a=branch[branch.Count-2];
		}
		else{
			Vector3 foo=branch[twigIndex-1];
			Vector3 bar=branch[twigIndex+1];
			float fooDistSqr=(foo-playerLocal).sqrMagnitude;
			float barDistSqr=(bar-playerLocal).sqrMagnitude;
			if(fooDistSqr<barDistSqr){
				a=foo;
				b=branch[twigIndex];
			}
			else{
				a=branch[twigIndex];
				b=bar;
			}
		}
		//frac=ap-ab
		float ab=(a-b).magnitude;
		float ap=(playerLocal-a).magnitude;
		float t01=ap/ab;
		Vector3 spot = transform.TransformPoint(Vector3.Lerp(a,b,t01));
		RaycastHit hit;
		if(Physics.Raycast(spot+Vector3.up*0.5f,Vector3.down, out hit, 1f, 1)){
			return hit.point;
		}
		return Vector3.zero;
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
