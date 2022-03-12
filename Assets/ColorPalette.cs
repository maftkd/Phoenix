﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorPalette : MonoBehaviour
{
	[System.Serializable]
	public struct Palette {
		public string _name;
		[Header("Puzzle box")]
		public Color _puzzleBox;
		public Color _puzzleBoxOutline;
		public float _secondaryMult;
		[Header("Circuitry")]
		public Color _powerOff;
		public Color _powerOn;
		[Header("Cable")]
		public Color _cableOff;
		public Color _cableOn;
		public Material _cable;
		public Material [] _circuitMats;
		public Material _puzzleBoxMat;
		public Material _puzzleBoxSecondary;
		public Material _beaconMat;

		public void UpdateMaterials(){
			foreach(Material m in _circuitMats){
				m.SetColor("_ColorOn",_powerOn);
				m.SetColor("_ColorOff",_powerOff);
			}
			_cable.SetColor("_ColorOn",_cableOn);
			_cable.SetColor("_ColorOff",_cableOff);
			_puzzleBoxMat.SetColor("_Color",_puzzleBox);
			_puzzleBoxMat.SetColor("_OutlineColor",_puzzleBoxOutline);
			if(_puzzleBoxSecondary!=null)
			{
				_puzzleBoxSecondary.SetColor("_Color",_puzzleBox*_secondaryMult);
				_puzzleBoxSecondary.SetColor("_OutlineColor",_puzzleBoxOutline);
			}
			if(_beaconMat!=null)
			{
				_beaconMat.SetColor("_ColorBot",_puzzleBox);
				Color foo=_puzzleBox;
				foo.a=0;
				_beaconMat.SetColor("_ColorTop",foo);
			}
		}
	}

	public Palette [] _palettes;

	[Header("Buttons")]
	public bool _updateColors;
	public bool _autoUpdateColors;


	void OnValidate(){
		if(_updateColors||_autoUpdateColors){
			foreach(Palette p in _palettes)
				p.UpdateMaterials();
			_updateColors=false;
		}
	}

	public Color GetButtonEmissionColor(Transform t){
		Island i = t.GetComponentInParent<Island>();
		if(i==null)
			return Color.magenta;
		foreach(Palette p in _palettes){
			if(i.name==p._name){
				return p._powerOn;
			}
		}
		return Color.black;
	}
}
