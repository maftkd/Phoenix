using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MTree : MonoBehaviour
{
	//trunk
	public int _numRings;
	public float _ringSpacing;
	public bool _useSeed;
	public int _seed;
	public bool _incSeed;
	public bool _decSeed;
	public float _trunkCurve;

	//branch
	public int _branchStart;
	public int _branchSpacing;
	List<List<Vector3>> _branches;
	public int _numBranchRings;
	public float _branchRingSpacing;
	public float _branchCurve;

	//buttons
	public bool _genTree;
	public bool _autoGen;
	List<Vector3> _trunk;

	void OnValidate(){
		if(_incSeed)
		{
			_seed++;
			_incSeed=false;
		}
		if(_decSeed){
			_seed--;
			_decSeed=false;
		}
		if(_genTree||_autoGen){
			_genTree=false;
			GenTree();
		}
	}

	void GenTree(){
		if(_numRings<2||_branchSpacing<1||_numBranchRings<2)
			return;
		if(_useSeed)
			Random.InitState(_seed);
		_trunk = new List<Vector3>();
		_branches = new List<List<Vector3>>();
		Vector3 curPos=Vector3.zero;
		Vector3 offset=Vector3.zero;
		_trunk.Add(curPos);
		for(int i=1;i<_numRings; i++){
			offset+=Vector3.right*(Random.value*2f-1)*_trunkCurve;
			offset+=Vector3.forward*(Random.value*2f-1)*_trunkCurve;
			curPos+=Vector3.up*_ringSpacing;
			curPos+=offset;
			_trunk.Add(curPos);
			if(i>=_branchStart&&(i-_branchStart)%_branchSpacing==0){
				//make new branch
				List<Vector3> branch = new List<Vector3>();
				Vector3 bPos=curPos;
				branch.Add(bPos);
				Vector3 branchGrowth=Vector3.Cross((_trunk[i]-_trunk[i-1]).normalized,Vector3.up);
				Vector3 branchRight=Vector3.Cross(branchGrowth,Vector3.up);
				Vector3 bOffset=Vector3.zero;
				for(int j=1;j<_numBranchRings; j++){
					bOffset+=Vector3.up*Random.value*_branchCurve;
					bOffset+=branchRight*(Random.value*2f-1f)*_branchCurve;
					bPos+=branchGrowth*_branchRingSpacing;
					bPos+=bOffset;
					branch.Add(bPos);
				}
				_branches.Add(branch);
			}
		}

	}

	void OnDrawGizmos(){
		if(_trunk!=null){
			Gizmos.color=Color.red;
			foreach(Vector3 v in _trunk){
				Gizmos.DrawSphere(transform.TransformPoint(v),0.15f);
			}
		}
		if(_branches!=null){
			Gizmos.color=Color.magenta;
			foreach(List<Vector3> b in _branches){
				foreach(Vector3 v in b){
					Gizmos.DrawSphere(transform.TransformPoint(v),0.1f);
				}
			}
		}
	}
}
