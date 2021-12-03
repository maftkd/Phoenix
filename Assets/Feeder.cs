using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Feeder : MonoBehaviour
{
	public enum FoodType {SEED, WORMS, BERRIES};
	public FoodType _foodType;
	public bool _forage;
	Perch [] _perches;
	[Header("Forage")]
	public float _radius;
    // Start is called before the first frame update
    void Start()
    {
		_perches = transform.GetComponentsInChildren<Perch>();
		if(_forage && GetComponent<MeshRenderer>()!=null)
			GetComponent<MeshRenderer>().enabled=false;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public Perch HasSpace(){
		foreach(Perch p in _perches)
			if(!p._occupied)
				return p;
		return null;
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.blue;
		Gizmos.DrawWireSphere(transform.position,_radius);
	}
}
