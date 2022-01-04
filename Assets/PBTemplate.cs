using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBTemplate : PuzzleBox
{
	protected override void Awake(){
		base.Awake();
		//your code here
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
}
