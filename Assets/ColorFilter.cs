using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorFilter : MonoBehaviour
{
	public Material _mat;
	public VisionLevel [] _levels;
	int _curLevel;

	[System.Serializable]
	public struct VisionLevel{
		public float _maxHue;
		public float _minHue;
		public Color _subMask;
	}

	void Awake(){
		SetVisionLevel(_curLevel);
	}

	void SetVisionLevel(int level){
		_curLevel=level;
		VisionLevel vl = _levels[_curLevel];
		_mat.SetFloat("_MaxHue",vl._maxHue);
		_mat.SetFloat("_MinHue",vl._minHue);
		_mat.SetColor("_SubMask",vl._subMask);
	}

	public void NextVisionLevel(){
		_curLevel++;
		VisionLevel vl = _levels[_curLevel];
		_mat.SetFloat("_MaxHue",vl._maxHue);
		_mat.SetFloat("_MinHue",vl._minHue);
	}

	public void LerpVisionLevel(int levelA, int levelB, float t01){
		VisionLevel vlA = _levels[levelA];
		VisionLevel vlB = _levels[levelB];
		_mat.SetFloat("_MaxHue",Mathf.Lerp(vlA._maxHue,vlB._maxHue,t01));
		_mat.SetFloat("_MinHue",Mathf.Lerp(vlA._minHue,vlB._minHue,t01));
		_mat.SetColor("_SubMask",Color.Lerp(vlA._subMask,vlB._subMask,t01));
	}

	public void LerpMask(float t01){
		VisionLevel vlA = _levels[_curLevel-1];
		VisionLevel vlB = _levels[_curLevel];
		_mat.SetColor("_SubMask",Color.Lerp(vlA._subMask,vlB._subMask,t01));
	}

	void OnRenderImage(RenderTexture src, RenderTexture dst){
		Graphics.Blit(src,dst,_mat);
	}
}
