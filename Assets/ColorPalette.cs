using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorPalette : MonoBehaviour
{
	[Header("Circuits")]
	public Color _powerOff;
	public Color _powerOn;
	public Material _lockMat;
	public Material _lockBase;
	public Material _circuitMat;
	public Material _buttonIcon;

	public bool _updateColors;
	public bool _autoUpdateColors;

	void OnValidate(){
		if(_updateColors||_autoUpdateColors){
			_lockMat.SetColor("_Color",_powerOff);
			_lockMat.SetColor("_ColorOn",_powerOn);
			_lockBase.SetColor("_Color",_powerOff);
			_lockBase.SetColor("_ColorB",_powerOn);
			_circuitMat.SetColor("_ColorOff",_powerOff);
			_circuitMat.SetColor("_ColorOn",_powerOn);
			_buttonIcon.SetColor("_Color",_powerOff);
			_buttonIcon.SetColor("_ColorB",_powerOn);
			_updateColors=false;
		}
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
